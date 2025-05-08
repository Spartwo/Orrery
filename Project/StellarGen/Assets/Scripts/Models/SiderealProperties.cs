using Newtonsoft.Json;
using System;

namespace Models
{
    // Sidereal properties specific to celestial bodies
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class SiderealProperties
    {
        [JsonProperty] private double siderealDayLength; // Sidereal Day Length (in hours)
        [JsonProperty] private float axialTilt; // Sidereal Longitude (degrees)

        // Constructor
        public SiderealProperties(double SiderealDayLength, float AxialTilt)
        {
            this.SiderealDayLength = Math.Max(siderealDayLength, 0.001);  // Sidereal day cannot be zero or negative
            this.AxialTilt = axialTilt;
        }

        public string GetInfo()
        {
            return $"Sidereal Properties:\n" +
                   $"Sidereal Day Length: {SiderealDayLength} hours\n" +
                   $"Axial Tilt: {AxialTilt}°";
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
            set => axialTilt = value;
        }

        #endregion
    }
}