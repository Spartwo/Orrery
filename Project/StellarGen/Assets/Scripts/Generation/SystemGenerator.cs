using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Random = System.Random;
using RandomUtils = StellarGenHelpers.RandomUtils;
using PhysicsUtils = StellarGenHelpers.PhysicsUtils;
using ColorUtils = StellarGenHelpers.ColorUtils;
using JsonUtils = StellarGenHelpers.JsonUtils;
using System;
using Models;
using Settings;

namespace SystemGen
{
    public class SystemGenerator : MonoBehaviour
    {
        // Declare seed input
        [SerializeField]
        private string seedInput;
        public string SeedInput { set => seedInput = value; }

        private readonly string assetsFolder = Application.streamingAssetsPath;
        // The generated stellar data
        public List<BodyProperties> stellarBodies = new List<BodyProperties>();

        void Start()
        {
            //Currently instantly try to generate
            if (GlobalSettings.GenerateOnStartup) { StartGeneration(); }
        }

        /// <summary>
        /// Starts the system generation process asynchronously.
        /// </summary>
        public void StartGeneration() 
        { 
            GenerateSystem().ConfigureAwait(false); 
        }

        /// <summary>
        /// The Main Function, goes through the steps to produce a stellar system json file using helper functions
        /// </summary>
        private async Task GenerateSystem()
        {
            // Create folder on initialisation incase of total deletion
            Directory.CreateDirectory($"{assetsFolder}/Star_Systems/");

            // Check for a manual seed input
            seedInput ??= GenerateSystemName();
            // Convert alphanumeric seed to a usable integer
            int usableSeed = seedInput.GetHashCode();

            // Kick on a generator for quanity of stars.
            int starCount = DetermineStarCount(usableSeed);
            Logger.Log("System Generator", "Stars: " + starCount);

            // Temporarily define the stellar systems total age
            float systemAge = await GenerateStellarBodies(usableSeed, starCount);

            AssignAges(systemAge);


            // Check for ejected bodies(P-type orbits)
            if (starCount > 1)
            {
                await MoveEjectedBodies();
            }

            // Save the System Data Arrays
            CreateSystemFile();
        }

        private async Task<float> GenerateStellarBodies(int seed, int starCount)
        {
            float systemAge;
            stellarBodies.Clear();

            if (starCount == 0)
            {
                // Generate a new planet
                PlanetGen newRogueGen = new PlanetGen();

                // Add the generated properties to the stellarBodies list
                stellarBodies.Add(newRogueGen.Generate(seed));
                systemAge = RandomUtils.RandomFloat(0.25f, 10.5f, seed);
            }
            else
            {
                float shortestLifespan = 10.5f;
                for (int i = 0; i < starCount; i++)
                {
                    // Generate a new star but only get the properties via StarProperties
                    StarGen newStarGen = new StarGen();
                    StarProperties newStarProperties = newStarGen.Generate(seed + i);  // Get properties

                    // Add the generated properties to the stellarBodies list
                    stellarBodies.Add(newStarProperties);

                    // Roll back all the stars to the youngest
                    shortestLifespan = Math.Min(shortestLifespan, newStarProperties.Lifespan);

                    // Every star, yield execution to prevent blocking
                    await Task.Yield();
                }
                systemAge = RandomUtils.RandomFloat(0.25f, shortestLifespan, seed);
            }
            return systemAge;
        }

        /// <summary>
        /// Sets the ages of all bodies to one shared value
        /// </summary>
        private void AssignAges(float systemAge)
        {
            // Convert to decimal to limit the points when exported
            decimal transferredAge = (decimal)Math.Round(systemAge, 3);
            Logger.Log(GetType().Name, $"System Age: {transferredAge}bY");

            // Assign the system age to each body
            foreach (var body in stellarBodies)
            {
                body.Age = transferredAge;
            }
        }

        /// <summary>
        /// Recursively assigns names to stars, planets, and moons based on their mass and parent star.
        /// This method is called after the generation process to ensure all bodies are properly named.
        /// Stars are named A, B, C, etc.; planets are named Aa, Ab, Ac, etc.; moons are named Aa-1, Aa-2, etc.
        /// </summary>
        private async Task AssignNames()
        {
            // Sort root stellar bodies, larger stars get the first letters
            stellarBodies.Sort((a, b) => b.Mass.CompareTo(a.Mass));
            char rootSuffix = 'A'; // Start base object names from A

            // Iterate through the stellarBodies to assign names to stars
            foreach (var star in stellarBodies.OfType<StarProperties>())
            {
                // Assign initial star name ('A', 'B', 'C') if no custom name
                if (!star.CustomName)
                {
                    star.Name = $"{seedInput} {rootSuffix}";
                }

                // Assign names to planets and their moons recursively
                if (star.ChildBodies.Any())
                {
                    await AssignNamesRecursive(star, star.Name);
                }

                rootSuffix++;
            }
        }

        /// <summary>
        /// Recursively assigns names to child bodies (planets and moons) based on their parent name.
        /// </summary>
        /// <param name="parent">The parent body whose children are being named.</param>
        /// <param name="parentName">The name of the parent body.</param>
        private async Task AssignNamesRecursive(BodyProperties parent, string parentName)
        {
            char childSuffix = 'a'; // Start child names from 'a'

            foreach (var child in parent.ChildBodies)
            {
                // Assign name to the child body
                if (!child.CustomName)
                {
                    child.Name = $"{parentName}{childSuffix}";
                }

                // If the child has its own children (moons), assign names recursively
                if (child.ChildBodies.Any())
                {
                    await AssignNamesRecursive(child, child.Name);
                }

                childSuffix++;
            }
        }

        private async Task GenerateMajorBodies()
        {
            // Generate children (planets)
            //body.GenerateChildren();
        }
        private async Task GenerateLesserBodies()
        {
            // Generate children without children of their own (moons etc)
            //body.GenerateChildren();
        }

        /// <summary>
        /// Checks all generated celestial bodies in multi-star systems for unstable positions 
        /// and moves them into a higher orbit around the barycenter if necessary.
        /// </summary>
        private async Task MoveEjectedBodies()
        {
            foreach (BodyProperties b in stellarBodies)
            {

                // Check the stability of the planet's orbit
                bool isStable = true;//b.Orbit.CheckOrbit();

                if (!isStable)
                {
                    // Create a new planet instance as a copy of the original
                    //Body ejectedPlanet = new Body(p);

                    // Remove the original unstable planet from its star's planet list
                    //s.planets.Remove(p);

                    // Estimate a new safe barycentric orbit for the ejected planet
                    //ejectedPlanet.Orbit.SemiMajorAxis = EstimateBarycenterOrbit(planet, stellarBodies);

                    // Add the updated ejected planet to the stellar system as an independent body
                    //stellarBodies.Add(ejectedPlanet);
                }

            }
        }

        /// <summary>
        /// One-Time export of a freshly generated system file
        /// </summary>
        private void CreateSystemFile()
        {
            // Set appropriate file name to the name value
            string systemFilePath = $"{assetsFolder}/Star_Systems/{seedInput}.json";

            // Turn the lists into usable outputs
            JsonUtils.SerializeListToJsonFile(stellarBodies, systemFilePath);

            Logger.Log(GetType().Name, $"Exported System File to {systemFilePath}");
        }

        /// <summary>
        /// Generates a seed input value / system file name
        /// </summary>
        /// <returns>An input string of the systems seed</returns>
        private string GenerateSystemName()
        {
            // Import list from a plaintext file
            string nameGenerationFilePath = $"{assetsFolder}/Localisation/System_Names.txt";
            List<string> systemNameArray = File.ReadAllLines(nameGenerationFilePath).ToList();

            // Choose a random row in the file
            Random random = new Random();
            string randName = systemNameArray[random.Next(1, (systemNameArray.Count))];
            int randNumber = random.Next(1, 1000);

            // Format the output as [RandName RandNumber]
            Logger.Log(GetType().Name, $"System Name: {randName}-{randNumber}");
            return $"{randName}-{randNumber}";
        }

        /// <summary>
        /// Generates a number of stars in the system based on observed probabilities.
        /// </summary>
        /// <param name="seed">The seed used for the randomisation</typeparam>
        /// <returns>A weighted quanity of stars in the system</returns>
        private static int DetermineStarCount(int seed)
        {
            int randomValue = RandomUtils.RandomInt(1, 1000, seed);
            int starCount;

            switch (randomValue)
            {
                case >= 1 and <= 400:
                    starCount = 1;
                    break;
                case >= 401 and <= 800:
                    starCount = 2;
                    break;
                case >= 801 and <= 875:
                    starCount = 3;
                    break;
                default:
                    // Log the value when it falls into the default case
                    Logger.Log("System Generator", "Star Quantity random outside expected range. Defaulting to 0.");
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
        public BodyProperties FindParent(int key)
        {
            // Iterate through the stellarBodies to find parent, not so many elements to require 3d
            foreach (BodyProperties body in stellarBodies)
            {
                if (key == body.SeedValue)
                {
                    return body;
                }
            }

            // Return the barycentre if no parent expressely declared
            return stellarBodies[0];
        }
    }
}
