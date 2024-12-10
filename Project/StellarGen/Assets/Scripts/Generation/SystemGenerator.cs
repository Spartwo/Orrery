using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Random = System.Random;
using RandomUtils = StellarGenHelpers.RandomUtils;
using PhysicsUtils = StellarGenHelpers.PhysicsUtils;
using ColorUtils = StellarGenHelpers.ColorUtils;
using JsonUtils = StellarGenHelpers.JsonUtils;
using System;
using Unity.VisualScripting;
using static UnityEngine.Rendering.DebugUI;

namespace SystemGen
{
    public class SystemGenerator : MonoBehaviour
    {
        // Declare seed input from UI and
        [SerializeField]
        private string seedInput;
        public string SeedInput
        {
            set => seedInput = value;
        }
        private string assetsFolder = Application.streamingAssetsPath;
        // The generated stellar data
        public static List<Body> stellarBodies = new List<Body>();

        void Start()
        {
            // Create folder on initialisation incase of total deletion
            Directory.CreateDirectory($"{assetsFolder}/Star_Systems/");
            
            //Currently instantly try to generate
            GenerateSystemFile();
        }

        /// <summary>
        /// The Main Function, goes through the steps to produce a stellar system json file using helper functions
        /// </summary>
        public void GenerateSystemFile()
        {
            StartCoroutine(GenerateSystemCoroutine());
        }

        IEnumerator GenerateSystemCoroutine()
        {
            // Check for a manual seed input
            seedInput ??= GenerateSystemName();
            // Convert alphanumeric seed to a usable integer
            int usableSeed = seedInput.GetHashCode();

            // Kick on a generator for quanity of stars 
            Random rand = new Random(usableSeed);
            int starDictator = rand.Next(1, 1001);
            int starCount;

            // Assign number of stars to spawn
            switch (starDictator)
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

                case >= 876 and <= 1001:
                    starCount = 0;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("starDictator", "Value is outside the expected range.");
            }

            // Temporarily define the stellar systems total age
            float systemAge; 

            if (starCount == 0) {
                // Create a singular rogue planet
                Planet newPlanet = new Planet(usableSeed);
                stellarBodies.Add(newPlanet);
                // Age is less important than with a star
                systemAge = RandomUtils.RandomFloat(0.25f, 10.5f, usableSeed);
            } 
            else
            {
                // Lifespan in Billions, oldest known rocky body is 10b
                float shortestLifespan = 10.5f;
                for (int i = 0; i < starCount; i++)
                {
                    // Create a new star and add it to the stellarBodies list
                    Star newStar = new Star(usableSeed + i);
                    newStar.StarGen();
                    stellarBodies.Add(newStar);
                    // Check to see if this star has the shortest lifespan
                    shortestLifespan = Math.Min(shortestLifespan, newStar.Lifespan);
                }

                // Set the system age within the possible bounds of main sequence age
                systemAge = RandomUtils.RandomFloat(0.25f, shortestLifespan, usableSeed);
            }

            // Convert to decimal to limit the points when exported
            decimal transferredAge = (decimal)Math.Round(systemAge, 3);
            Debug.Log("System Age: " + transferredAge);

            // Sort the stellarBodies list by mass in descending order
            stellarBodies.Sort((a, b) => b.Mass.CompareTo(a.Mass));

            // Assign suffixes to each body based on its arrangement in the system
            char suffix = 'A';
            for (int i = 0; i < stellarBodies.Count; i++)
            {
                // Get the current body
                var body = stellarBodies[i];

                // Set the age for each body
                body.Age = transferredAge;

                // Combine the system name with a suffix
                body.Name = $"{seedInput}{suffix}";
                // Increment suffix for the next largest body
                suffix++;

                // Generate children (planets)
                body.GenerateChildren();
            }

            // Check for ejected bodies(P-type orbits)
            if (starCount >1)
            {
               // MoveEjectedBodies();
            }

            // Save the System Data Arrays
            CreateSystemFile();

            return null;
        }

        /// <summary>
        /// Checks all generated celestial bodies in multi-star systems for unstable positions 
        /// and moves them into a higher orbit around the barycenter if necessary.
        /// </summary>
        private void MoveEjectedBodies()
        {
            foreach (Body b in stellarBodies)
            {
               
                    // Check the stability of the planet's orbit
                    bool isStable = b.CheckOrbit();
                    
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

            Debug.Log($"Exported System File to {systemFilePath}");
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
            Debug.Log($"System Name: {randName}-{randNumber}");
            return $"{randName}-{randNumber}";
        }

    }
}
