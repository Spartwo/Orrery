using System;
using Newtonsoft.Json;

namespace Models
{
    [Serializable]
    public class StarProperties : BodyProperties
    {
        private float lifespan;
        private float diameter;
        private float luminosity;
        private float stellarMass;

        public StarProperties(int seedValue = 0, string name = null, decimal? age = null, decimal? mass = null, decimal? hillSphere = 0m, int[] orbitLine = null, float? lifespan = null, float? diameter = null, float? luminosity = null, float? stellarMass = null) : base(seedValue, name, age, mass, hillSphere, orbitLine)
        {
            // Set specific properties for the Star
            this.lifespan = lifespan ?? 0f;  // Default to 0 if not provided
            this.diameter = diameter ?? 0f;  // Default to 0 if not provided
            this.luminosity = luminosity ?? 0f;  // Default to 0 if not provided
            this.stellarMass = stellarMass ?? 0f;  // Default to 0 if not provided
        }

        public override string GetInfo()
        {
            return base.GetInfo() + $"\n" +
                   $"Stellar Mass: {StellarMass} Solar masses\n" +
                   $"Luminosity: {Luminosity} L☉\n" +
                   $"Diameter: {Diameter} Solar diameters\n" +
                   $"Lifespan: {Lifespan} billion years";
        }

        #region Getters and Setters

        [JsonIgnore]
        public float Lifespan
        {
            get => lifespan;
            set => lifespan = value;
        }

        [JsonIgnore]
        public float StellarMass
        {
            get => stellarMass;
            set => stellarMass = value;
        }

        // Diameter and Luminosity are derivative of Lifespan and Mass, they are not directly set

        [JsonIgnore]
        public float Diameter
        {
            get => diameter;
            set => stellarMass = value;
        }

        [JsonIgnore]
        public float Luminosity
        {
            get => luminosity;
            set => stellarMass = value;
        }
        #endregion
    }
}