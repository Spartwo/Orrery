using Newtonsoft.Json;
using StellarGenHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using static UnityEditor.FilePathAttribute;

namespace Models
{
    // Atmospheric properties specific to celestial bodies
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class AtmosphereProperties
    {
        // List of elements in the atmosphere with their respective percentages
        [JsonProperty] public List<(Element, float)> elementPercentages { get; set; }

        // Total atmospheric mass in kilotons
        [JsonProperty("Total Mass (Kilotons)")] private decimal totalAtmosphericMass;

        // Predefined elements: molarMass, freezingPoint (K), boilingPoint (K), gasConstant (J/kg·K), latentHeat(J/mol), localisation
        public static readonly Element H2 = new Element(2.016f, 14.01f, 20.28f, 4124f, 0.452f, "#loc_Hydrogen");
        public static readonly Element He = new Element(4.0026f, 0.95f, 4.22f, 2077f, 0.084f, "#loc_Helium");
        public static readonly Element NH3 = new Element(17.031f, 195.45f, 239.82f, 488f, 23.35f, "#loc_Ammonia");
        public static readonly Element CH4 = new Element(16.043f, 90.67f, 111.66f, 518f, 8.19f, "#loc_Methane");
        public static readonly Element H2O = new Element(18.015f, 273.15f, 373.15f, 461.52f, 40.79f, "#loc_WaterVapour");
        public static readonly Element O2 = new Element(31.998f, 54.36f, 90.20f, 259f, 5.0f, "#loc_Oxygen");
        public static readonly Element N = new Element(28.014f, 63.15f, 77.36f, 296f, 7.57f, "#loc_Nitrogen");
        public static readonly Element CO2 = new Element(44.009f, 216.58f, 194.65f, 189f, 1.98f, "#loc_CarbonDioxide");
        public static readonly Element Ar = new Element(39.948f, 83.81f, 87.30f, 208f, 1.5f, "#loc_Argon");
        public static readonly Element Na = new Element(22.990f, 370.87f, 1156.09f, 206f, 2.6f, "#loc_Sodium");
        public static readonly Element Xe = new Element(131.293f, 161.4f, 165.1f, 63f, 12.4f, "#loc_Xenon");



        public AtmosphereProperties(decimal? totalAtmosphericMass = null)
        {
            this.totalAtmosphericMass = totalAtmosphericMass ?? 0m;
            this.elementPercentages = new List<(Element, float)>();
        }
        /// <summary>
        /// Calculates the weighted average of gas constants based on the percentages of elements in the atmosphere.
        /// </summary>
        /// <returns>The weighted average gas constant in J/(kg·K).</returns>
        public double GetAtmosphereGasConstant()
        {
            double totalGasConstant = 0;
            // Iterate through each element and its percentage
            foreach (var (element, percentage) in elementPercentages)
            {
                totalGasConstant += element.GasConstant * (percentage / 100);
            }
            return totalGasConstant;
        }

        /// <summary>
        /// Sets the percentage of a specific element in the atmosphere, ensuring the total percentage does not exceed 100%.
        /// </summary>
        /// <param name="element">The element to set the percentage for.</param>
        /// <param name="percentage">The percentage of the element (0-100).</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the percentage is not between 0 and 100.</exception>
        public void SetElementPercentage(Element element, float percentage)
        {
            if (percentage < 0 || percentage > 100)
                throw new ArgumentOutOfRangeException(nameof(percentage), "Percentage must be between 0 and 100.");

            int existingElementIndex = elementPercentages.FindIndex(e => e.Item1 == element);
            if (existingElementIndex >= 0)
            {
                elementPercentages[existingElementIndex] = (element, percentage);
            }
            else
            {
                elementPercentages.Add((element, percentage));
            }

            // Adjust other elements to ensure the total adds up to 100%
            float totalPercentage = elementPercentages.Sum(e => e.Item2);
            if (totalPercentage > 100)
            {
                float excess = totalPercentage - 100;
                for (int i = 0; i < elementPercentages.Count; i++)
                {
                    if (elementPercentages[i].Item1 != element && elementPercentages[i].Item2 > 0)
                    {
                        float adjustment = Math.Min(elementPercentages[i].Item2, excess);
                        elementPercentages[i] = (elementPercentages[i].Item1, elementPercentages[i].Item2 - adjustment);
                        excess -= adjustment;

                        if (excess <= 0)
                            break;
                    }
                }
            }

            Logger.Log(GetType().Name, $"Updated Atmospheric Composition: {string.Join(", ", elementPercentages.Select(e => $"{e.Item1.Localisation}: {e.Item2}%"))}");
        }

        /// <summary>
        /// Retrieves the percentage of a specific element in the atmosphere.
        /// </summary>
        /// <param name="element">The element to retrieve the percentage for.</param>
        /// <returns>The percentage of the element, or 0 if the element is not present.</returns>
        public float GetElementPercentage(Element element)
        {
            var elementEntry = elementPercentages.Find(e => e.Item1 == element);
            return elementEntry != default ? elementEntry.Item2 : 0;
        }
        /// <summary>
        /// Provides a summary of the atmospheric properties, including total mass and composition.
        /// </summary>
        /// <returns>A string containing the total atmospheric mass and the composition of elements above 0%.</returns>
        public string GetInfo()
        {
            // Print off all elements in the atmosphere above 0%
            string elementsInfo = string.Join(", ", elementPercentages
                .FindAll(e => e.Item2 > 0)
                .ConvertAll(e => $"{Settings.LocalisationProvider.GetLocalisedString(e.Item1.Localisation)}: {e.Item2}%"));
            return $"{Settings.LocalisationProvider.GetLocalisedString("#loc_Compositon_Atmosphere")}: {elementsInfo}\n";
        }

        
        public decimal TotalAtmosphericMass
        {
            get => totalAtmosphericMass;
            set => totalAtmosphericMass = value;
        }
    }

    // Class for atmospheric elements
    [Serializable]
    public class Element
    {
        public float MolarMass { get; }     // Molar mass in g/mol
        public float FreezingPoint { get; } // Freezing point in Kelvin at 1 atm
        public float BoilingPoint { get; }  // Boiling point in Kelvin at 1 atm
        public float GasConstant { get; }   // R gas constant in J/(kg·K)
        public float LatentHeat { get; }    // J/mol (required for phase calc)
        public string Localisation { get; } // Localization association as a string

        public Element(float molarMass, float freezingPoint, float boilingPoint, float gasConstant, float latentHeat, string localisation)
        {
            this.MolarMass = molarMass;
            this.FreezingPoint = freezingPoint;
            this.BoilingPoint = boilingPoint;
            this.GasConstant = gasConstant;
            this.LatentHeat = latentHeat;
            this.Localisation = localisation;
        }

        ///<summary>
        /// Estimates the phase of the element in atmo based on temperature and pressure.
        ///<summary>
        /// <param name="temperature">Averaged temperature of the target atmosphere</param>
        /// <param name="pressureAtm">Maximum(Surface) pressure of the element</param>
        /// <returns>The percentage of the element, or 0 if the element is not present.</returns>
        public Phase GetPhase(float temperature, float pressureAtm)
        {
            float P = pressureAtm * PhysicalConstants.pascalToAtm;

            if (temperature < FreezingPoint)
                return Phase.Solid;

            // Clausius-Clapeyron
            float lnPsat = MathF.Log(PhysicalConstants.pascalToAtm) - (LatentHeat / (float)PhysicalConstants.gasConstantR) * ((1f / temperature) - (1f / BoilingPoint));
            float Psat = (float)Math.Exp(lnPsat);

            if (P < Psat)
                return Phase.Gas;
            else
                return Phase.Liquid;
        }
        public enum Phase { Solid, Liquid, Gas }
    }
}