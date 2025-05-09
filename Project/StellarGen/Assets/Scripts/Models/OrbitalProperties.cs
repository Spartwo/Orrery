using Newtonsoft.Json;
using System;
using StellarGenHelpers;

namespace Models
{
    // Orbital properties are a subvalue of all bodies
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class OrbitalProperties
    {
        [JsonProperty("Semi-Major Axis (m)")] private decimal semiMajorAxis; // Semi-Major Axis
        [JsonProperty("Eccentricity (0-1)")] private float eccentricity; // Eccentricity
        [JsonProperty("Longitude of Ascending Node")] private float longitudeOfAscending; // Longitude of Ascending Node
        [JsonProperty("Inclination")] float inclination; // Inclination
        [JsonProperty("Argument of Periapsis")] private float periArgument; // Argument of Periapsis

        // Constructor
        public OrbitalProperties(decimal semiMajorAxis, float eccentricity, float longitudeOfAscending, float inclination, float periArgument)
        {
            this.SemiMajorAxis = Math.Floor(Math.Max(semiMajorAxis, 1m));
            this.Eccentricity = Math.Clamp(eccentricity, 0f, 0.9999f);
            this.LongitudeOfAscending = longitudeOfAscending;
            this.Inclination = inclination;
            this.PeriArgument = periArgument;
        }
        public string GetInfo()
        {
            return $"{Settings.LocalisationProvider.GetLocalisedString("#loc_Orbital_Properties")}:\n" +
                   $"{Settings.LocalisationProvider.GetLocalisedString("#loc_SemiMajorAxis")}: {PhysicsUtils.ConvertToAU(SemiMajorAxis)} AU\n" +
                   $"{Settings.LocalisationProvider.GetLocalisedString("#loc_Eccentricity")}: {Eccentricity}\n" +
                   $"{Settings.LocalisationProvider.GetLocalisedString("#loc_Inclination")}: {Inclination}°\n" +
                   $"{Settings.LocalisationProvider.GetLocalisedString("#loc_LongitudeOfAscendingNode")}: {LongitudeOfAscending}°\n" +
                   $"{Settings.LocalisationProvider.GetLocalisedString("#loc_ArgumentOfPeriapsis")}: {PeriArgument}°\n";
        }

        #region Getter and Setters
        
        public decimal SemiMajorAxis
        {
            get => semiMajorAxis;
            set => semiMajorAxis = Math.Floor(Math.Max(value, 1m));
        }

        
        public float Eccentricity
        {
            get => eccentricity;
            // Eccentricity cannot be more than 1
            set => eccentricity = Math.Clamp(value, 0f, 0.9999f);
        }

        
        public float LongitudeOfAscending
        {
            get => longitudeOfAscending;
            set => longitudeOfAscending = value;
        }

        
        public float Inclination
        {
            get => inclination;
            set => inclination = value;
        }

        
        public float PeriArgument
        {
            get => periArgument;
            set => periArgument = value;
        }
        #endregion
    }
}