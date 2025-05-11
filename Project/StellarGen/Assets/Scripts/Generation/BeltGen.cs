using System;
using System.Collections.Generic;
using Random = System.Random;
using UnityEngine;
using Models;
using StellarGenHelpers;
using Unity.VisualScripting;
using static Models.BeltProperties;

namespace SystemGen
{
    public static class BeltGen
    {
        public static BeltProperties Generate(int seedValue, StarProperties parent, OrbitalProperties orbit, decimal totalMass, float lowerEdge, float upperEdge)
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

            BeltProperties newBelt = new BeltProperties(seedValue, PhysicsUtils.ConvertToMetres(lowerEdge), PhysicsUtils.ConvertToMetres(upperEdge));
            newBelt.Orbit = orbit;
            newBelt.Mass = totalMass;

            // Estimate Belt composition average at inner, centre, and outer ranges
            newBelt.MeanComposition = GenerateBeltComposition(parent, newBelt);

            return newBelt;
        }

        /// <summary>
        /// Base method to generate planets, moons, etc
        /// </summary>
        /// <param name="children">The elements being passed downwards from the inherited classes</param>
        public static List<BodyProperties> GenerateMinorChildren(BodyProperties planet)
        {
            List<BodyProperties> moons = new List<BodyProperties>();
            return moons;
        }

        /// <summary>
        /// Estimates the averaged composition of the belt based on its position in the system
        /// </summary>
        /// <param name="planet">The planet object containing its properties.</param>
        /// <param name="star">The star object containing its properties.</param>
        /// <returns> A SurfaceProperties tuple containing the estimated composition of the belt over its range</returns>
        private static BeltComposition GenerateBeltComposition(StarProperties star, BeltProperties belt)
        {
            float frostLine = (float)Math.Sqrt(star.BaseLuminosity)*4.8f;
            float sublimationLine = (float)Math.Sqrt(star.BaseLuminosity) * 0.034f;

            int seedValue = belt.SeedValue;
            decimal beltMass = belt.Mass;

            // Set the lerp values for the belt
            float[] distances = new float[] { PhysicsUtils.ConvertToAU(belt.LowerEdge), PhysicsUtils.ConvertToAU(belt.Orbit.SemiMajorAxis), PhysicsUtils.ConvertToAU(belt.UpperEdge) };
            SurfaceProperties[] compositions = new SurfaceProperties[distances.Length];

            // Calculate baseline composition values for 3 ranges
            for (int i = 0; i < distances.Length; i++)
            {
                // Calculate the baseline composition values
                float ice = 0f;
                float distance = distances[i];
                if (distance < frostLine)
                {
                    float term = Math.Max(0f, (distance - sublimationLine) / (frostLine - sublimationLine));
                    ice = Math.Min(Math.Max(0f, 76.27810046f * (float)Math.Pow(term, 3.8263522568f) - 0.11f), 37.9f);
                }
                else
                {
                    float term = Math.Max(0f, Math.Min(1f, (distance - frostLine) / (14f * frostLine - frostLine)));
                    ice = 25f + 45f * (float)Math.Pow(term, 0.35f);
                }

                float metal = Math.Max(3f, 80f * (float)Math.Exp(-0.85714f * (float)Math.Pow(distance, 1.3205f)));

                float rock = 100f - (ice + metal);

                // Shimmy the mass around a bit
                decimal takenMass = beltMass * (decimal)RandomUtils.RandomFloat(0.1f, i/5, seedValue);
                beltMass -= takenMass;

                compositions[i] = new SurfaceProperties(rock, ice, metal, takenMass);
            }

            decimal multipler = (belt.Mass - beltMass) / belt.Mass;
            // Normalize the compositions to ensure they sum to 100%
            for (int i = 0; i < compositions.Length; i++)
            {
                compositions[i].TotalSolidMass = compositions[i].TotalSolidMass * multipler;
            }

            return new BeltComposition(compositions[0], compositions[1], compositions[2]);
        }
    }
}