﻿using System;
using System.Collections.Generic;
using Random = System.Random;
using RandomUtils = StellarGenHelpers.RandomUtils;
using PhysicsUtils = StellarGenHelpers.PhysicsUtils;
using ColorUtils = StellarGenHelpers.ColorUtils;
using JsonUtils = StellarGenHelpers.JsonUtils;
using UnityEngine;
using Unity.VisualScripting;
using System.IO;
using System.Collections;
using Newtonsoft.Json;
using Models;
using System.Xml.Linq;

namespace SystemGen
{
    public class StarGen : BodyGen
    {
        public StarProperties Generate(int seedValue)
        {
            seedValue = RandomUtils.TweakSeed(seedValue);

            // Generate Stellar mass using the method
            float stellarMass = GenerateStellarMass(seedValue);
            decimal mass = (decimal)stellarMass * 332946;
            // Diameter at formation
            float diameter = Mathf.Pow(stellarMass, 0.7f);
            // General luminosity from size and mass based temperature
            float luminosity = Mathf.Pow(diameter, 2) * Mathf.Pow((Mathf.Pow(stellarMass, 0.5f)), 4);
            // Estimate main sequence lifespan from diameter divided by luminosity
            float lifespan = (diameter / luminosity) * 10;

            return new StarProperties(
                seedValue: seedValue,
                mass: mass,
                diameter: diameter,
                luminosity: luminosity,
                lifespan: lifespan
            );
        }

        // Stellar Generation segments after comparing lifespans
        public BodyProperties LateGenerate(int seedValue, StarProperties star)
        {

            // Send the star back up to systemgen
            return star;
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

        public void GenerateChildren(int seedValue)
        {

            // Generate number of planets
            int planetCount = (int)(Mathf.Max((Mathf.Pow(stellarMass, 0.3f) * RandomUtils.RandomInt(1, 10, seedValue)), 1));

            // Estimated distance of the heliopause
            float SOIEdge = Mathf.Sqrt(Luminosity) * 75;
            // Set inner edge as closest bearable temperature limit
            float SOIInner = Mathf.Pow((3f * stellarMass) / (9f * 3.14f * 5.51f), 0.33f);
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

            List<BodyGen> childBodies = new List<BodyGen>();
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
                BodyGen newPlanet = new PlanetGen(seedValue + p);

                // Set the orbital properties of the new planet
                PhysicsUtils.ConstructOrbitProperties(position, meanEccentricity, maxInclination);

                // Add the new planet to the childBodies list
                childBodies.Add(newPlanet);

            }


            base.GenerateChildren(childBodies);
        }
    }
}