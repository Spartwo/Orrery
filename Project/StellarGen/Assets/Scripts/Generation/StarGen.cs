using System;
using System.Collections.Generic;
using Random = System.Random;
using RandomUtils = StellarGenHelpers.RandomUtils;
using PhysicsUtils = StellarGenHelpers.PhysicsUtils;
using ColourUtils = StellarGenHelpers.ColourUtils;
using JsonUtils = StellarGenHelpers.JsonUtils;
using UnityEngine;
using Unity.VisualScripting;
using System.IO;
using System.Collections;
using Newtonsoft.Json;
using Models;
using System.Xml.Linq;
using StellarGenHelpers;

namespace SystemGen
{
    public class StarGen : BodyGen
    {
        // Default to most common
        private PlanetOrder planetOrder = PlanetOrder.SIMILAR; 
        private StarProperties starProperties;

        public StarProperties Generate(int seedValue)
        {
            if (seedValue == 0)
            {
                // If no seed is provided then pick one at random
                seedValue = RandomUtils.RandomInt(0, int.MaxValue);
            }
            else
            {
                // If it is provided then adjust its value to avoid intersections
                seedValue = RandomUtils.TweakSeed(seedValue);
            }

            // Generate Stellar mass using the method
            float stellarMass = GenerateStellarMass(seedValue);

            StarProperties newStar = new StarProperties(seedValue);
            // Determine the properties of the star at the current time from its mass
            newStar.GenerateStarProperties(stellarMass);

            return newStar;
        }

        /// <summary>
        /// Generates a stellar mass based on probability values along a curve
        /// {{0, 0.1}, {0.16, 0.2}, {0.47, 0.5}, {0.7, 0.75}, {0.9, 1.5}, {0.96, 3.5}, {1, 5}}
        /// </summary>
        /// <param name="seedValue">The numerical seed for this </param>
        /// <returns>A stellar mass in sols</returns>
        private static float GenerateStellarMass(int seedValue)
        {
            // Instantiate random number generator 
            Random rand = new Random(seedValue);

            // Get a random percentile
            float graphX = (float)rand.NextDouble();
            // Convert it to stellar masses between 0.1 and 5 using the formula
            float graphY = (0.1f + ((-0.374495f * graphX) / (-1.073858f + graphX)));

            Logger.Log("System Generation", "Sols Mass: " + graphY);
            return graphY;
        }

        /// <summary>
        /// Method to generate the major child bodies(planets) of a star
        /// </summary>
        /// <param name="children">The elements being passed downwards from the inherited classes</param>
        public List<BodyProperties> GenerateChildren(StarProperties star)
        {
            base.GenerateChildren((BodyProperties)star);


            int seedValue = star.SeedValue;
            // Generate number of planets
            int planetCount = GeneratePlanetCount(star, seedValue);

            float SOIEdge, SOIInner, innerOrbit, planetarySpacing;
            InitializeOrbitalBounds(star, seedValue, out SOIEdge, out SOIInner, out innerOrbit, out planetarySpacing);

            // Mean eccentricity due to planet count
            float meanEccentricity = (float)Math.Max(Math.Pow(planetCount, -0.15) - 0.65, 0.01);
            float maxInclination = (15f - planetCount) / 2;

            // Define all resonant orbital positions relative to the keystone
            List<float> orbitalPositions = CalculateOrbitalPositions(SOIEdge, innerOrbit, planetarySpacing);
            List<int> usedPositions = new List<int>();

            DetermineArrangement(star);

            List<BodyProperties> childBodies = GeneratePlanetsFromPositions(
                seedValue, planetCount, orbitalPositions, meanEccentricity, maxInclination
            );

            return childBodies;

        }
        private void InitializeOrbitalBounds(StarProperties star, int seed, out float SOIEdge, out float SOIInner, out float innerOrbit, out float spacing)
        {
            // Estimated distance of the heliopause
            SOIEdge = Mathf.Sqrt(star.Luminosity) * 75;
            // Set inner edge as closest bearable temperature limit
            SOIInner = Mathf.Pow((3f * star.StellarMass) / (9f * 3.14f * 5.51f), 0.33f);

            innerOrbit = RandomUtils.RandomFloat(SOIInner, SOIInner * 5, seed);
            // Set keystone planet(others resonate to it)
            spacing = RandomUtils.RandomFloat(0.004f * SOIEdge, 0.05f * SOIEdge, seed);
        }
        private List<float> CalculateOrbitalPositions(float SOIEdge, float innerOrbit, float spacing)
        {
            List<float> positions = new List<float>();
            int i = 0;

            int maxIterations = 30;
            while (i < maxIterations)
            {
                // Calculate the orbital position based on the formula
                float pos = innerOrbit + spacing * Mathf.Pow(2, i);
                Debug.Log($"Orbital Position: {pos}");
                // Stop adding further positions once we exceed the SOI edge
                if (pos > SOIEdge) break;
                positions.Add(pos);
                i++;
            }
            if (i >= maxIterations)
            {
                Debug.LogWarning("Orbital position generation hit maximum iteration cap.");
            }

            Debug.Log($"Orbital Positions: {positions.Count+1}");
            return positions;
        }
        private List<BodyProperties> GeneratePlanetsFromPositions(int seed, int count, List<float> positions, float eccentricity, float inclination)
        {
            var planets = new List<BodyProperties>();
            var used = new HashSet<int>();

            for (int p = 0; p < count; p++)
            {
                break;
                int index;
                do
                {
                    index = RandomUtils.RandomInt(0, positions.Count, seed + p);
                    Debug.Log(positions.Count);
                }
                while (used.Contains(index));

                used.Add(index);
                float position = positions[index];
                positions.RemoveAt(index); // Optional: can skip if not reusing

                var newPlanet = new PlanetGen().Generate(seed + p);
                PhysicsUtils.ConstructOrbitProperties(seed, position, eccentricity, inclination);

                planets.Add(newPlanet);
            }

            return planets;
        }

        private int GeneratePlanetCount(StarProperties star, int seed)
        {
            int planetCount = (int)Mathf.Max(Mathf.Pow(star.StellarMass, 0.3f) * RandomUtils.RandomInt(1, 10, seed), 1);
            Logger.Log("System Generation", "Planet Count: " + planetCount);
            return planetCount;
        }

        private PlanetOrder DetermineArrangement(StarProperties star)
        {
            return PlanetOrder.SIMILAR;
        }


        /// <summary>
        /// Enum to store the arrangement of the given star's planets
        /// </summary>
        private enum PlanetOrder
        {
            ORDERED,
            ANTI_ORDERED,
            MIXED,
            SIMILAR
        }
    }
}