using Newtonsoft.Json;
using StellarGenHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.FilePathAttribute;

namespace Models
{
    // solid object properties specific to celestial bodies
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class SurfaceProperties
    {
        [JsonProperty][SerializeField] private float rock, ice, metals;

        // Total mass in kilotons
        [JsonProperty("Total Mass (Kilotons)")][SerializeField] private decimal totalSolidMass;

        private SurfaceProperties() : base() { }
        public SurfaceProperties(float? rock = null, float? ice = null, float? metals = null, decimal? totalSolidMass = null)
        {
            // Default composition: 40% Rock, 40% Ice, 20% True Metals
            this.rock = rock ?? 40f;
            this.ice = ice ?? 40f;
            this.metals = metals ?? 20f;
            // Default mass
            this.totalSolidMass = totalSolidMass ?? 0m; 
        }

        /// <summary>
        /// Ensures that rock, ice, and metals together sum to 100%.
        /// </summary>
        private void NormaliseComposition()
        {
            float total = rock + ice + metals;

            rock = rock / total * 100f;
            ice = ice / total * 100f;
            metals = metals / total * 100f;
        }

        /// <summary>
        /// Calculates the density of the object based on its composition.
        /// </summary>
        /// <returns>A density measurement in kg/m^3</returns>
        public float CalculateDensity()
        {
            float density = (rock * PhysicalConstants.ROCK_DENSITY +
                             ice * PhysicalConstants.ICE_DENSITY +
                             metals * PhysicalConstants.METAL_DENSITY) / 100f;
            return density;
        }

        /// <summary>
        /// Calculates the roche coeffficient based on its composition.
        /// </summary>
        public float CalculateRocheCoefficient()
        {
            float coeff = (rock * 1.4f +
                             ice * 1.6f+
                             metals * 1.2f) / 100f;
            return coeff;
        }

        /// <summary>
        /// Provides a summary of the surface composition.
        /// </summary>
        /// <returns>A string containing the percentile composition of values above 0%.</returns>
        public string GetInfo()
        {
            // Print off all elements in the surface composition above 0%
            string elementsInfo = string.Join(", ", new List<(string, float)>
            {
                (Settings.LocalisationProvider.GetLocalisedString("#loc_Rock"), rock),
                (Settings.LocalisationProvider.GetLocalisedString("#loc_Ice"), ice),
                (Settings.LocalisationProvider.GetLocalisedString("#loc_Metals"), metals)
            }
            .FindAll(e => e.Item2 > 0)
            .ConvertAll(e => $"{e.Item1}: {e.Item2}%"));

            return $"{Settings.LocalisationProvider.GetLocalisedString("#loc_Composition_Surface")}: {elementsInfo}\n{Settings.LocalisationProvider.GetLocalisedString("#loc_Density")}: {CalculateDensity()} kg/m^3\"";
        }

        #region getters & setters
        public void SetComposition(float newRock, float newIce, float newMetal)
        {
            rock = newRock;
            ice = newIce;
            metals = newMetal;
            NormaliseComposition();
        }

        
        public float Rock
        {
            get => rock;
            set
            {
                rock = Math.Clamp(value, 0f, 100f);
                float remaining = 100f - rock;
                float totalOther = ice + metals;

                ice = (ice / totalOther) * remaining;
                metals = (metals / totalOther) * remaining;
            }
        }


        public float Ice
        {
            get => ice;
            set
            {
                ice = Math.Clamp(value, 0f, 100f);
                float remaining = 100f - ice;
                float totalOther = rock + metals;

                rock = (rock / totalOther) * remaining;
                metals = (metals / totalOther) * remaining;
            }
        }


        public float Metals
        {
            get => metals;
            set
            {
                metals = Math.Clamp(value, 0f, 100f);
                float remaining = 100f - metals;
                float totalOther = rock + ice;

                rock = (rock / totalOther) * remaining;
                ice = (ice / totalOther) * remaining;
            }
        }

        
        public decimal TotalSolidMass
        {
            get => totalSolidMass;
            set => totalSolidMass = value;
        }
        #endregion
    }
}