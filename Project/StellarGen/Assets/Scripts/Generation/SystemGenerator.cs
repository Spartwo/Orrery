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

            Logger.Log("System Generation", $"Seed: {seedInput} / {usableSeed}");

            // Kick on a generator for quanity of stars.
            int starCount = DetermineStarCount(usableSeed);
            // Override the star count to 1 for testing
            starCount = 1;
            Logger.Log("System Generation", "Stars: " + starCount);

            // Temporarily define the stellar systems total age
            float systemAge = await GenerateStellarBodies(usableSeed, starCount);
            AssignAges(systemAge);

            // This sectionn is the important bit, drives the layered generation processes from stellar orbits to body positions
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
                stellarBodies.Add(PlanetGen.Generate(seed, null));
                systemAge = RandomUtils.RandomFloat(0.25f, 10.5f, seed);
            }
            else
            {
                float shortestLifespan = 10.5f;
                for (int i = 0; i < starCount; i++)
                {
                    // Generate a new star but only get the properties via StarProperties
                    StarProperties newStarProperties = StarGen.Generate(seed + i);

                    // Add the generated properties to the stellarBodies list
                    stellarBodies.Add(newStarProperties);

                    // Roll back all the stars to the youngest
                    shortestLifespan = Math.Min((float)shortestLifespan, (float)newStarProperties.Lifespan);

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

            if (starCount < 2) return; // No need to position stars if there's only one

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
            decimal totalMass = stellarBodies[0].Mass + stellarBodies[1].Mass;
            float distanceA = distanceAB * (float)(stellarBodies[1].Mass / totalMass);
            float distanceB = distanceAB * (float)(stellarBodies[0].Mass / totalMass);

            // Assign orbit to the first star
            stellarBodies[0].Orbit = new OrbitalProperties(
                PhysicsUtils.ConvertToMetres(distanceA),
                eccentricityAB,
                0f,
                0f,
                0f
            );

            // Assign orbit to the second star
            stellarBodies[1].Orbit = new OrbitalProperties(
                PhysicsUtils.ConvertToMetres(distanceB),
                eccentricityAB,
                180f,
                0f,
                0f
            );
        }

        private void CalculateDeadZone(out float closeMax, out float farMin)
        {
            if (stellarBodies[0] is StarProperties star1 && stellarBodies[1] is StarProperties star2)
            {
                // Now you can access StarProperties-specific members
                float mass1 = star1.StellarMass;
                float mass2 = star2.StellarMass;

                // Perform calculations for closeMax and farMin based on the star's properties
                closeMax = (float)(mass1 * 0.1f); // Example calculation
                farMin = (float)(mass1 * 0.5f); // Example calculation
            }
            else
            {
                // Handle the case where stellarBodies[0] is not a StarProperties instance
                Logger.LogWarning("System Generation", "One or more of the bodies are not stars");
                closeMax = 0f;
                farMin = 0f;
            }

        }

        /// <summary>
        /// Sets the ages of all bodies to one shared value
        /// </summary>
        private void AssignAges(float systemAge)
        {
            // Convert to decimal to limit the points when exported
            decimal transferredAge = (decimal)Math.Round(systemAge, 3);
            Logger.Log("System Generation", $"System Age: {transferredAge}bY");

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
            Logger.Log("System Generation", "Generating Body Names");
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

        /// <summary>
        /// Asynchronously generates the major celestial bodies (e.g., planets) for each star in the system by 
        /// delegating to a <see cref="StarGen"/> instance and invoking its planet generation logic.
        /// </summary>
        /// <param name="seed">Seed value used to ensure deterministic generation across runs.</param>
        /// <returns>A task representing the asynchronous generation operation.</returns>
        private async Task GenerateMajorBodies(int seed)
        {
            Logger.Log("System Generation", "Generating Major Bodies");

            for (int i = 0; i < stellarBodies.Count; i++)
            {
                // Create a new Generator to build the children
                stellarBodies[i].ChildBodies = (StarGen.GenerateChildren((StarProperties)stellarBodies[i]));

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

            // Generate children without children of their own (moons etc)
            for (int i = 0; i < stellarBodies.Count; i++)
            {
                if (stellarBodies[i] is StarProperties star)
                {
                    // Create a new Generator to build the children
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
                await Task.Yield();
            }
        }

        /// <summary>
        /// Checks all generated celestial bodies in multi-star systems for unstable positions 
        /// and moves them into a higher orbit around the barycenter if necessary.
        /// </summary>
        private async Task MoveEjectedBodies()
        {
            Logger.Log(GetType().Name, "Resolving Unstable Orbits");
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
        /// Assigns star-based orbit lines to all bodies in the system.
        /// </summary>
        private async Task AssignColours()
        {
            Logger.Log("System Generation", "Assigning Orbital Colours");

            // Default color if no stars exist
            if (!stellarBodies.OfType<StarProperties>().Any())
            {
                foreach (var body in stellarBodies)
                {
                    body.OrbitLine = new int[] { 255, 255, 255 }; // White
                }
                return;
            }

            // Iterate through the stellarBodies to assign colors
            foreach (BodyProperties body in stellarBodies)
            {
                if (body is StarProperties star)
                {
                    body.OrbitLine  = ColourUtils.ColorToArray(PhysicsUtils.DetermineSpectralColor(star.Temperature));
                    
                    // Assign color downwards to all children
                    foreach (BodyProperties child in star.ChildBodies)
                    {
                        child.OrbitLine = body.OrbitLine;
                        foreach (BodyProperties grandchild in star.ChildBodies)
                        {
                            grandchild.OrbitLine = body.OrbitLine;
                        }
                    }
                }
                else
                {
                    var parentStars = stellarBodies.OfType<StarProperties>().Take(2).ToList();
                    if (parentStars.Count == 2)
                    {
                        body.OrbitLine = new int[]
                        {
                            (parentStars[0].OrbitLine[0] + parentStars[1].OrbitLine[0]) / 2,
                            (parentStars[0].OrbitLine[1] + parentStars[1].OrbitLine[1]) / 2,
                            (parentStars[0].OrbitLine[2] + parentStars[1].OrbitLine[2]) / 2
                        };
                    }
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
            string systemFilePath = $"{assetsFolder}/Star_Systems/{seedInput}.json";

            // Turn the lists into usable outputs
            JsonUtils.SerializeListToJsonFile(stellarBodies, systemFilePath);

            Logger.Log("I/O", $"Exported System File to {systemFilePath}");
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
