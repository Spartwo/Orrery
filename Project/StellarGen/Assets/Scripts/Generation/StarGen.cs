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
        /// Method to generate 
        /// </summary>
        /// <param name="children">The elements being passed downwards from the inherited classes</param>
        public void GenerateChildren(int seedValue, StarProperties star)
        {

            // Generate number of planets
            int planetCount = (int)(Mathf.Max((Mathf.Pow(star.StellarMass, 0.3f) * RandomUtils.RandomInt(1, 10, seedValue)), 1));

            // Estimated distance of the heliopause
            float SOIEdge = Mathf.Sqrt(star.Luminosity) * 75;
            // Set inner edge as closest bearable temperature limit
            float SOIInner = Mathf.Pow((3f * star.StellarMass) / (9f * 3.14f * 5.51f), 0.33f);
            // Set keystone planet(others resonate to it)
            float innerOrbit = RandomUtils.RandomFloat(SOIInner, SOIInner * 5, seedValue);
            float planetarySpacing = RandomUtils.RandomFloat((0.06f) * SOIEdge, 10, seedValue);

            // Mean eccentricity due to planet count
            float meanEccentricity = (float)Math.Max(Math.Pow(planetCount, -0.15) - 0.65, 0.01);
            float maxInclination = (15f - planetCount) / 2;


            // Define all resonant orbital positions relative to the keystone
            List<float> orbitalPositions = new List<float>();
            List<int> usedPositions = new List<int>();
            // Iterate until the position exceeds the SOI edge
            int i = 0;
            while (true)
            {
                // Calculate the orbital position based on the formula
                float position = innerOrbit + (planetarySpacing * Mathf.Pow(2, i));

                // Check if the position exceeds the SOI edge
                if (position > SOIEdge)
                {
                    // Stop adding further positions once we exceed the SOI edge
                    break;
                }

                // Add the position to the list if it is within the SOI edge
                orbitalPositions.Add(position);

                i++;
            }

            List<BodyProperties> childBodies = new List<BodyProperties>();
            // Populate the orbital positions
            for (int p = 0; p < planetCount; p++)
            {
                // Generate a random position in the positions array
                int pos = RandomUtils.RandomInt(0, orbitalPositions.Count, seedValue + p);

                // Ensure that the position isn't used yet
                do pos = RandomUtils.RandomInt(0, orbitalPositions.Count, seedValue + p);
                while (usedPositions.Contains(pos));

                // Get the position from the orbitalPositions list
                float position = orbitalPositions[pos];

                // Remove the used position from the list to avoid duplicates
                orbitalPositions.RemoveAt(pos);

                // Create a new planet with a unique seed value
                PlanetProperties newPlanet = new PlanetGen().Generate(seedValue + p);

                // Set the orbital properties of the new planet
                PhysicsUtils.ConstructOrbitProperties(seedValue, position, meanEccentricity, maxInclination);

                // Add the new planet to the childBodies list
                childBodies.Add(newPlanet);

            }

        }

        private int GetPlanetCount(int seedValue)
        {
            // Generate number of planets
            int planetCount = 1;
            return planetCount;
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