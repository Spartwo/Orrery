using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Models
{
    // Sidereal properties specific to celestial bodies
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class SiderealProperties
    {
        [JsonProperty("Day Length (Hours)")][SerializeField] private double siderealDayLength; // Sidereal Day Length (in hours)
        [JsonProperty("Axial Tilt (Degrees)")][SerializeField] private float axialTilt; // Sidereal Longitude (degrees)

        private SiderealProperties() : base() { }
        public SiderealProperties(double siderealDayLength, float axialTilt)
        {
            SiderealDayLength = siderealDayLength;  // Sidereal day cannot be zero or negative
            AxialTilt = axialTilt;
        }

        public string GetInfo()
        {
            return $"{Settings.LocalisationProvider.GetLocalisedString("#loc_Sidereal_Properties")}:\n" +
                   $"{Settings.LocalisationProvider.GetLocalisedString("#loc_SiderealDayLength")}: {SiderealDayLength} {Settings.LocalisationProvider.GetLocalisedString("#loc_Hours")}\n" +
                   $"{Settings.LocalisationProvider.GetLocalisedString("#loc_AxialTilt")}: {AxialTilt}°";
        }

        #region Getter and Setters

        public double SiderealDayLength
        {
            get => siderealDayLength;
            set => siderealDayLength = Math.Max(value, 0.001);  // Ensures sidereal day is not too small
        }

        
        public float AxialTilt
        {
            get => axialTilt; 
            set
            {
                // Ensure the value is in the range 0-360
                float normalizedValue = value % 360;
                if (normalizedValue < 0) normalizedValue += 360; // Adjust for negative values

                // Ensure axial tilt is between 0 and 180 degrees
                axialTilt = normalizedValue >= 180 ? 360 - normalizedValue : normalizedValue;
            }
        }

        #endregion
    }
}