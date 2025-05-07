using System;
using System.Collections.Generic;
using Random = System.Random;
using UnityEngine;
using Models;
using StellarGenHelpers;
using static StarDataPrototype;
using Unity.VisualScripting;

namespace SystemGen
{
    public static class PlanetGen
    {
        public static PlanetProperties Generate(int seedValue, StarProperties parent, OrbitalProperties orbit, decimal coreMass)
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

            Logger.Log("Planet Generation", $"Planet Seed: {seedValue}");

            if (parent != null)
            {
                //GenerateRogue();
            }

            PlanetProperties newPlanet = new PlanetProperties(seedValue);
            newPlanet.Orbit = orbit;

            //EstimatePlanetMass

            newPlanet.Composition = GeneratePlanetaryComposition(parent, newPlanet, coreMass);
            newPlanet.Atmosphere = GenerateAtmosphereComposition(parent, newPlanet);

            EstimatePlanetRadius(
                PhysicsUtils.RawToEarthMass(newPlanet.Composition.TotalSolidMass),
                newPlanet.Composition.CalculateDensity(),
                PhysicsUtils.RawToEarthMass(newPlanet.Atmosphere.TotalAtmosphericMass),
                CalculatePlanetTemperature(parent, orbit),
                out float coreRadius,
                out float atmRadius,
                out float totalRadius
            );

            newPlanet.Radius = coreRadius;

            newPlanet.Mass = (newPlanet.Composition.TotalSolidMass + newPlanet.Atmosphere.TotalAtmosphericMass);


            return newPlanet;
        }

        /// <summary>
        /// Base method to generate planets, moons, etc
        /// </summary>
        /// <param name="children">The elements being passed downwards from the inherited classes</param>
        public static List<BodyProperties> GenerateMinorChildren(PlanetProperties planet)
        {
            List<BodyProperties> childBodies = new List<BodyProperties>();
            return childBodies;
        }


        private static short CalculatePlanetTemperature(StarProperties star, OrbitalProperties orbit)
        {
            // Calculate the distance from the star in AU
            float distance = PhysicsUtils.ConvertToAU(orbit.SemiMajorAxis);
            // Calculate the temperature using the Stefan-Boltzmann law
            float temperature = Mathf.Sqrt(star.BaseLuminosity / (4 * Mathf.PI * Mathf.Pow(distance, 2)));
            return (short)temperature;
        }


        /// <summary>
        /// Estimates the composition and mass of the planet's atmosphere based on its core mass and distance from the star.
        /// </summary>
        /// <param name="coreMass">The mass of the planet's core in Earth masses.</param>
        /// <param name="planet">The planet object containing its properties.</param>
        /// <param name="star">The star object containing its properties.</param>
        /// <returns> An AtmosphereProperties object containing the estimated composition of the planets atmosphere.</returns>
        private static AtmosphereProperties GenerateAtmosphereComposition(StarProperties star, PlanetProperties planet) 
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
        private static SurfaceProperties GeneratePlanetaryComposition(StarProperties star, PlanetProperties planet, decimal coreMass)
        {
            float frostLine = Mathf.Sqrt(star.BaseLuminosity)*4.8f;
            float sublimationLine = Mathf.Sqrt(star.BaseLuminosity) * 0.034f;
            float earthMasses = PhysicsUtils.RawToEarthMass(coreMass);

            float distance = PhysicsUtils.ConvertToAU(planet.Orbit.SemiMajorAxis);
            int seedValue = planet.SeedValue;

            // Calculate the baseline composition values
            float baselineIce = 0f;
            if (distance < frostLine)
            {
                float term = Math.Max(0f, (distance - sublimationLine) / (frostLine - sublimationLine));
                baselineIce = Math.Max(0f, 76.27810046f * (float)Math.Pow(term, 3.8263522568f) - 0.11f);
            }
            else
            {
                float term = Math.Max(0f, Math.Min(1f, (distance - frostLine) / (14f * frostLine - frostLine)));
                baselineIce = 25f + 45f * (float)Math.Pow(term, 0.35f);
            }

            float baselineMetal = Math.Max(3f, 80f * (float)Math.Exp(-0.85714f * (float)Math.Pow(distance, 1.3205f)));

            Logger.Log("Planet Generation", $"Baseline Materials: Ice: {baselineIce}, Metal: {baselineMetal}");

            // Calculate the composition deviation based on the seed value and body mass
            float sharedDeviation = CalculateCompositionDeviation(earthMasses);

            float ice = baselineIce * (1 + RandomUtils.RandomFloat(-sharedDeviation, sharedDeviation, seedValue));
            float metal = baselineMetal * (1 + RandomUtils.RandomFloat(-sharedDeviation, sharedDeviation, seedValue+1));
            float rock = 100f - ice - metal;

            Logger.Log("Planet Generation", $"Final Materials: Ice: {ice}%, Metal: {metal}%, Rock: {rock}%");

            return new SurfaceProperties(rock, ice, metal, coreMass);
        }

        private static float CalculateCompositionDeviation(float mass)
        {
            float deviation = (0.25f * MathF.Exp(-MathF.Log10(mass + 0.001f) * 1.1f)) * 6;
            Logger.Log("Planet Generation", $"Deviation: {deviation}");
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
            coreRadius = MathF.Pow(coreMass, 0.27f) * Mathf.Pow(solidDensity, 1.2f);


            // Calculate the atmospheric inflation factor based on temperature
            float atmInflation = MathF.Pow(temperature / 650f, PhysicalConstants.ATMOSPHERE_TEMPERATURE_EXPONENT);

            // Calculate the atmosphere radius based on its mass and inflation factor
            if (atmosphereMass <= 4.4f)
                atmRadius = MathF.Pow(atmosphereMass, 0.27f) * atmInflation;
            else if (atmosphereMass <= 127f)
                atmRadius = MathF.Pow(4.4f, 0.27f) * MathF.Pow(atmosphereMass / 4.4f, 0.67f) * atmInflation;
            else
                atmRadius = MathF.Pow(4.4f, 0.27f) * MathF.Pow(127f / 4.4f, 0.67f) * MathF.Pow(atmosphereMass / 127f, -0.06f) * atmInflation;

            // Calculate the total radius as the sum of the core and atmosphere radii
            totalRadius = coreRadius + atmRadius;
        }


        /// <summary>
        /// Calculates the properties of a rogue planet, ejected from its parent system
        /// </summary>
        /// <param name="seedValue">The numerical seed for this </param>
        private static PlanetProperties GenerateRogue(int seedValue, StarProperties parent, OrbitalProperties orbit)
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

            Logger.Log("Planet Generation", $"Planet Seed: {seedValue}");

            if (parent != null)
            {
                //GenerateRogue();
            }

            // GeneratePlanetaryComposition(seedValue, parent, orbit);

            PlanetProperties newPlanet = new PlanetProperties(seedValue);

            newPlanet.Orbit = orbit;

            //EstimatePlanetRadius


            return newPlanet;
        }
    }
}