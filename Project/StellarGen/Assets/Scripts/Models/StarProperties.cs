using System;
using Newtonsoft.Json;
using StellarGenHelpers;
using Unity.VisualScripting;
using UnityEngine;

namespace Models
{
    [Serializable]
    public class StarProperties : BodyProperties
    {
        private decimal lifespan;
        private float radius;
        private float stellarMass;
        private float luminosity;
        private short temperature;

        [JsonIgnore]
        private float baseLuminosity;

        public StarProperties(int seedValue = 0, string name = null, decimal? age = null, decimal? mass = null, decimal? hillSphere = 0m, int[] orbitLine = null, double? siderealDayLength = null, float? axialTilt = null, decimal? lifespan = null, float? radius = null, float? luminosity = null, float? baseLuminosity = null, float? stellarMass = null, short? temperature = null) : base(seedValue, name, age, mass, hillSphere, orbitLine, siderealDayLength, axialTilt)
        {
            // Set specific properties for the Star
            this.radius = radius ?? 0f;  // Default to 0 if not provided
            this.temperature = temperature ?? 0;  // Default to 0 if not provided
            this.lifespan = lifespan ?? 0m;  // Default to 0 if not provided
            this.stellarMass = stellarMass ?? 0f;  // Default to 0 if not provided
            this.luminosity = luminosity ?? 0f;  // Default to 0 if not provided
            this.baseLuminosity = baseLuminosity ?? 0f;  // Default to 0 if not provided
        }

        public override string GetInfo()
        {
            return base.GetInfo() + $"\n" +
                   $"Stellar Mass: {StellarMass} Solar masses\n" +
                   $"Luminosity: {Luminosity} L☉\n" +
                   $"Surface Temperature: {Luminosity} K\n" +
                   $"Diameter: {Radius} Solar Radii\n" +
                   $"Lifespan: {Lifespan} billion years";
        }

        /// <summary>
        /// From Initial mass and age, determine the properties of the star at the current time
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="age"> age is unchanging and inherited from the system at large </param>
        public void GenerateStarProperties(float inputMass)
        {
            stellarMass = inputMass;
            // Set the raw mass
            base.Mass = PhysicsUtils.SolMassToRaw(inputMass);

            Logger.Log(GetType().Name, $"Generating Star Properties");

            // Radius at formation
            float baseRadius = Mathf.Pow(stellarMass, 0.7f) * 0.8f;
            // Surface temperature is constant through the main sequence
            temperature = (short)(Mathf.Pow(baseRadius * 1.25f, 0.54f) * PhysicalConstants.SOLAR_TEMPERATURE);

            // Estimate main sequence lifespan
            lifespan = (decimal)(Math.Pow(inputMass, -2.5)) * 10;
            // Age Adjustment is % though lifespan
            float ageAdjustment = (float)(base.Age / (decimal)lifespan);

            // Set Luminosity and Diameter now  knowing the age
            radius = baseRadius + ((baseRadius / 2) * ageAdjustment);
            // Base luminosity controls formation materials while age adjusted is current atmospheric states
            baseLuminosity = Mathf.Pow(baseRadius, 2) * Mathf.Pow(((float)Temperature / PhysicalConstants.SOLAR_TEMPERATURE), 4);
            luminosity = Mathf.Pow(radius, 2) * Mathf.Pow(((float)Temperature / PhysicalConstants.SOLAR_TEMPERATURE), 4);
        }

        #region Getters and Setters

        // Lifespan is derivative of Formation Diameter and Luminosity, it is not directly set
        [JsonIgnore]
        public decimal Lifespan
        {
            get => lifespan;
        }

        [JsonIgnore]
        public float StellarMass
        {
            get => stellarMass;
        }

        // Diameter, Temperature, and Luminosity are derivative of Lifespan and Mass, they are not directly set

        [JsonIgnore]
        public float Radius
        {
            get => radius;
        }

        [JsonIgnore]
        public float Luminosity
        {
            get => luminosity;
        }

        [JsonIgnore]
        public float BaseLuminosity
        {
            get => baseLuminosity;
        }

        [JsonIgnore]
        public short Temperature
        {
            get => temperature;
        }
        #endregion
    }
}