using System;
using System.Collections.Generic;
using Random = System.Random;
using UnityEngine;
using Models;
using StellarGenHelpers;
using Unity.VisualScripting;

namespace SystemGen
{
    public static class PlanetGen
    {
        public static BodyProperties Generate(int seedValue, StarProperties parent, OrbitalProperties orbit, decimal coreMass)
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

            BodyProperties newPlanet = new BodyProperties(seedValue);
            newPlanet.Orbit = orbit;

            // EstimatePlanetMass

            newPlanet.Composition = GeneratePlanetaryComposition(parent, newPlanet, coreMass);
            newPlanet.Atmosphere = GenerateAtmosphereComposition(parent, newPlanet);

            EstimatePlanetRadius(
                PhysicsUtils.RawToEarthMass(newPlanet.Composition.TotalSolidMass),
                newPlanet.Composition.CalculateDensity(),
                PhysicsUtils.RawToEarthMass(newPlanet.Atmosphere.TotalAtmosphericMass),
                PhysicsUtils.CalculateBodyTemperature(parent, orbit),
                out float coreRadius,
                out float atmRadius,
                out float totalRadius
            );

            newPlanet.Radius = totalRadius;
            newPlanet.Mass = (newPlanet.Composition.TotalSolidMass + newPlanet.Atmosphere.TotalAtmosphericMass);

            newPlanet.Sidereal = GenerateSiderialProperties(parent, newPlanet);

            return newPlanet;
        }

        /// <summary>
        /// Generate the siderial properties of the planet
        /// </summary>
        private static SiderealProperties GenerateSiderialProperties(StarProperties parent, BodyProperties planet)
        {
            double siderealDayLength = RandomUtils.RandomFloat(0.5f, 24f, planet.SeedValue);
            float axialTilt = RandomUtils.RandomFloat(0f, 180f, planet.SeedValue);
            return new SiderealProperties(siderealDayLength, axialTilt);
        }

        /// <summary>
        /// Base method to generate planets, moons, etc
        /// </summary>
        /// <param name="planet">The properties of the planet being generated for.</param>
        public static List<BodyProperties> GenerateMinorChildren(BodyProperties planet)
        {
            List<BodyProperties> moons = new List<BodyProperties>();
            return moons;
        }

        /// <summary>
        /// Estimates the composition and mass of the planet's atmosphere based on its core mass and distance from the star.
        /// </summary>
        /// <param name="coreMass">The mass of the planet's core in Earth masses.</param>
        /// <param name="planet">The planet object containing its properties.</param>
        /// <param name="star">The star object containing its properties.</param>
        /// <returns> An AtmosphereProperties object containing the estimated composition of the planets atmosphere.</returns>
        private static AtmosphereProperties GenerateAtmosphereComposition(StarProperties star, BodyProperties planet) 
        {
            float coreMass = PhysicsUtils.RawToEarthMass(planet.Composition.TotalSolidMass);

            AtmosphereProperties atmosphere = new AtmosphereProperties(0);

            return atmosphere;
        }

        /// <summary>
        /// Estimates the composition of the planet based on its position in the system
        /// </summary>
        /// <param name="coreMass">The mass of the planet's core in Earth masses.</param>
        /// <param name="planet">The planet object containing its properties.</param>
        /// <param name="star">The star object containing its properties.</param>
        /// <returns> A SurfaceProperties object containing the estimated composition of the planet.</returns>
        private static SurfaceProperties GeneratePlanetaryComposition(StarProperties star, BodyProperties planet, decimal coreMass)
        {
            double frostLine = (float)Math.Sqrt(star.BaseLuminosity)*4.8f;
            double sublimationLine = (float)Math.Sqrt(star.BaseLuminosity) * 0.034f;
            double earthMasses = PhysicsUtils.RawToEarthMass(coreMass);

            double distance = PhysicsUtils.ConvertToAU(planet.Orbit.SemiMajorAxis);
            int seedValue = planet.SeedValue;

            // Calculate the baseline composition values
            float baselineIce = 0f;
            if (distance < frostLine)
            {
                double term = Math.Max(0f, (distance - sublimationLine) / (frostLine - sublimationLine));
                baselineIce = Math.Min(Math.Max(0f, 76.27810046f * (float)Math.Pow(term, 3.8263522568f) - 0.11f), 37.9f);
            }
            else
            {
                double term = Math.Max(0f, Math.Min(1f, (distance - frostLine) / (13f * frostLine)));
                baselineIce = 25f + 45f * (float)Math.Pow(term, 0.35f);
            }

            float baselineMetal = Math.Max(3f, 80f * (float)Math.Exp(-0.85714f * (float)Math.Pow(distance, 1.3205f)));

            // Calculate the composition deviation based on the seed value and body mass
            float sharedDeviation = CalculateCompositionDeviation((float)earthMasses) / 100f;

            float ice = (float)baselineIce * (1 + RandomUtils.RandomFloat(-sharedDeviation, sharedDeviation, seedValue));
            float metal = (float)baselineMetal * (1 + RandomUtils.RandomFloat(-sharedDeviation, sharedDeviation, seedValue+1));
            float rock = 100f - (ice + metal);

            return new SurfaceProperties(rock, ice, metal, coreMass);
        }

        private static float CalculateCompositionDeviation(float mass)
        {
            float deviation = (float)((0.25f * Math.Exp(-Math.Log10(mass + 0.001f) * 1.15f)) * 6);
            return deviation;
        }

        /// <summary>
        /// Estimates the radii of a planet's core, atmosphere, and total size based on its core mass, atmosphere mass, and temperature.
        /// </summary>A
        /// <param name="coreMass">The mass of the planet's core in Earth masses.</param>
        /// <param name="solidDensity">The density of the planet's solid core in kg/m^3.</param>
        /// <param name="atmosphereMass">The mass of the planet's atmosphere in Earth masses.</param>
        /// <param name="temperature">The temperature of the planet in Kelvin.</param>
        /// <param name="coreRadius">The calculated radiusof the planet's core in Earth radii (output).</param>
        /// <param name="atmRadius">The calculated radiusof the planet's atmosphere in Earth radii (output).</param>
        /// <param name="totalRadius">The total calculated radiusof the planet in Earth radii (output).</param>
        public static void EstimatePlanetRadius(float coreMass, float solidDensity, float atmosphereMass, short temperature, out float coreRadius, out float atmRadius, out float totalRadius)
        {
            // Calculate the core radius using a power-law relationship
            coreRadius = (float)(Math.Pow(coreMass, 0.27f) * Math.Pow(5515 / solidDensity, 1.2f));

            // Calculate the atmospheric inflation factor based on temperature
            float atmInflation = (float)(Math.Pow(temperature / 650f, PhysicalConstants.ATMOSPHERE_TEMPERATURE_EXPONENT));

            // Calculate the atmosphere radius based on its mass and inflation factor
            if (atmosphereMass <= 4.4f)
                atmRadius = (float)(Math.Pow(atmosphereMass, 0.27f) * atmInflation);
            else if (atmosphereMass <= 127f)
                atmRadius = (float)(Math.Pow(4.4f, 0.27f) * Math.Pow(atmosphereMass / 4.4f, 0.67f) * atmInflation);
            else
                atmRadius = (float)(Math.Pow(4.4f, 0.27f) * Math.Pow(127f / 4.4f, 0.67f) * MathF.Pow(atmosphereMass / 127f, -0.06f) * atmInflation);

            // Calculate the total radius as the sum of the core and atmosphere radii
            totalRadius = coreRadius + atmRadius;
        }
    }
}