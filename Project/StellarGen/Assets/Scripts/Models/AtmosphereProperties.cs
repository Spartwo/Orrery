using Newtonsoft.Json;
using StellarGenHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Xml.Linq;
using static UnityEditor.FilePathAttribute;
using UnityEngine;
using UnityEngine.Categorization;

namespace Models
{
    // Atmospheric properties specific to celestial bodies
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class AtmosphereProperties
    {
        // List of elements in the atmosphere with their respective percentages
        [JsonProperty("Atmospheric Elements (%)")]
        [SerializeField]
        public List<AtmosphereElement> Elements { get; private set; }

        // Total atmospheric mass in kilotons
        [JsonProperty("Total Mass (Kilotons)")]
        [SerializeField]
        private decimal totalAtmosphericMass;

        // Predefined elements: molarMass, freezingPoint (K), boilingPoint (K), gasConstant (J/kg·K), latentHeat(J/mol), localisation
        public static readonly Element H2 = new(2.016f, 14.01f, 20.28f, 4124f, 0.452f, "#loc_Hydrogen");
        public static readonly Element He = new(4.0026f, 0.95f, 4.22f, 2077f, 0.084f, "#loc_Helium");
        public static readonly Element CO2 = new(44.009f, 216.58f, 194.65f, 189f, 1.98f, "#loc_CarbonDioxide");
        public static readonly Element N = new(28.014f, 63.15f, 77.36f, 296f, 7.57f, "#loc_Nitrogen");
        public static readonly Element O2 = new(31.998f, 54.36f, 90.20f, 259f, 5.0f, "#loc_Oxygen");
        public static readonly Element NH3 = new(17.031f, 195.45f, 239.82f, 488f, 23.35f, "#loc_Ammonia");
        public static readonly Element CH4 = new(16.043f, 90.67f, 111.66f, 518f, 8.19f, "#loc_Methane");
        public static readonly Element H2O = new(18.015f, 273.15f, 373.15f, 461.52f, 40.79f, "#loc_WaterVapour");
        public static readonly Element Ar = new(39.948f, 83.81f, 87.30f, 208f, 1.5f, "#loc_Argon");
        public static readonly Element Na = new (22.990f, 370.87f, 1156.09f, 206f, 2.6f, "#loc_Sodium");
        public static readonly Element Xe = new(131.293f, 161.4f, 165.1f, 63f, 12.4f, "#loc_Xenon");


        private AtmosphereProperties() : base() { }
        public AtmosphereProperties(decimal? totalAtmosphericMass = null, List<Element> elements = null)
        {
            this.totalAtmosphericMass = totalAtmosphericMass ?? 0m;
            if (elements == null || elements.Count == 0)
            {
                Elements = new List<AtmosphereElement>()
                {
                    new AtmosphereElement(H2,  700),
                    new AtmosphereElement(He,  250),
                    new AtmosphereElement(N,    20),
                    new AtmosphereElement(O2,   10),
                    new AtmosphereElement(CO2,  30),
                    new AtmosphereElement(Ar,   1),
                    new AtmosphereElement(NH3,  1),
                    new AtmosphereElement(CH4,  1),
                    new AtmosphereElement(H2O,  1),
                    new AtmosphereElement(Na,   1),
                    new AtmosphereElement(Xe,   1)
                };
            }
        }


        public float GetAtmosphereGasConstant() 
        {
            // Calculate the weighted average molar mass of the atmosphere
            float totalGasConstant = 0f;
            float totalPercentile = 0f;
            foreach (var element in Elements)
            {
                if (element.Percentile > 0)
                {
                    totalGasConstant += element.Element.GasConstant * (element.Percentile / 100f);
                    totalPercentile += element.Percentile / 100f;
                }
}
            return totalGasConstant / totalPercentile;
        }

        public float GetAtmosphereMolarMass()
        {
            // Calculate the weighted average molar mass of the atmosphere
            float totalMolarMass = 0f;
            float totalPercentile = 0f;
            foreach (var element in Elements)
            {
                if (element.Percentile > 0)
                {
                    totalMolarMass += element.Element.MolarMass * (element.Percentile / 100f);
                    totalPercentile += element.Percentile / 100f;
                }
            }
            return totalMolarMass / totalPercentile;
        }

        /// <summary>
        /// Sets the percentage of a specific element in the atmosphere, ensuring the total percentage does not exceed 100%.
        /// </summary>
        /// <param name="element">The element to set the percentage for.</param>
        /// <param name="percentage">The percentage of the element (0-100).</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the percentage is not between 0 and 100.</exception>
        public void SetElementPercentage(Element element, short pct)
        {
            if (pct < 0 || pct > 100) throw new ArgumentOutOfRangeException(nameof(pct));
            var entry = Elements.Find(e => e.Element == element);
            if (entry != null) entry.Percentile = pct;
            else Elements.Add(new AtmosphereElement(element, pct));

            // Normalise if sum > 100%
            // Normalise all other elements while retaining the provided element's percentage
            int total = Elements.Sum(e => e.Element != element ? e.Percentile : 0);
            if (total > 0)
            {
                int remaining = 100 - pct;
                foreach (var e in Elements.Where(e => e.Element != element && e.Percentile > 0))
                {
                    e.Percentile = (short)((e.Percentile * remaining) / total);
                }
            }
        }

        public void NormaliseElements()
        {
            // Normalize all elements to ensure their total percentage is 100%
            int total = Elements.Sum(e => e.Percentile);
            if (total > 0)
            {
                foreach (var e in Elements)
                {
                    e.Percentile = (short)((e.Percentile * 100) / total);
                }
            }
        }

        /// <summary>
        /// Retrieves the percentage of a specific element in the atmosphere.
        /// </summary>
        /// <param name="element">The element to retrieve the percentage for.</param>
        /// <returns>The percentage of the element, or 0 if the element is not present.</returns>
        public short GetElementPercentage(Element element) =>
            Elements.Find(e => e.Element == element)?.Percentile ?? (short)0;

        /// <summary>
        /// Provides a summary of the atmospheric properties, including total mass and composition.
        /// </summary>
        /// <returns>A string containing the total atmospheric mass and the composition of elements above 0%.</returns>
        public string GetInfo()
        {
            var parts = Elements
                .Where(e => e.Percentile > 0)
                .Select(e => $"{Settings.LocalisationProvider.GetLocalisedString(e.Element.Name)}: {e.Percentile}%");
            return $"{Settings.LocalisationProvider.GetLocalisedString("#loc_Composition_Atmosphere")}: {string.Join(", ", parts)}\n";
        }
        
        public decimal TotalAtmosphericMass
        {
            get => totalAtmosphericMass;
            set => totalAtmosphericMass = value;
        }
    }

    [Serializable]
    public class AtmosphereElement
    {
        [JsonProperty("Element")]
        public Element Element { get; }

        [JsonProperty("Percentile")]
        public short Percentile { get; set; }

        private AtmosphereElement() : base() { }
        public AtmosphereElement(Element element, short percentile)
        {
            Element = element;
            Percentile = percentile;
        }
    }

    // Class for atmospheric elements
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class Element
    {
        public float MolarMass { get; }     // Molar mass in g/mol
        public float FreezingPoint { get; } // Freezing point in Kelvin at 1 atm
        public float BoilingPoint { get; }  // Boiling point in Kelvin at 1 atm
        public float GasConstant { get; }   // R gas constant in J/(kg·K)
        public float LatentHeat { get; }    // J/mol (required for phase calc)
        [JsonProperty] public string Name { get; } // Localization association as a string

        private Element() : base() { }
        public Element(float molarMass, float freezingPoint, float boilingPoint, float gasConstant, float latentHeat, string localisation)
        {
            this.MolarMass = molarMass;
            this.FreezingPoint = freezingPoint;
            this.BoilingPoint = boilingPoint;
            this.GasConstant = gasConstant;
            this.LatentHeat = latentHeat;
            this.Name = localisation;
        }

        ///<summary>
        /// Estimates the escape velocity of the element based on temperature and surface mass.
        ///<summary>
        /// <param name="temperature">Averaged temperature of the target atmosphere</param>
        /// <param name="surfaceMass">Mass of the body being calculated for</param>
        /// <returns>The percentage of the element, or 0 if the element is not present.</returns>
        public bool ExceedsJeanEscape(short temperature, decimal surfaceMass, double radius)
        {
            // Calculate the thermal velocity
            double thermalVelocity = Math.Sqrt(0.35 * PhysicalConstants.GAS_CONSTANT_R * temperature / MolarMass);

            // Calculate the escape velocity
            decimal radiusMetres = (decimal)(radius * PhysicalConstants.EARTH_RADIUS);
            double escapeVelocity = Math.Sqrt(2 * PhysicalConstants.GRAV * (double)(surfaceMass / radiusMetres));

            //Logger.Log("Atmosphere Element", $"Escape Velocity: {escapeVelocity} km/s, Thermal Velocity: {thermalVelocity} km/s");

            // If the ratio is greater than 1, the element can escape
            return (thermalVelocity / escapeVelocity) > 1;
        }

        ///<summary>
        /// Estimates the phase of the element in atmo based on temperature and pressure.
        ///<summary>
        /// <param name="temperature">Averaged temperature of the target atmosphere</param>
        /// <param name="pressureAtm">Maximum(Surface) pressure of the element</param>
        /// <returns>The percentage of the element, or 0 if the element is not present.</returns>
        public Phase GetPhase(float temperature, float pressureAtm)
        {
            float P = pressureAtm * PhysicalConstants.PASCAL_ATM;

            if (temperature < FreezingPoint)
                return Phase.Solid;

            // Clausius-Clapeyron
            float lnPsat = MathF.Log(PhysicalConstants.PASCAL_ATM) - (LatentHeat / (float)PhysicalConstants.GAS_CONSTANT_R) * ((1f / temperature) - (1f / BoilingPoint));
            float Psat = (float)Math.Exp(lnPsat);

            if (P < Psat)
                return Phase.Gas;
            else
                return Phase.Liquid;
        }
        public enum Phase { Solid, Liquid, Gas }
    }
}