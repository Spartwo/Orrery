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
        public static BodyProperties Generate(int seedValue, StarProperties parent, OrbitalProperties orbit, decimal coreMass, float solidsFraction, decimal diskMass)
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
            try
            {
                newPlanet.Atmosphere = GenerateAtmosphereComposition(parent, newPlanet, solidsFraction, diskMass);
            }
            catch (Exception e)
            {
                Logger.LogError("Planet Generation", $"Atmosphere generation failed: {e}");
                newPlanet.Atmosphere = new AtmosphereProperties(0);
            }

            EstimatePlanetRadius(
                PhysicsUtils.RawToEarthMass(newPlanet.Composition.TotalSolidMass),
                newPlanet.Composition.CalculateDensity(),
                PhysicsUtils.RawToEarthMass(newPlanet.Atmosphere.TotalAtmosphericMass),
                PhysicsUtils.CalculateBodyTemperature(parent, orbit),
                newPlanet.Atmosphere.GetAtmosphereMolarMass(),
                out double coreRadius,
                out double atmRadius,
                out double totalRadius
            );

            newPlanet.Radius = (float)totalRadius;
            newPlanet.Mass = (newPlanet.Composition.TotalSolidMass + newPlanet.Atmosphere.TotalAtmosphericMass);

            newPlanet.Sidereal = GenerateSiderialProperties(parent, newPlanet);

            return newPlanet;
        }

        /// <summary>
        /// Generate the siderial properties of the planet
        /// </summary>
        private static SiderealProperties GenerateSiderialProperties(StarProperties parent, BodyProperties planet)
        {
            float planetMass = PhysicsUtils.RawToEarthMass(planet.Mass);                      
            float starMass = PhysicsUtils.RawToSolMass(parent.Mass);                      
            float distanceAU = PhysicsUtils.ConvertToAU(planet.Orbit.SemiMajorAxis);
            float ageBillionYr = (float)parent.Age;

            double baselineHours = (double)(Math.Pow(starMass / planetMass, 0.3)
                         * Math.Pow(distanceAU, 0.8)
                         * Math.Sqrt(ageBillionYr)
                         * 24.0);
            Debug.Log($"Planet Generation: {planet.Name} baseline hours: {baselineHours}");

            double baselinetest = (double)((1 / 1)
                        * Math.Pow(2, 0.8)
                        * (double)4.5
                        * 24.0);
            Debug.Log($"Planet Generation: {baselinetest}");

            double siderealDayLength = 4 * baselineHours * RandomUtils.RandomFloat(0.8f, 1.2f, planet.SeedValue);
            
            
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
        /// <param name="solidsFraction">The general metalicity of the stellar system</param>
        /// <returns> An AtmosphereProperties object containing the estimated composition of the planets atmosphere.</returns>
        private static AtmosphereProperties GenerateAtmosphereComposition(StarProperties star, BodyProperties planet, float solidsFraction, decimal diskMass)
        {
            int seed = planet.SeedValue;
            decimal coreMass = planet.Composition.TotalSolidMass;

            AtmosphereProperties atmosphere = new AtmosphereProperties(0);
            float temperature = PhysicsUtils.CalculateBodyTemperature(star, planet.Orbit);
            // Calculate the core radius using a power-law relationship
            float coreRadius = (float)(Math.Pow(PhysicsUtils.RawToEarthMass(coreMass), 0.27f) * Math.Pow(5515f / planet.Composition.CalculateDensity(), 1.2f));


            // Calculate the material abundance factor from the star
            float[] elementCommonalities = new float[] { 0.70f, 0.24f, 0.02f, 0.02f, 0.01f };
            // Adjust the volatiles to star metalicity
            elementCommonalities[2] = (solidsFraction * 100) / (atmosphere.Elements.Count - 3);
            elementCommonalities[3] = (solidsFraction * 100) / (atmosphere.Elements.Count - 3);
            elementCommonalities[4] = (solidsFraction * 100) / (atmosphere.Elements.Count - 3);

            // Calculate the overall gathered atmosphere of any type before stripping
            // Get draw from the disk mass in solar terms
            decimal diskMult = (diskMass / PhysicalConstants.STELLAR_DISK_MASS);
            // Limit the range to 60% of the disk
            decimal maxDraw = diskMass * 0.6m;
            decimal diskDraw = PhysicsUtils.EarthMassToRaw((float)(15.8f * Math.Pow(PhysicsUtils.RawToEarthMass(planet.Composition.TotalSolidMass), 1.06f)));
            decimal atmoBaseMass = Math.Min(diskDraw, maxDraw);

            // Check for thermal escape
            decimal atmoMass = atmoBaseMass;

            // Check each elements viability
            for (int i = 0; i < atmosphere.Elements.Count-1; i++)
            {
                if (atmosphere.Elements[i].Element.ExceedsJeanEscape(temperature, coreMass, coreRadius))
                {
                    // If the element is not viable then set its fraction to 0 and decuct from the total mass
                    atmosphere.SetElementPercentage(atmosphere.Elements[i].Element, 0);
                    atmoMass -= atmoBaseMass * (decimal)elementCommonalities[i];
                }
                else
                {
                    // If the element if viable then estimate it's presence
                    float commonalityBaseline = elementCommonalities[Math.Min(i, 2)];
                    float commonality = commonalityBaseline * RandomUtils.RandomFloat(0.8f, 1.2f, seed + i) * 100;
                    // Adjust the element definition based on the commmonality value
                    atmosphere.SetElementPercentage(atmosphere.Elements[i].Element, (short)commonality);
                }
            }

            // Deviate downwards, lighter atmospheres shift more
            float reduction = (float)(PhysicsUtils.RawToEarthMass(atmoMass) - (Math.Pow(PhysicsUtils.RawToEarthMass(atmoMass), 0.4f) * RandomUtils.RandomFloat(0, 1)));
            
            // If there's no atmosphere don't bother with the rest
            if (reduction < 1)
            {
                if (reduction > 0)
                {
                    reduction = reduction/25;
                }
                else
                {
                    reduction = 0;
                }
            }

            atmosphere.TotalAtmosphericMass = PhysicsUtils.EarthMassToRaw(reduction);

            // Calculate surface gravity
            float gravity = (float)(PhysicalConstants.GRAV * (double)(coreMass * 1000000) / Math.Pow(coreRadius * PhysicalConstants.EARTH_RADIUS, 2)); //m/s
            // Atmospheric column mass per surface unit area
            double sigma = (double)(atmosphere.TotalAtmosphericMass * 1000000) / (4.0 * Math.PI * Math.Pow(coreRadius * PhysicalConstants.EARTH_RADIUS, 2));
            // Estimate the surface pressure
            float atmPressure = (float)((gravity * sigma) / PhysicalConstants.PASCAL_ATM);

            Logger.Log("Planet Generation", $"Atmospheric Pressure: {atmPressure} sur/atm({PhysicsUtils.RawToEarthMass(coreMass)}/{PhysicsUtils.RawToEarthMass(atmosphere.TotalAtmosphericMass)} masses) at {gravity / 9.71f} G");

            // Eliminate any condensed elements
            for (int i = 0; i < atmosphere.Elements.Count-1; i++)
            {
                if (atmosphere.Elements[i].Percentile > 0)
                {
                    Element.Phase phase = atmosphere.Elements[i].Element.GetPhase(temperature, atmPressure);
                    if (phase == Element.Phase.Solid || phase == Element.Phase.Liquid)
                    {
                        // Assume the rest of the atmosphere picks up the load
                        atmosphere.SetElementPercentage(atmosphere.Elements[i].Element, 0);
                    }
                    Debug.Log($"Element {atmosphere.Elements[i].Element.Name} phase: {phase}");
                }
            }

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
        public static void EstimatePlanetRadius(float coreMass, float solidDensity, float atmosphereMass, short temperature, float atmoMolar, out double coreRadius, out double atmRadius, out double totalRadius)
        {
            float baseMolar = 2.617f;
            // Calculate the core radius using a power-law relationship
            coreRadius = (float)(Math.Pow(coreMass, 0.27f) * Math.Pow(5515 / solidDensity, 1.2f));

            // Calculate the atmospheric inflation factor based on temperature and composition
            float temptInflation = (float)(Math.Pow(temperature / 650f, PhysicalConstants.ATMOSPHERE_TEMPERATURE_EXPONENT));
            float molarInflation = (float)2.617f / atmoMolar;
            float atmInflation = temptInflation + molarInflation;

            Logger.Log("Planet Generation", $"Atmospheric Mass: {atmosphereMass} (Base: {coreMass})");

            // Calculate the atmosphere radius based on its mass and inflation factor
            if (atmosphereMass < -0f)
                atmRadius = 0f;
            else if (atmosphereMass <= 4.4f)
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