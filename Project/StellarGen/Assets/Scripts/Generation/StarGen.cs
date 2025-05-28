using System;
using System.Collections.Generic;
using Random = System.Random;
using RandomUtils = StellarGenHelpers.RandomUtils;
using PhysicsUtils = StellarGenHelpers.PhysicsUtils;
using ColourUtils = StellarGenHelpers.ColourUtils;
using JsonUtils = StellarGenHelpers.JsonUtils;
using UnityEngine;
using System.IO;
using System.Collections;
using Newtonsoft.Json;
using Models;
using System.Xml.Linq;
using StellarGenHelpers;
using System.Linq;
using System.Buffers;
using System.Numerics;

namespace SystemGen
{
    public static class StarGen
    {
        public static StarProperties Generate(int seedValue)
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
            // Get a random percentile
            float graphX = RandomUtils.RandomFloat(0, 1, seedValue);
            // Convert it to stellar masses between 0.1 and 5 using the formula
            float graphY = (0.1f + ((-0.374495f * graphX) / (-1.073858f + graphX)));

            return graphY;
        }

        /// <summary>
        /// Generates the primary child bodies (planets) of a star system by computing orbital positions,
        /// disk characteristics, and planet formation order, then creating the corresponding planetary bodies.
        /// </summary>
        /// <param name="star">The <see cref="StarProperties"/> object from which planetary parameters are derived.</param>
        /// <returns>A list of generated <see cref="BaseProperties"/> representing the planets of the star.</returns>
        public static void GenerateChildren(StarProperties star, out List<BodyProperties> planets, out List<BeltProperties> belts)
        {
            int seedValue = star.SeedValue;
            // Generate number of planets
            int planetCount = GeneratePlanetCount(star, seedValue);

            // Declare variables for the out parameters
            float metalicity;
            PlanetOrder planetOrder;
            decimal diskMass;

            // Pass the required out parameters
            DetermineArrangement(star, out metalicity, out planetOrder, out diskMass);

            // metalicity is [Fe/H] index
            float metalFraction = Mathf.Pow(10f, metalicity);
            float solidsFraction = PhysicalConstants.SOLAR_METALICITY * metalFraction;
            decimal totalSolidMass = diskMass * (decimal)solidsFraction;

            belts = new List<BeltProperties>();
            belts.Add(GenerateKuiperBelt(star));

            float sublimationRadius = GenerateSublimationRadius(star);

            // Define all resonant orbital positions relative to the keystone
            List<float> orbitalPositions = CalculateOrbitalPositions(sublimationRadius, PhysicsUtils.ConvertToAU(belts[0].LowerEdge));

            // Mean eccentricity due to planet count
            float meanEccentricity = (float)Math.Max(Math.Pow(planetCount, -0.15) - 0.65, 0.01);
            float maxInclination = (15f - planetCount) / 2;

            planets = GeneratePlanetsFromPositions(
                star, seedValue, planetCount, orbitalPositions, meanEccentricity, maxInclination, metalicity, planetOrder, totalSolidMass, solidsFraction, diskMass
            );

            return;
        }

        /// <summary>
        /// Generates the minor child bodies (dwarf planets, major asteroids) of a star.
        /// </summary>
        /// <param name="star">The <see cref="StarProperties"/> object from which planetary parameters are derived.</param>
        /// <returns>A list of generated <see cref="BaseProperties"/> representing the planets of the star.</returns>
        public static List<BodyProperties> GenerateMinorChildren(StarProperties star)
        {
            // Generate a list of minor bodies (e.g., asteroids, comets) based on the parent body
            List<BodyProperties> minorBodies = new List<BodyProperties>();
            return minorBodies;
        }

        /// <summary>
        /// Generates a Kuiper Belt around the star, which is a region of icy bodies and debris beyond the outermost planet.
        /// </summary>
        /// <param name="star"></param>
        /// <returns></returns>
        public static BeltProperties GenerateKuiperBelt(StarProperties star)
        {
            int seedValue = star.SeedValue;
            // Declare parameters of the observed trends
            const float baseline = 45;    // Base radius for 1 L☉ (AU)
            const float slope = 0.14f;      // Slope of the trendline
            const float scatter = 0.44f;    // Scatter of the trendline

            float baseRadius = (float)(baseline * Math.Pow(star.BaseLuminosity, slope));
            float minLowerEdge = (float)Math.Sqrt(star.BaseLuminosity) * 4.8f; // Frost Line

            // Scatter 2 edges
            float edgeA = baseRadius * RandomUtils.RandomFloat(1 - scatter, 1 + scatter, star.SeedValue);
            float edgeB = baseRadius * RandomUtils.RandomFloat(1 - scatter, 1 + scatter, star.SeedValue + 1);

            float lowerEdge = Math.Min(edgeA, edgeB);
            float upperEdge = Math.Max(edgeA, edgeB);
            float position = (lowerEdge + upperEdge) / 2;

            // Adjust edges if lowerEdge is less than minLowerEdge in very high mass stars
            if (lowerEdge < minLowerEdge)
            {
                float adjustmentFactor = minLowerEdge / lowerEdge;
                lowerEdge *= adjustmentFactor;
                upperEdge *= adjustmentFactor;
            }

            // Calculate the rough area of the belt zone
            double lowerArea = Math.PI * lowerEdge * lowerEdge;
            double upperArea = Math.PI * upperEdge * upperEdge;
            double beltArea = upperArea - lowerArea;

            // Apply the solar system to this area
            decimal beltMass =(decimal)(PhysicalConstants.KUIPER_DENSITY * beltArea);

            Logger.Log("System Generation", $"Generated Kuiper Belt from {lowerEdge}AU to {upperEdge}AU");

            // Produce a belt within the given paramaters
            OrbitalProperties orbit = PhysicsUtils.ConstructOrbitProperties(seedValue, position, 0, 0);
            BeltProperties kuiperBelt = BeltGen.Generate(seedValue, star, orbit, beltMass, lowerEdge, upperEdge);
            kuiperBelt.Parent = star.SeedValue;

            return kuiperBelt;
        }

        /// <summary>
        /// Determines the sublimation radius of the star, which is the distance at which matter can coalesce into a solid body.
        /// </summary>
        /// <param name="star"></param>
        /// <returns></returns>
        private static float GenerateSublimationRadius(StarProperties star)
        {
            float radius = 0.034f * (float)Math.Sqrt(star.BaseLuminosity) * (1500f / PhysicalConstants.SUBLIMATION_TEMPERATURE);
            return radius;
        }

        /// <summary>
        /// Calculates a list of resonant orbital positions for potential planets
        /// spaced according to Bode's Law
        /// from the inner edge of the Kuiper Belt until the sublimation radius.
        /// </summary>
        /// <param name="sublimationRadius">The inner edge of the system, which serves as the limit for orbital positions.</param>
        /// <param name="kuiperEdge">The outer edge of the Kuiper Belt, which serves as the limit for orbital positions.</param>
        /// <returns>A list of orbital radii (in AU) for planet placement.</returns>
        private static List<float> CalculateOrbitalPositions(float sublimationRadius, float kuiperEdge)
        {
            List<float> positions = new List<float>();
            float currentPosition = kuiperEdge;
            float divisor = 1.7f;
            int maxIterations = 30;

            for (int i = 0; i < maxIterations; i++)
            {
                // Stop adding further positions once we reach the sublimation radius
                if (currentPosition < sublimationRadius) break;

                positions.Add(currentPosition);
                currentPosition /= divisor; // Shrink toward the inner system
            }

            if (positions.Count == maxIterations)
            {
                Logger.LogWarning("System Generation", "Orbital position generation hit maximum iteration cap.");
            }
            Logger.Log("System Generation", $"Selected Positions: {string.Join(", ", positions)}");

            Logger.Log("System Generation", $"Orbital Positions: {positions.Count}");
            return positions;
        }

        /// <summary>
        /// Instantiates planetary bodies at selected orbital positions using the given parameters,
        /// including orbital eccentricity, inclination, metalicity, and planetary ordering.
        /// </summary>
        /// <param name="seed">Seed value for deterministic randomization.</param>
        /// <param name="count">The number of planets to generate.</param>
        /// <param name="positions">List of valid orbital positions for placement.</param>
        /// <param name="eccentricity">Mean eccentricity of planetary orbits.</param>
        /// <param name="inclination">Maximum inclination variance allowed.</param>
        /// <param name="metalicity">Stellar metalicity used in planetary type selection.</param>
        /// <param name="planetOrder">The overall orbital pattern (e.g. similar, mixed).</param>
        /// <param name="solidMass">Estimated protoplanetary disk mass that isn't gas</param>
        /// <returns>A list of instantiated planetary <see cref="BaseProperties"/>.</returns>
        private static List<BodyProperties> GeneratePlanetsFromPositions(StarProperties star, int seed, int count, List<float> positions, float eccentricity, float inclination, float metalicity, PlanetOrder planetOrder, decimal solidMass, float solidsFraction, decimal diskMass)
        {
            int minCount = Math.Min(count, positions.Count);
            Logger.Log("System Generation", $"Generating Planets for {star.SeedValue}");

            List<BodyProperties> planets = new List<BodyProperties>();

            // Produce deviation factors for all the planets
            List<float> factors = StructureSystem(planetOrder, seed, minCount);

            // Normalise to add up to 100%
            float factorSum = factors.Sum();
            var coreMasses = new decimal[minCount];
            for (int i = 0; i < minCount; i++)
            {
                coreMasses[i] = solidMass * (decimal)(factors[i] / factorSum);
            }


            // Select the positions that will be populated
            List<float> availablePositions = new List<float>(positions);
            List<float> selectedPositions = new List<float>(minCount);
            
            for (int i = 0; i < minCount; i++)
            {
                int index = RandomUtils.RandomInt(0, availablePositions.Count -1, seed + i);
                selectedPositions.Add(availablePositions[index]);
                availablePositions.RemoveAt(index);
            }

            // Generate the required planets and slot into those positions
            for (int p = 0; p < minCount; p++)
            {
                int planetSeed = seed + p;
                float position = selectedPositions[p];
                decimal coreMass = coreMasses[p];

                // Generate the planet's properties
                // Orbital parameters
                OrbitalProperties orbit = PhysicsUtils.ConstructOrbitProperties(planetSeed, position, eccentricity, inclination);
                
                Debug.Log($"Orbital Properties: {orbit.GetInfo()}");

                // Estimate surface composition
                BodyProperties newPlanet = PlanetGen.Generate(planetSeed, star, orbit, coreMass, solidsFraction, diskMass);
                newPlanet.Parent = star.SeedValue;

                planets.Add(newPlanet);

                Logger.Log("Planet Generation", $"Created Planet at {position} AU");
            }

            return planets;
        }


        /// <summary>  
        /// Generates a list of deviation factors for planetary mass distribution based on the specified planetary order.  
        /// - For SIMILAR order: Uses a log-normal distribution to create similar-sized planets.  
        /// - For ORDERED or ANTI_ORDERED: Sorts the factors in ascending or descending order with slight shuffling.  
        /// - For MIXED order: Produces a random distribution without sorting.  
        /// </summary>  
        /// <param name="order">The planetary order type (e.g., SIMILAR, ORDERED, ANTI_ORDERED, MIXED).</param>  
        /// <param name="seed">Seed value for deterministic randomization.</param>  
        /// <param name="count">The number of factors to generate.</param>  
        /// <returns>A list of deviation factors for planetary mass distribution.</returns>
        private static List<float> StructureSystem(PlanetOrder order, int seed, int count)
        {
            List<float> factors = new List<float>();

            // Different processes for similar vs others
            if (order == PlanetOrder.SIMILAR)
            {
                for (int i = 0; i < count; i++)
                {
                    int s = seed + i;

                    double u1 = 1.0 - RandomUtils.RandomFloat(0f, 1f, s);
                    double u2 = 1.0 - RandomUtils.RandomFloat(0f, 1f, s + 1);
                    double z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
                    factors.Add((float)Math.Exp(0.9 * z));
                }

                return factors;
            } 
            else
            {
                for (int i = 0; i < count; i++)
                {
                    int s = seed + i;

                    double u1 = 1.0 - RandomUtils.RandomFloat(0f, 1f, s);
                    double u2 = 1.0 - RandomUtils.RandomFloat(0f, 1f, s + 1);

                    // Get standard normal sample using inverse transform
                    double standardNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);

                    double scaledNormal = 0.1 * standardNormal;

                    double result = Math.Exp(9 * scaledNormal);
                    factors.Add((float)result);
                }
            }

            // Sorting the planets kinda to be ordered or anti-ordered systems
            if (order == PlanetOrder.ORDERED || order == PlanetOrder.ANTI_ORDERED)
            {
                // Apply soft sort
                var indexed = factors.Select((val, i) => new { Index = i, Value = val }).ToList();

                // First sort strictly
                indexed.Sort((a, b) => order == PlanetOrder.ORDERED ? b.Value.CompareTo(a.Value) : a.Value.CompareTo(b.Value));

                // Introduce local shuffling
                for (int i = 0; i < indexed.Count - 1; i++)
                {
                    if (RandomUtils.RandomFloat(0f, 1f, seed + i) > 0.2f) // 20% chance to swap with neighbor
                    {
                        var temp = indexed[i];
                        indexed[i] = indexed[i + 1];
                        indexed[i + 1] = temp;
                        i++; // skip next to avoid re-swapping
                    }
                }
                
                // Return shuffled values
                return indexed.Select(x => x.Value).ToList();
            }
            else
            {
                // Mixed don't need to be sorted
                return factors;
            }
        }

        

        /// <summary>
        /// Calculates the number of planets for a given star based on stellar mass and a seeded random factor.
        /// </summary>
        /// <param name="star">The parent star for which to generate the planet count.</param>
        /// <param name="seed">Seed value for consistent generation.</param>
        /// <returns>An integer representing the number of planets to be created.</returns>
        private static int GeneratePlanetCount(StarProperties star, int seed)
        {
            int planetCount = (int)Math.Max(Math.Pow(star.StellarMass, 0.3f) * RandomUtils.RandomInt(1, 10, seed), 1);
            Logger.Log("System Generation", "Planet Count: " + planetCount);
            return planetCount;
        }
        

        /// <summary>
        /// Determines the stellar metalicity, protoplanetary disk mass, and overall planetary ordering scheme
        /// based on the star’s age, mass, and randomly derived variables.
        /// </summary>
        /// <param name="star">The star whose attributes influence system arrangement.</param>
        /// <param name="metalicity">Outputs the calculated metalicity value [Fe/H] of the star.</param>
        /// <param name="planetOrder">Outputs the type of orbital order (e.g., similar, ordered).</param>
        /// <param name="diskMass">Outputs the calculated mass of the protoplanetary disk.</param>
        private static void DetermineArrangement(StarProperties star, out float metalicity, out PlanetOrder planetOrder, out decimal diskMass)
        {
            int seed = star.SeedValue;

            // Calculate metalicity ratio based on the star's age Fe/H
            // Metalicity increases as the star is more recently formed but does depend on proximity to the galactic core
            float metalDeviation = RandomUtils.RandomFloat(-0.2f, 0.2f, seed);
            float metalAge;
            if ((float)star.Age >= 8f)
            {
                metalAge = -0.3f;
            }
            else
            {
                metalAge = -(((float)star.Age * 0.04f) - 0.15f);
            }
            metalicity = metalAge + metalDeviation;

            // Estimate the disk mass compared to the star's mass
            // High mass makes a dominant jupiter-like more likely
            float diskMassPercentile = (RandomUtils.RandomFloat((1000 * star.StellarMass), 4000 * star.StellarMass, seed) / 1000000) / star.StellarMass;
            diskMass = (decimal)diskMassPercentile * star.Mass;

            // Preset percentages for the planet order
            float draw = RandomUtils.RandomFloat(0f, 1f, seed);

            // Default order is similar
            if (metalicity >= 0f && diskMassPercentile > 0.003f)
            {
                // High Metalicity, High Mass
                // 2/10 mixed; 1/10 ordered; 7/10 anti-ordered

                if (draw < 1f / 3f)
                    planetOrder = PlanetOrder.MIXED;
                else if (draw < 2f / 3f)
                    planetOrder = PlanetOrder.ANTI_ORDERED;
                else
                    planetOrder = PlanetOrder.ORDERED;
            }
            else if (metalicity < 0f && diskMassPercentile <= 0.003f)
            {
                // Low Metalicity, High Mass
                // 1/10 mixed; 1/20 ordered or anti-ordered

                if (draw < 0.10f)
                    planetOrder = PlanetOrder.MIXED;
                else if (draw < 0.15f)
                    planetOrder = PlanetOrder.ANTI_ORDERED;
                else if (draw < 0.20f)
                    planetOrder = PlanetOrder.ORDERED;
                else
                    planetOrder = PlanetOrder.SIMILAR;
            }
            else
            {
                // Low Metalicity, Low Mass
                // e.g. metal ≥ 0 but disk ≤ 0.3% → fallback to ‘Similar’
                planetOrder = PlanetOrder.SIMILAR;
            }

            Logger.Log("System Generation", $"Star {star.SeedValue} order: {planetOrder}");
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