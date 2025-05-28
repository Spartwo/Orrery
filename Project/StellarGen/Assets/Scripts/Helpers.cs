using System;
using System.Collections.Generic;
using System.IO;
using SystemGen;
using Newtonsoft.Json;
using UnityEngine;
using Random = System.Random;
using Models;
using UnityEditor;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Linq;

namespace StellarGenHelpers
{
    public static class JsonUtils
    { 
        // Settings that include type names in the JSON
        private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = new DefaultContractResolver
            {
                // Include public AND non-public instance members
                DefaultMembersSearchFlags =
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            }
        };

        /// <summary>
        /// Serializes a given object to a JSON file.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="data">The object to serialize.</param>
        /// <param name="filePath">The path of the file to save the JSON data.</param>
        public static void SerializeToJsonFile<T>(T data, string filePath)
        {
            string json = JsonConvert.SerializeObject(data, _settings);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Deserializes a JSON file into an object.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="filePath">The path of the JSON file.</param>
        /// <returns>The deserialized object, or default if the file does not exist.</returns>
        public static T DeserializeFromJsonFile<T>(string filePath)
        {
            if (!File.Exists(filePath))
                return default;

            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<T>(json, _settings);
        }

        /// <summary>
        /// Serializes a given list to a JSON file.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <param name="list">The list to serialize.</param>
        /// <param name="filePath">The path of the file to save the JSON data.</param>
        public static void SerializeListToJsonFile<T>(List<T> list, string filePath)
        {
            SerializeToJsonFile(list, filePath);
        }

        /// <summary>
        /// Deserializes a JSON file into an object.
        /// </summary>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <param name="filePath">The path of the JSON file.</param>
        /// <returns>Deserialized object of type T.</returns>
        public static T DeserializeJsonFromFile<T>(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"JsonUtils: File not found - {filePath}");
                return default;
            }

            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<T>(json, _settings);
        }

        /// <summary>
        /// Deserializes a JSON file into a list.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <param name="filePath">The path of the JSON file.</param>
        /// <returns>A list of deserialized objects, or an empty list if the file does not exist.</returns>
        public static List<T> DeserializeListFromJsonFile<T>(string filePath)
        {
            if (!File.Exists(filePath))
                return new List<T>();

            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<T>>(json) ?? new List<T>();
        }
        
        /// <summary>
        /// Reads the given line number from a file and extracts the JSON string value.
        /// </summary>
        /// <param name="filePath">Path to the .loc (or any JSON) file.</param>
        /// <param name="lineNumber">1-based line index to read.</param>
        public static string ReadJsonValueAtLine(string filePath, int lineNumber)
        {
            if (lineNumber < 1)
            {
                Logger.LogError("JsonUtils", $"Line number must be greater than 0. Provided: {lineNumber}");
                return "";
            }

            using var sr = new StreamReader(filePath);
            string line = null;

            for (int i = 1; i <= lineNumber; i++)
            {
                line = sr.ReadLine();
                if (line == null)
                    return "";
            }

            // Extract the colon position
            int colonIdx = line.IndexOf(':');
            if (colonIdx < 0)
                return "";

            // Get the substring after the colon
            string after = line.Substring(colonIdx + 1).Trim();
            if (after.EndsWith(","))
                after = after.Substring(0, after.Length - 1).Trim();
            if (after.Length >= 2 && after[0] == '"' && after[^1] == '"')
                return after.Substring(1, after.Length - 2);

            return after;
        }
        public static SystemProperties Load(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            // 1) Read the raw JSON
            var json = File.ReadAllText(filePath);
            var jo = JObject.Parse(json);

            // 2) Extract the seed and age (or any other simple fields)
            string seed = jo["Seed"]?.Value<string>();
            decimal age = jo["System Age (bYo)"]?.Value<decimal>() ?? 0m;

            // 3) Create your sys-props with the right ctor
            var system = new SystemProperties(seed);
            system.systemAge = age;

            // 4) Prepare a populator that will fill private members too
            var popSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    DefaultMembersSearchFlags =
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                },
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            };

            // 5) Populate the lists
            PopulateList<StarProperties>(jo, "Stellar Bodies", popSettings, system.stellarBodies);
            PopulateList<BodyProperties>(jo, "Solid Bodies", popSettings, system.solidBodies);
            PopulateList<BeltProperties>(jo, "Belts/Rings", popSettings, system.belts);

            return system;
        }

        private static void PopulateList<T>(
            JObject source,
            string arrayName,
            JsonSerializerSettings settings,
            List<T> targetList)
        {
            targetList.Clear();
            if (!(source[arrayName] is JArray arr))
                return;

            // Create a JsonSerializer using your custom settings
            var serializer = JsonSerializer.Create(settings);

            // Let JSON.NET materialize the List<T> in one go
            List<T> list = arr.ToObject<List<T>>(serializer);

            // If it succeeded, copy into your existing list
            if (list != null)
                targetList.AddRange(list);
        }

        [Serializable]
        private class JsonWrapper<T>
        {
            public T[] Content;
        }
    }

    public static class RandomUtils
    {
        // Default Random object when no seed is provided
        private static Random defaultRandom = new Random();

        /// <summary>
        /// Tweaks a seed value to try avoid generation overlap
        /// </summary>
        /// <param name="seedValue">The seed to be changed</typeparam>
        /// <returns>A new sligtly randomised seed value.</returns>
        public static int TweakSeed(int seedValue)
        {
            // Distinguish seed value from other generated bodies
            float divisor = RandomUtils.RandomFloat(1f, 10f, seedValue);
            return (int)Math.Round(seedValue / divisor);
        }

        /// <summary>
        /// Generates a random decimal number between two given numbers
        /// </summary>
        /// <param name="max">The highest possible number</typeparam>
        /// <param name="min">The lowest possible number</typeparam>
        /// <param name="seed">(Optional) The seed value for the random generation</param>
        /// <returns>A Float.</returns>
        public static float RandomFloat(float min, float max, int? seed = null)
        {
            Random random = seed.HasValue ? new Random(seed.Value) : defaultRandom;
            return (float)(random.NextDouble() * (max - min) + min);
        }

        /// <summary>
        /// Generates a random whole number between two given numbers
        /// </summary>
        /// <param name="max">The highest possible number</typeparam>
        /// <param name="min">The lowest possible number</typeparam>
        /// <param name="seed">(Optional) The seed value for the random generation</param>
        /// <returns>An Integer.</returns>
        public static int RandomInt(int min, int max, int? seed = null)
        {
            Random random = seed.HasValue ? new Random(seed.Value) : defaultRandom;
            return random.Next(min, max);
        }

        /// <summary>
        /// Generates a random colour using rgb
        /// </summary>
        /// <param name="seed">(Optional) The seed value for the random generation</param>
        /// <returns>A Unity Framework Color</returns>
        public static Color RandomColor(int? seed = null)
        {
            Random random = seed.HasValue ? new Random(seed.Value) : new Random();

            // Generate random values for RGB components
            float r = (float)random.NextDouble();
            float g = (float)random.NextDouble();
            float b = (float)random.NextDouble();

            // Create a Color object in Unity
            return new Color(r, g, b);  
        }

        /// <summary>
        /// Generates a seed input value / system file name
        /// </summary>
        /// <returns>An input string of the systems seed</returns>
        public static string GenerateSystemName()
        {
            // Import list from a plaintext file
            string nameGenerationFilePath = $"{Application.streamingAssetsPath}/Localisation/system-names.loc";
            List<string> systemNameArray = File.ReadAllLines(nameGenerationFilePath).ToList();

            // Choose a random row in the file
            Random random = new Random();
            string randName = systemNameArray[random.Next(1, (systemNameArray.Count))];
            int randNumber = random.Next(1, 1000);

            // Format the output as [RandName RandNumber]
            Logger.Log("RandomUtils", $"System Name: {randName}-{randNumber}");
            return $"{randName}-{randNumber}";
        }
    }

    // Folks get confused if theres a U
    public static class ColourUtils
    {
        /// <summary>
        /// Converts user RGB values to a unity colour
        /// </summary>
        /// <param name="r">Red Channel 0-255</param>
        /// <param name="g">Green Channel 0-255</param>
        /// <param name="b">Blue Channel 0-255</param>
        /// <returns>A Unity Framework Color</returns>
        public static Color RGBtoColor(int r, int g, int b)
        {
            return new Color(r/255f, g/255f, b/255f);
        }

        /// <summary>
        /// Converts an array of RGB floats into a Unity Color
        /// If the alpha value is not provided, it defaults to 1 (fully opaque).
        /// </summary>
        /// <param name="rgb">An array of floats representing the RGB values of the color. Should have a length of 3.</param>
        /// <param name="a">An optional alpha value (between 0 and 1), defaults to 1 (fully opaque) if not provided.</param>
        /// <returns>A Unity Color object created from the RGB values.</returns>
        public static Color ArrayToColor(int[] rgb, int? a = null)
        {
            // If 'a' is not provided (null), default to 255 alpha
            float alpha = a ?? 255f;
            // Return a color made from the RGB elements of the array
            return new Color(rgb[0]/255f, rgb[1]/255f, rgb[2]/255f, alpha/255);
        }

        /// <summary>
        /// Converts a Unity Color object into an array of floats representing the RGB channels.
        /// </summary>
        /// <param name="c">The Unity Color object to be converted.</param>
        /// <returns>An array of floats representing the RGB channels of the color, in the order [r, g, b].</returns>
        public static int[] ColorToArray(Color c)
        {
            int[] rgb = new int[3];
            // Set the array to the RGB channels of the color
            rgb[0] = (int)(c.r * 255);
            rgb[1] = (int)(c.g * 255);
            rgb[2] = (int)(c.b * 255);
            // Return the now populated array
            return rgb;
        }

        /// <summary>
        /// Converts a hexadecimal color string (e.g., "#RRGGBB" or "#RRGGBBAA") to a Unity Color.
        /// </summary>
        /// <param name="hex">The hexadecimal string representing the color.</param>
        /// <returns>A Unity Color object.</returns>
        public static Color HexToColor(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                throw new ArgumentException("Hex string cannot be null or empty.");

            // Remove the '#' if present
            hex = hex.TrimStart('#');

            if (hex.Length != 6 && hex.Length != 8)
                throw new ArgumentException("Hex string must be 6 (RGB) or 8 (RGBA) characters long.");

            // Parse the color components
            byte r = Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = Convert.ToByte(hex.Substring(4, 2), 16);
            byte a = hex.Length == 8 ? Convert.ToByte(hex.Substring(6, 2), 16) : (byte)255;

            // Normalize to 0-1 range for Unity Color
            return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
        }

        /// <summary>
        /// Converts a Unity Color to a hexadecimal string (e.g., "#RRGGBBAA").
        /// </summary>
        /// <param name="color">The Unity Color object.</param>
        /// <returns>A hexadecimal color string.</returns>
        public static string ColorToHex(Color color)
        {
            byte r = (byte)(color.r * 255);
            byte g = (byte)(color.g * 255);
            byte b = (byte)(color.b * 255);
            byte a = (byte)(color.a * 255);

            return $"#{r:X2}{g:X2}{b:X2}{a:X2}";
        }
    }

    public static class PhysicsUtils
    {
        /// <summary>
        /// Converts the given temperature into a color by sampling a gradient image.
        /// </summary>
        /// <param name="temperature">The temperature of the star in Kelvin.</param>
        /// <returns>A System.Drawing.Color sampled from the gradient based on the given temperature.</returns>
        public static Color DetermineSpectralColor(int temperature)
        {
            // Load the PNG file as a byte array
            byte[] fileData = File.ReadAllBytes($"{Application.streamingAssetsPath}/Assets/Gradient.png");

            Texture2D texture = new Texture2D(1000, 1);
            if (!texture.LoadImage(fileData))
            {
                Logger.LogError("PhysicsUtils", "Failed to load PNG file.");
                return Color.magenta;
            }

            // Extract the width of the image
            int imageWidth = texture.width;

            // Validate the temperature input incase of manual mass edits
            if (temperature < 1000 || temperature > 11000)
            {
                Logger.LogWarning("PhysicsUtils", "Temperature out of expected range (1000K to 11000K). Clamping to valid range.");
                temperature = Math.Clamp(temperature, 1000, 11000);
            }

            // Scale the temperature to the image index range (0 to imageWidth - 1)
            int pixelIndex = (int)((temperature - 1000f) / (11000f - 1000f) * (imageWidth - 1));

            // Extract all pixels from the texture
            Color[] colors = texture.GetPixels();

            // Return the corresponding color
            return colors[pixelIndex];
        }

        /// <summary>
        /// Calcualtes the ambient temperature at a given distance from a star using the Stefan-Boltzmann law.
        /// </summary>
        /// <param name="star">The star object containing its properties.</param>
        /// <param name="orbit">The orbital properties of the body.</param>
        /// <returns>A kelvin temperature in short format</returns>
        public static short CalculateBodyTemperature(StarProperties star, OrbitalProperties orbit)
        {
            // Calculate the distance from the star in AU
            float distance = ConvertToAU(orbit.SemiMajorAxis);
            // Calculate the temperature
            //float temperature = (float)Math.Sqrt(star.BaseLuminosity / (4 * Mathf.PI * Mathf.Pow(distance, 2)));
            short temperature = (short)(279.25f * Math.Pow(star.BaseLuminosity / Math.Pow(distance, 2), 0.25f));
            return temperature;
        }

        /// <summary>
        /// Lets you run power operations on decimal datatypes.
        /// </summary>
        public static decimal DecimalPow(decimal baseValue, decimal exponent)
        {
            // Handle the case for exponent == 0
            if (exponent == 0)
                return 1m;

            // Handle positive exponents
            decimal result = 1m;
            for (int i = 0; i < (int)exponent; i++)
            {
                result *= baseValue;
            }

            // Handle fractional exponents if necessary
            return result;
        }

        /// <summary>
        /// Checks whether a body's orbit is stable within the sphere of its parent
        /// Hill sphere minimum is stored but this is for live checks
        /// </summary>
        /// <param name="A">The body for which the Hill Sphere is being calculated.</param>
        /// <param name="B">The body for which is being orbited.</param>
        /// <param name="distance">The distance between the checked body</param>
        /// <returns>True if the orbit is stable, otherwise false.</returns>
        public static bool CheckOrbit(BaseProperties A, BaseProperties B, decimal distance)
        {
            // Calculate the semi-minor axis using the formula b = a⋅/1−e^2
            decimal semiMinorAxis = (A.Orbit.SemiMajorAxis * (decimal)Math.Sqrt(1 - Math.Pow(A.Orbit.Eccentricity, 2)));
            // Get the mimimum hill sphere of the parent body around the grandparent
            decimal hillSphere = CalculateHillSphere(A, B, semiMinorAxis);

            // True if the semi-major axis is within the hillsphere of the parent
            return distance < hillSphere;
        }

        /// <summary>
        /// Calculates the size of a body's stable "sphere of influence" in metres by comparing
        /// the masses of the body and its parent. 
        /// </summary>
        /// <param name="A">The body for which the Hill Sphere is being calculated.</param>
        /// <param name="B">The body for which is being orbited.</param>
        /// <param name="distance">The distance between the body and its parent in m</param>
        /// <returns>The radius of the body's Hill Sphere</returns>
        public static decimal CalculateHillSphere(BaseProperties A, BaseProperties B, decimal distance)
        {
            return distance * DecimalPow(B.Mass / (3 * A.Mass), (1/3));
        }

        /// <summary>
        /// Calculates if a given object is within the Roche limit of its parent body.
        /// </summary>
        /// <param name="A">The body for which the roche is being calculated.</param>
        /// <param name="B">The body for which is being orbited.</param>
        /// <param name="distance">The distance between the body and its parent in mm</param>
        /// <returns>The radius of the body's Hill Sphere</returns>
        public static decimal CalculateRoche(BodyProperties A, BodyProperties B, decimal distance)
        {
            float rocheCoefficient = A.Composition.CalculateRocheCoefficient();

            decimal rocheLimitMeters = (decimal)(B.Radius * rocheCoefficient * Math.Pow(B.Composition.CalculateDensity() / A.Composition.CalculateDensity(), 1f / 3f));

            return rocheLimitMeters;
        }

        /// <summary>
        /// Converts an SMA value in AU to a metre value
        /// </summary>
        public static decimal ConvertToMetres(float SMAInput)
        {
            try
            {
                return (decimal)(SMAInput) * PhysicalConstants.AU_TO_METERS;
            }
            catch (Exception ex)
            {
                Logger.LogError("Helpers", $"ConvertToMetres: An error occurred during conversion. {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Converts an SMA value in metres to AU
        /// </summary>
        public static float ConvertToAU(decimal SMAInput)
        {
            try
            {
                return (float)(SMAInput / PhysicalConstants.AU_TO_METERS);
            }
            catch (Exception ex)
            {
                Logger.LogError("Helpers", $"ConvertToAU: An error occurred during conversion. {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Converts a SMA value to physical worldspace
        /// </summary>
        public static float GetWorldDistance(float au, bool logScale, float scale)
        {
            if (logScale)
            {
                return Mathf.Log10(au + 1f) / Mathf.Log10(2f) * scale;
            }
            else
            {
                return au * scale;
            }
        }

        /// <summary>
        /// Converts a sol mass to standard values
        /// </summary>
        public static decimal SolMassToRaw(float solMass)
        {
            try
            {
                return (decimal)(solMass * PhysicalConstants.SOLAR_MASS);
            }
            catch (Exception ex)
            {
                Logger.LogError("Helpers", $"SolMassToRaw: An error occurred during conversion. {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Converts standard mass values to sol mass
        /// </summary>
        public static float RawToSolMass(decimal RawMass)
        {
            try
            {
                return (float)((double)RawMass / PhysicalConstants.SOLAR_MASS);
            }
            catch (Exception ex)
            {
                Logger.LogError("Helpers", $"RawToSolMass: An error occurred during conversion. {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Converts a earth mass to standard values
        /// </summary>
        public static decimal EarthMassToRaw(float earthMass)
        {
            try
            {
                return (decimal)(earthMass * PhysicalConstants.EARTH_MASS);
            }
            catch (Exception ex)
            {
                Logger.LogError("Helpers", $"EarthMassToRaw: An error occurred during conversion. {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Converts standard mass values to earth masses
        /// </summary>
        public static float RawToEarthMass(decimal RawMass)
        {
            try
            {
                return (float)((double)RawMass / PhysicalConstants.EARTH_MASS);
            }
            catch (Exception ex)
            {
                Logger.LogError("Helpers", $"RawToEarthMass: An error occurred during conversion. {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Constructs the orbital properties of a celestial body based on given parameters and 
        /// randomized variations within realistic constraints.
        /// </summary>
        /// <param name="seedValue">The random seed used for deterministic orbit generation.</param>
        /// <param name="SMAInput">The initial semi-major axis (SMA) in astronomical units (AU).</param>
        /// <param name="MeanEccentricity">
        /// The mean eccentricity used as a basis for generating the orbital eccentricity.
        /// </param>
        /// <param name="MaxInclination">
        /// The maximum inclination angle (degrees) allowed for the orbit.
        /// </param>
        /// <returns>
        /// An <see cref="OrbitalProperties"/> object containing the body's semi-major axis,
        /// eccentricity, longitude of ascending node, inclination, and argument of periapsis.
        /// </returns>
        public static OrbitalProperties ConstructOrbitProperties(int seedValue, float SMAInput, float MeanEccentricity, float MaxInclination)
        {
            // Jiggle the orbit a little within a permissible range
            decimal semiMajorAxis = (decimal)(SMAInput + (RandomUtils.RandomFloat(-0.03f, 0.03f, seedValue) * SMAInput)) * PhysicalConstants.AU_TO_METERS;

            // Generate Eccentricity with a basis in the mean solar eccentricity
            // Mean is generated by shared SOI and isn't the actual mean
            float eccentricity = RandomUtils.RandomFloat(0.001f, MeanEccentricity * 2, seedValue);

            // Generate the angle where the orbit goes from below the equator to above it
            float longitudeOfAscending = RandomUtils.RandomFloat(0f, 359f, seedValue);

            // Generate the inclination of the body in a given range
            float inclination = RandomUtils.RandomFloat(0f, MaxInclination, seedValue);

            // Generate how far in the inclined disk the body will set its periapsis
            float periArgument = RandomUtils.RandomFloat(0f, 359f, seedValue);

            // Create new OrbitData Instance
            OrbitalProperties orbit = new OrbitalProperties(semiMajorAxis, eccentricity, longitudeOfAscending, inclination, periArgument);

            return orbit;
        }

        internal static OrbitalProperties ConstructOrbitProperties(decimal v1, float v2, float v3, float v4, float v5)
        {
            throw new NotImplementedException();
        }
    }
}
