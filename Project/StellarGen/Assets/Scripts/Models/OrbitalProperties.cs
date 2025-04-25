using System;

namespace Models
{
    // Orbital properties are a subvalue of all bodies
    [Serializable]
    public class OrbitalProperties
    {
        private decimal semiMajorAxis; // Semi-Major Axis
        private float eccentricity; // Eccentricity
        private float longitudeOfAscending; // Longitude of Ascending Node
        private float inclination; // Inclination
        private float periArgument; // Argument of Periapsis

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
            return $"Orbital Properties:\n" +
                   $"Semi-Major Axis: {SemiMajorAxis} AU\n" +
                   $"Eccentricity: {Eccentricity}\n" +
                   $"Inclination: {Inclination}°\n" +
                   $"Longitude of Ascending Node: {LongitudeOfAscending}°\n" +
                   $"Argument of Periapsis: {PeriArgument}°";
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