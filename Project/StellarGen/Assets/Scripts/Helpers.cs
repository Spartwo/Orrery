using System;
using System.Collections.Generic;
using System.IO;
using SystemGen;
using Newtonsoft.Json;
using UnityEngine;
using Random = System.Random;
using static UnityEngine.Rendering.DebugUI;

namespace StellarGenHelpers
{
    public static class JsonUtils
    { 
        /// <summary>
        /// Serializes a given list to a JSON file.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <param name="list">The list to serialize.</param>
        /// <param name="filePath">The path of the file to save the JSON data.</param>
        public static void SerializeListToJsonFile<T>(List<T> list, string filePath)
        {
            string json = JsonConvert.SerializeObject(list, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Deserializes a JSON file into a list.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <param name="filePath">The path of the JSON file.</param>
        /// <returns>A list of deserialized objects.</returns>
        public static string ToJson<T>(T[] array, bool prettyPrint)
        {
            JsonWrapper<T> wrapper = new JsonWrapper<T>();
            wrapper.Content = array;
            return JsonUtility.ToJson(wrapper, prettyPrint);
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
            return random.Next(min, max + 1);
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
    }

    // Folks get confused if theres a U
    public static class ColorUtils
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
            // If 'a' is not provided (null), default to 1f
            float alpha = a ?? 1f;
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
        /// Calculates the size of a body's stable "sphere of influence" in astronomical units (AU) by comparing
        /// the masses of the body and its parent. 
        /// </summary>
        /// <param name="A">The body for which the Hill Sphere is being calculated.</param>
        /// <param name="distance">The distance between the body and its parent in AU.</param>
        /// <returns>The radius of the body's Hill Sphere in AU.</returns>
        public static float CalculateHillSphere(Body A, float distance)
        {
            Body B = FindParent(A.Parent);

            return distance * (float)(B.Mass / (3 * A.Mass));
        }

        /// <summary>
        /// Finds the parent body of a given body using the parent's seed value.
        /// </summary>
        /// <param name="key">The unique identifier (SeedValue) of the parent body.</param>
        /// <returns>The parent body associated with the given SeedValue, or the barycenter body if not found.</returns>
        public static Body FindParent(int key)
        {
            // Iterate through the stellarBodies to find parent, not so many elements to require 3d
            foreach (Body body in SystemGenerator.stellarBodies)
            {
                if (key == body.SeedValue)
                {
                    return body;
                }
            }

            // Return the barycentre if no parent expressely declared
            return SystemGenerator.stellarBodies[0];
        }

        /// <summary>
        /// Converts the given temperature into a color by sampling a gradient image.
        /// </summary>
        /// <param name="temperature">The temperature of the star in Kelvin.</param>
        /// <returns>A System.Drawing.Color sampled from the gradient based on the given temperature.</returns>
        public static Color DetermineSpectralColor(int temperature)
        {
            // Load the PNG file as a byte array
            byte[] fileData = File.ReadAllBytes("Assets/Materials/gradient.png");


            Texture2D texture = new Texture2D(2, 2);
            if (!texture.LoadImage(fileData))
            {
                Debug.LogError("Failed to load PNG file.");
                return Color.magenta;
            }

            // Extract the width of the image
            int imageWidth = texture.width;

            // Validate the temperature input incase of manual mass edits
            if (temperature < 1000 || temperature > 11000)
            {
                Debug.LogWarning("Temperature out of expected range (1000K to 11000K). Clamping to valid range.");
                temperature = Math.Clamp(temperature, 1000, 11000);
            }

            // Scale the temperature to the image index range (0 to imageWidth - 1)
            int pixelIndex = (temperature - 1000) / (11000 - 1000) * (imageWidth - 1);

            // Extract all pixels from the texture
            Color[] colors = texture.GetPixels();

            // Return the corresponding color
            return colors[pixelIndex];

        }

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
    }
}
