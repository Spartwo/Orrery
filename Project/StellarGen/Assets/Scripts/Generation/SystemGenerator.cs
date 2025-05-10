using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Random = System.Random;
using RandomUtils = StellarGenHelpers.RandomUtils;
using PhysicsUtils = StellarGenHelpers.PhysicsUtils;
using ColourUtils = StellarGenHelpers.ColourUtils;
using JsonUtils = StellarGenHelpers.JsonUtils;
using System;
using Models;
using Settings;
using StellarGenHelpers;
using Unity.VisualScripting;
using Newtonsoft.Json;
using UnityEngine.Rendering.VirtualTexturing;

namespace SystemGen
{
    public class SystemGenerator : MonoBehaviour
    {
        private readonly string assetsFolder = Application.streamingAssetsPath;
        [SerializeField] private string seedInput;
        public SystemProperties systemProperties;

        void Start()
        {
            //Currently instantly try to generate
            if (GlobalSettings.GenerateOnStartup) { StartGeneration(seedInput); }
        }

        /// <summary>
        /// Starts the system generation process asynchronously.
        /// </summary>
        public void StartGeneration(string seedInput)
        {
            this.seedInput = seedInput;
            systemProperties = new SystemProperties(seedInput);
            // Start the generation process
            GenerateSystem().ConfigureAwait(false);
        }

        /// <summary>
        /// The Main Function, goes through the steps to produce a stellar system json file using helper functions
        /// </summary>
        private async Task GenerateSystem()
        {
            // Create folder on initialisation incase of total deletion
            Directory.CreateDirectory($"{assetsFolder}/Star_Systems/");

            Debug.Log("Seed: " + seedInput);

            // Convert alphanumeric seed to a usable integer
            int usableSeed = systemProperties.seedInput.GetHashCode();

            Logger.Log("System Generation", $"Seed: {systemProperties.seedInput} / {usableSeed}");

            // Kick on a generator for quantity of stars.
            int starCount = DetermineStarCount(usableSeed);
            Logger.Log("System Generation", "Stars: " + starCount);

            // Define the stellar systems total age, generation occurs at 0bY
            decimal systemAge = (decimal)Math.Min(Math.Round(await GenerateStellarBodies(usableSeed, starCount), 3), 13.5f);
            // Set a Current age for the system
            AssignAges(systemAge);

            // This section is the important bit, drives the layered generation processes from stellar orbits to body positions
            if (starCount > 1)
            {
                // Generate the orbits of the stars
                await PositionStars(usableSeed, starCount);
                // Call Planet Generation in the stars
                await GenerateMajorBodies(usableSeed);
                // Generate minor bodies with no children of their own
                await GenerateMinorBodies(usableSeed);
                // Check for ejected bodies(P-type orbits)
                await MoveEjectedBodies();
            } 
            else
            {
                // Call Planet Generation in the stars
                await GenerateMajorBodies(usableSeed);
                // Generate minor bodies with no children of their own
                await GenerateMinorBodies(usableSeed);
            }

            // Set Orbit Lines
            await AssignColours();

            // Determine Names in the current configuration
            await AssignNames();

            // Set a Current age for the system
            AssignAges(systemAge);

            // Save the System Data Arrays
            CreateSystemFile();
        }

        private async Task<float> GenerateStellarBodies(int seed, int starCount)
        {
            float systemAge;

            if (starCount == 0)
            {
                systemAge = RandomUtils.RandomFloat(0.25f, 10.5f, seed);
                await Task.Yield();
            }
            else
            {
                float shortestLifespan = 10.5f;
                for (int i = 0; i < starCount; i++)
                {
                    // Generate a new star but only get the properties via StarProperties
                    StarProperties newStarProperties = StarGen.Generate(seed + i);

                    // Roll back all the stars to the youngest
                    shortestLifespan = Math.Min((float)shortestLifespan, (float)newStarProperties.Lifespan);

                    // Add the generated properties to the stellarBodies list
                    systemProperties.stellarBodies.Add(newStarProperties);

                    // Every star, yield execution to prevent blocking
                    await Task.Yield();
                }
                systemAge = RandomUtils.RandomFloat(0.25f, shortestLifespan, seed);
            }
            return systemAge;
        }

        /// <summary>
        /// Sets the orbital parameters of the stars in the system before planet generation.
        /// Stars at medium distances(3-50AU) don't produce planets. These are filtered.
        /// </summary>
        /// <param name="starCount">The number of stars in the system.</param>
        private async Task PositionStars(int seed, int starCount)
        {
            Logger.Log(GetType().Name, "Setting Stellar Positions");

            if (starCount < 2)
            {
                systemProperties.stellarBodies[0].Orbit = new OrbitalProperties(
                    0,
                    0f,
                    0f,
                    0f,
                    0f
                );
                return; // No need to position stars if there's only one
            }

            // Binary star separation log distribution
            double muAB = 3.059;
            double sigmaAB = 0.6;

            double u1 = 1.0 - RandomUtils.RandomFloat(0f, 1f, seed);
            double u2 = 1.0 - RandomUtils.RandomFloat(0f, 1f, seed);

            double standardNormalAB = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
            double log10_r = muAB + sigmaAB * standardNormalAB;
            float distanceAB = (float)Math.Pow(10, log10_r);

            if (starCount == 3)
            {
                // Ternary stars are so far away that there is little point in modeling them but we will note their existence

                // Generate P-type separation for the third star
                double muC = 4.7;      
                double sigmaC = 0.7;

                double uc1 = 1.0 - RandomUtils.RandomFloat(0f, 1f, seed);
                double uc2 = 1.0 - RandomUtils.RandomFloat(0f, 1f, seed);

                double standardNormalC = Math.Sqrt(-2.0 * Math.Log(uc1)) * Math.Cos(2.0 * Math.PI * uc2);
                double log10_rC = muC + sigmaC * standardNormalC;
                float distanceC = (float)Math.Pow(10, log10_rC); // Log-normal C star distance
                float eccentricityC = RandomUtils.RandomFloat(0f, 0.5f, seed);

                Logger.Log(GetType().Name, $"ABC Trinary Spacing: \nAB:{distanceAB}AU\nC:{distanceC}AU");

                /*
                // Assign orbit to the third star
                stellarBodies[2].Orbit = PhysicsUtils.ConstructOrbitProperties(
                    seed,
                    cDistance,
                    cEccentricity,
                    45f
                );
                */
            } 
            else
            {
                Logger.Log(GetType().Name, $"AB Binary Spacing: {distanceAB}AU");
            }

            // Generate eccentricity for the binary stars
            float eccentricityAB = RandomUtils.RandomFloat(0f, (float)(distanceAB * 0.0017f), seed);
            // Assign orbits to the first two stars
            // Semi-Major Axis is proportional to the mass of each body
            decimal totalMass = systemProperties.stellarBodies[0].Mass + systemProperties.stellarBodies[1].Mass;
            float distanceA = distanceAB * (float)(systemProperties.stellarBodies[1].Mass / totalMass);
            float distanceB = distanceAB * (float)(systemProperties.stellarBodies[0].Mass / totalMass);

            // Assign orbit to the first star
            systemProperties.stellarBodies[0].Orbit = new OrbitalProperties(
                PhysicsUtils.ConvertToMetres(distanceA),
                eccentricityAB,
                0f,
                0f,
                0f
            );

            // Assign orbit to the second star
            systemProperties.stellarBodies[1].Orbit = new OrbitalProperties(
                PhysicsUtils.ConvertToMetres(distanceB),
                eccentricityAB,
                180f,
                0f,
                0f
            );
        }

        private void CalculateDeadZone(out float closeMax, out float farMin)
        {
            if (systemProperties.stellarBodies.Count == 1)
            {
                // Now you can access StarProperties-specific members
                float mass1 = systemProperties.stellarBodies[0].StellarMass;
                float mass2 = systemProperties.stellarBodies[0].StellarMass;

                // Perform calculations for closeMax and farMin based on the star's properties
                closeMax = (float)(mass1 * 0.1f); // Example calculation
                farMin = (float)(mass1 * 0.5f); // Example calculation
            }
            else
            {
                Logger.LogWarning("System Generation", "Only One Star is Present");
                closeMax = 0f;
                farMin = 0f;
            }

        }

        /// <summary>
        /// Sets the ages of all bodies to one shared value
        /// </summary>
        private void AssignAges(decimal systemAge)
        {
            Logger.Log("System Generation", $"System Age: {systemAge}bY");
            // Set the system age
            systemProperties.systemAge = systemAge;

            // Assign the system age to each body
            foreach (StarProperties body in systemProperties.stellarBodies)
            {
                body.Age = systemAge;
                body.GenerateAgedStarProperties();
            }
            foreach (BodyProperties body in systemProperties.solidBodies)
            {
                body.Age = systemAge;
            }
            foreach (BeltProperties body in systemProperties.belts)
            {
                body.Age = systemAge;
            }
        }

        /// <summary>
        /// Recursively assigns names to stars, planets, and moons based on their mass and parent star.
        /// This method is called after the generation process to ensure all bodies are properly named.
        /// Stars are named A, B, C, etc.; planets are named Aa, Ab, Ac, etc.; moons are named Aa-1, Aa-2, etc.
        /// </summary>
        private async Task AssignNames()
        {
            Logger.Log("System Generation", "Generating Body Names");
            // Sort root stellar bodies, larger stars get the first letters
            systemProperties.stellarBodies.Sort((a, b) => b.Mass.CompareTo(a.Mass));
            char rootSuffix = 'A';

            foreach (StarProperties star in systemProperties.stellarBodies)
            {
                if (!star.CustomName)
                {
                    star.Name = $"{systemProperties.seedInput}-{rootSuffix}";
                }

                // Find planets orbiting this star
                var planets = systemProperties.solidBodies
                    .Where(p => p.Parent == star.SeedValue)
                    .OrderByDescending(p => p.Mass)
                    .ToList();

                char planetSuffix = 'a';
                foreach (BodyProperties planet in planets)
                {
                    if (!planet.CustomName)
                    {
                        planet.Name = $"{star.Name}{planetSuffix}";
                    }

                    // Find moons of this planet
                    var moons = systemProperties.solidBodies
                        .Where(m => m.Parent == planet.SeedValue)
                        .OrderByDescending(m => m.Mass)
                        .ToList();

                    for (int i = 0; i < moons.Count; i++)
                    {
                        if (!moons[i].CustomName)
                        {
                            moons[i].Name = $"{planet.Name}-{i + 1}";
                        }
                    }

                    planetSuffix++;
                }

                rootSuffix++;
                await Task.Yield();
            }
        }

        /// <summary>
        /// Asynchronously generates the major celestial bodies (e.g., planets) for each star in the system by 
        /// delegating to a <see cref="StarGen"/> instance and invoking its planet generation logic.
        /// </summary>
        /// <param name="seed">Seed value used to ensure deterministic generation across runs.</param>
        /// <returns>A task representing the asynchronous generation operation.</returns>
        private async Task GenerateMajorBodies(int seed)
        {
            Logger.Log("System Generation", "Generating Major Bodies");

            for (int i = 0; i < systemProperties.stellarBodies.Count; i++)
            {
                // Generate the planets and belts for this star
                StarGen.GenerateChildren(systemProperties.stellarBodies[i], out List<BodyProperties> planets, out List<BeltProperties> belts);
                // Place the generated planets and belts into the appropriate lists
                systemProperties.solidBodies.AddRange(planets);
                systemProperties.belts.AddRange(belts);

                await Task.Yield();
            }
        }
        
        /// <summary>
        /// Asynchronously generates the minor celestial bodies (e.g., moons, dwarf planets) for each stellar or planetary body
        /// by recursively invoking appropriate generators for each hierarchical level.
        /// </summary>
        /// <param name="seed">Seed value used to ensure deterministic generation across runs.</param>
        /// <returns>A task representing the asynchronous generation operation.</returns>
        private async Task GenerateMinorBodies(int seed)
        {
            Logger.Log("System Generation", "Generating Minor Bodies");

            // Simulate asynchronous work
            await Task.Run(() =>
            {
                // Placeholder for actual logic to generate minor bodies
                // Uncomment and implement the logic as needed
                /*
                for (int i = 0; i < systemProperties.stellarBodies.Count; i++)
                {
                    if (systemProperties.stellarBodies[i] is StarProperties star)
                    {
                        stellarBodies[i].ChildBodies = StarGen.GenerateMinorChildren((StarProperties)stellarBodies[i]);

                        for (int j = 0; j < stellarBodies[i].ChildBodies.Count; i++)
                        {
                            stellarBodies[i].ChildBodies[j].ChildBodies = PlanetGen.GenerateMinorChildren((PlanetProperties)stellarBodies[i].ChildBodies[j]);
                        }
                    }
                    else
                    {
                        stellarBodies[i].ChildBodies = PlanetGen.GenerateMinorChildren((PlanetProperties)stellarBodies[i]);
                    }
                }
                */
            });
        }

        /// <summary>
        /// Checks all generated celestial bodies in multi-star systems for unstable positions 
        /// and moves them into a higher orbit around the barycenter if necessary.
        /// </summary>
        private async Task MoveEjectedBodies()
        {
            Logger.Log(GetType().Name, "Resolving Unstable Orbits");
            await Task.Run(() =>
            {
                foreach (BaseProperties b in systemProperties.stellarBodies)
                {
                    // Check the stability of the planet's orbit
                    bool isStable = true; // Placeholder for actual stability check logic

                    if (!isStable)
                    {
                        // Logic to handle unstable orbits (e.g., moving bodies to a higher orbit)
                        // Placeholder for actual implementation
                        //
                        // // Create a new planet instance as a copy of the original
                        // //Body ejectedPlanet = new Body(p);
                        // 
                        // // Remove the original unstable planet from its star's planet list
                        // //s.planets.Remove(p);
                        // 
                        // // Estimate a new safe barycentric orbit for the ejected planet
                        // //ejectedPlanet.Orbit.SemiMajorAxis = EstimateBarycenterOrbit(planet, stellarBodies);
                        // 
                        // // Add the updated ejected planet to the stellar system as an independent body
                        // //stellarBodies.Add(ejectedPlanet);
                    }
                }
            });
        }

        /// <summary>
        /// Assigns star-based orbit lines to all bodies in the system.
        /// </summary>
        private async Task AssignColours()
        {
            Logger.Log("System Generation", "Assigning Orbital Colours");

            // Default color if no stars exist
            if (systemProperties.stellarBodies.Count() == 0)
            {
                foreach (BodyProperties body in systemProperties.solidBodies)
                {
                    body.OrbitLine = new int[] { 255, 255, 255 }; // White
                }
                foreach (BeltProperties belt in systemProperties.belts)
                {
                    belt.OrbitLine = new int[] { 255, 255, 255 }; // White
                }
                return;
            }

            // Iterate through the stellarBodies to assign colors
            foreach (StarProperties star in systemProperties.stellarBodies)
            {
                star.OrbitLine = ColourUtils.ColorToArray(PhysicsUtils.DetermineSpectralColor(star.Temperature));
            }
            
            // Iterate through belts and bodies to inherit their colour
            foreach (BodyProperties planet in systemProperties.solidBodies)
            {
                BaseProperties parent = FindBody(planet.Parent);

                if (parent != null)
                {
                    planet.OrbitLine = parent.OrbitLine;
                }
                else
                {
                    // If no parent is found it must be circumbinary
                    List<StarProperties> parentStars = systemProperties.stellarBodies.Take(2).ToList();
                    if (parentStars.Count == 2)
                    {
                        planet.OrbitLine = new int[]
                        {
                            (parentStars[0].OrbitLine[0] + parentStars[1].OrbitLine[0]) / 2,
                            (parentStars[0].OrbitLine[1] + parentStars[1].OrbitLine[1]) / 2,
                            (parentStars[0].OrbitLine[2] + parentStars[1].OrbitLine[2]) / 2
                        };
                    }
                    planet.OrbitLine = new int[] { 255, 255, 255 }; // White
                }

                await Task.Yield();
            }

            foreach (BeltProperties belt in systemProperties.belts)
            {
                BaseProperties parent = FindBody(belt.Parent);

                if (parent != null)
                {
                    belt.OrbitLine = parent.OrbitLine;
                }
                else
                {
                    // If no parent is found it must be circumbinary
                    List<StarProperties> parentStars = systemProperties.stellarBodies.Take(2).ToList();
                    if (parentStars.Count == 2)
                    {
                        belt.OrbitLine = new int[]
                        {
                            (parentStars[0].OrbitLine[0] + parentStars[1].OrbitLine[0]) / 2,
                            (parentStars[0].OrbitLine[1] + parentStars[1].OrbitLine[1]) / 2,
                            (parentStars[0].OrbitLine[2] + parentStars[1].OrbitLine[2]) / 2
                        };
                    }
                    belt.OrbitLine = new int[] { 255, 255, 255 }; // White
                }

                await Task.Yield();
            }
        }

        /// <summary>
        /// One-Time export of a freshly generated system file
        /// </summary>
        private void CreateSystemFile()
        {
            // Set appropriate file name to the name value
            string systemFilePath = $"{assetsFolder}/Star_Systems/{systemProperties.seedInput}.json";

            JsonUtils.SerializeToJsonFile(systemProperties, systemFilePath);

            Logger.Log("I/O", $"Exported System File to {systemFilePath}");
        }

        /// <summary>
        /// Generates a number of stars in the system based on observed probabilities.
        /// </summary>
        /// <param name="seed">The seed used for the randomisation</typeparam>
        /// <returns>A weighted quanity of stars in the system</returns>
        private static int DetermineStarCount(int seed)
        {
            int randomValue = RandomUtils.RandomInt(1, 120, seed);
            int starCount;

            switch (randomValue)
            {
                case >= 1 and <= 78:
                    starCount = 1;
                    break;
                case >= 79 and <= 93:
                    starCount = 2;
                    break;
                case >= 94 and <= 100:
                    starCount = 3;
                    break;
                default:
                    // Log the value when it falls into the default case
                    Logger.Log("System Generator", "Defaulting to 0 stars");
                    starCount = 0;
                break;
            }

            return starCount;
        }

        /// <summary>
        /// Finds the parent body of a given body using the parent's seed value.
        /// </summary>
        /// <param name="key">The unique identifier (SeedValue) of the parent body.</param>
        /// <returns>The parent body associated with the given SeedValue, or the barycenter body if not found.</returns>
        public BaseProperties FindBody(int key)
        {
            // Combine all body lists into one and search for the parent
            var allBodies = systemProperties.stellarBodies
                .Cast<BaseProperties>()
                .Concat(systemProperties.solidBodies)
                .Concat(systemProperties.belts);

            return allBodies.FirstOrDefault(body => body.SeedValue == key);
        }
    }
    
    
    // I wish these could share one structure but it simply won't serialise that way.
    [Serializable]
    public class SystemProperties
    {
        // System age in billions of years
        [JsonProperty("System Age (bYo)", Order = 0)] public decimal systemAge = 0m;
        // Declare seed input
        [JsonProperty("Seed", Order = 1)][HideInInspector] public string seedInput;

        public SystemProperties(string seedInput)
        {
            // Check for a manual seed input  
            this.seedInput = string.IsNullOrEmpty(seedInput) ? RandomUtils.GenerateSystemName() : seedInput;
        }

        [JsonProperty("Stellar Bodies", Order = 5)] public List<StarProperties> stellarBodies = new List<StarProperties>();
        [JsonProperty("Solid Bodies", Order = 6)] public List<BodyProperties> solidBodies = new List<BodyProperties>();
        [JsonProperty("Belts/Rings", Order = 7)] public List<BeltProperties> belts = new List<BeltProperties>();

        // Metadata for the system
        [JsonProperty("Stars", Order = 2)]
        private int starCount
        {
            get => stellarBodies.Count;
        }

        [JsonProperty("Bodies", Order = 3)]
        private int bodyCount
        {
            get => solidBodies.Count;
        }

        [JsonProperty("Belts", Order = 4)]
        private int beltCount
        {
            get => belts.Count(belt => stellarBodies.Any(star => star.SeedValue == belt.Parent));
        }

    }
}
