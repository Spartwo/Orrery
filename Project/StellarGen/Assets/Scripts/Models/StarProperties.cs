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
        private float diameter;
        private float stellarMass;
        private float luminosity;
        private int temperature;

        public StarProperties(int seedValue = 0, string name = null, decimal? age = null, decimal? mass = null, decimal? hillSphere = 0m, int[] orbitLine = null, decimal? lifespan = null, float? diameter = null, float? luminosity = null, float? stellarMass = null, int? temperature = null) : base(seedValue, name, age, mass, hillSphere, orbitLine)
        {
            // Set specific properties for the Star
            this.diameter = diameter ?? 0f;  // Default to 0 if not provided
            this.temperature = temperature ?? 0;  // Default to 0 if not provided
            this.lifespan = lifespan ?? 0m;  // Default to 0 if not provided
            this.stellarMass = stellarMass ?? 0f;  // Default to 0 if not provided
            this.luminosity = luminosity ?? 0f;  // Default to 0 if not provided
        }

        public override string GetInfo()
        {
            return base.GetInfo() + $"\n" +
                   $"Stellar Mass: {StellarMass} Solar masses\n" +
                   $"Luminosity: {Luminosity} L☉\n" +
                   $"Surface Temperature: {Luminosity} K\n" +
                   $"Diameter: {Diameter} Solar diameters\n" +
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
            base.Mass = PhysicsUtils.MassToRaw(inputMass);

            Logger.Log(GetType().Name, $"Generating Star Properties");

            // Diameter at formation
            float baseDiameter = Mathf.Pow(stellarMass, 0.7f)*0.8f;
            // Surface temperature is constant through the main sequence
            temperature = (int)(Mathf.Pow(baseDiameter*1.25f, 0.54f) * PhysicalConstants.SOLAR_TEMPERATURE);
            // General luminosity from size and mass based temperature
            float baseLuminosity = Mathf.Pow(baseDiameter, 2) * Mathf.Pow(((float)temperature / PhysicalConstants.SOLAR_TEMPERATURE), 4);

            // Estimate main sequence lifespan from diameter divided by luminosity
            lifespan = (decimal)(baseDiameter / baseLuminosity) * 10;
            // Age Ajustment is % though lifespan
            float ageAjustment = (float)(base.Age / (decimal)lifespan);

            // Set Luminosity and Diameter now  knowing the age
            diameter = baseDiameter + ((baseDiameter / 2) * ageAjustment);
            luminosity = Mathf.Pow(diameter, 2) * Mathf.Pow(((float)Temperature / PhysicalConstants.SOLAR_TEMPERATURE), 4);
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
        public float Diameter
        {
            get => diameter;
        }

        [JsonIgnore]
        public float Luminosity
        {
            get => luminosity;
        }

        [JsonIgnore]
        public int Temperature
        {
            get => temperature;
        }
        #endregion
    }
}