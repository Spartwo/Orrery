using System;
using Newtonsoft.Json;
using StellarGenHelpers;
using Unity.VisualScripting;
using UnityEngine;

namespace Models
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class StarProperties : BaseProperties
    {
        [JsonProperty("Lifespan (bY)")] private decimal lifespan;
        [JsonProperty("Radius (Sols)")] private float radius;
        [JsonProperty("Luminosity (Sols)")] private float luminosity;
        [JsonProperty("Temperature (K)")] private short temperature;
         private float baseLuminosity;

        public StarProperties(int seedValue = 0, string name = null, decimal? age = null, decimal? mass = null, decimal? hillSphere = 0m, int[] orbitLine = null, double? siderealDayLength = null, float? axialTilt = null, decimal? lifespan = null, float? radius = null, float? luminosity = null, float? baseLuminosity = null, float? stellarMass = null, short? temperature = null) : base(seedValue, name, age, mass, hillSphere, orbitLine, siderealDayLength, axialTilt)
        {
            base.Name = name ?? "Unnamed Star";
            // Set specific properties for the Star
            this.radius = radius ?? 0f;  // Default to 0 if not provided
            this.temperature = temperature ?? 0;  // Default to 0 if not provided
            this.lifespan = lifespan ?? 0m;  // Default to 0 if not provided
            this.luminosity = luminosity ?? 0f;  // Default to 0 if not provided
            this.baseLuminosity = baseLuminosity ?? 0f;  // Default to 0 if not provided
        }

        public override string GetInfo()
        {
            return base.GetInfo() + "\n" +
               $"{Settings.LocalisationProvider.GetLocalisedString("#loc_StellarMass")}: {StellarMass} {Settings.LocalisationProvider.GetLocalisedString("#loc_SolarMasses")}\n" +
               $"{Settings.LocalisationProvider.GetLocalisedString("#loc_Luminosity")}: {Luminosity} L☉\n" +
               $"{Settings.LocalisationProvider.GetLocalisedString("#loc_SurfaceTemperature")}: {Temperature} K\n" +
               $"{Settings.LocalisationProvider.GetLocalisedString("#loc_Radius")}: {Radius} {Settings.LocalisationProvider.GetLocalisedString("#loc_SolarRadii")}\n" +
               $"{Settings.LocalisationProvider.GetLocalisedString("#loc_Lifespan")}: {Lifespan} bY\n";

        }

        /// <summary>
        /// From Initial mass determine the properties of the star
        /// </summary>
        /// <param name="mass"></param>
        public void GenerateStarProperties(float inputMass)
        {
            // Set the raw mass
            base.Mass = PhysicsUtils.SolMassToRaw(inputMass);

            Logger.Log(GetType().Name, $"Generating Star Properties");

            // Radius at formation
            float baseRadius = Mathf.Pow(StellarMass, 0.7f) * 0.8f;
            // Surface temperature is constant through the main sequence
            temperature = (short)(Mathf.Pow(baseRadius * 1.25f, 0.54f) * PhysicalConstants.SOLAR_TEMPERATURE);

            // Estimate main sequence lifespan
            lifespan = (decimal)Math.Round((Math.Pow(inputMass, -2.5)) * 10, 3);
            // Age Adjustment is % though lifespan
            float ageAdjustment = (float)(base.Age / (decimal)lifespan);

            // Set Luminosity and Diameter now  knowing the age
            radius = baseRadius + ((baseRadius / 2) * ageAdjustment);
            // Base luminosity controls formation materials while age adjusted is current atmospheric states
            baseLuminosity = Mathf.Pow(baseRadius, 2) * Mathf.Pow(((float)Temperature / PhysicalConstants.SOLAR_TEMPERATURE), 4);
            luminosity = Mathf.Pow(radius, 2) * Mathf.Pow(((float)Temperature / PhysicalConstants.SOLAR_TEMPERATURE), 4);
        }

        /// <summary>
        /// From generated paramaters, determine the properties of the star at the current time
        /// </summary>
        public void GenerateAgedStarProperties()
        {
            GenerateStarProperties(PhysicsUtils.RawToSolMass(base.Mass));
        }

        #region Getters and Setters

        // Lifespan is derivative of Formation Diameter and Luminosity, it is not directly set

        public decimal Lifespan
        {
            get => lifespan;
        }

        
        public float StellarMass
        {
            get => PhysicsUtils.RawToSolMass(base.Mass);
        }

        // Diameter, Temperature, and Luminosity are derivative of Lifespan and Mass, they are not directly set

        
        public float Radius
        {
            get => radius;
        }

        
        public float Luminosity
        {
            get => luminosity;
        }

        
        public float BaseLuminosity
        {
            get => baseLuminosity;
        }

        
        public short Temperature
        {
            get => temperature;
        }
        #endregion
    }
}