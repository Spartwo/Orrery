using System;
using System.Collections.Generic;
using System.Xml.Linq;
using static UnityEditor.FilePathAttribute;

namespace Models
{

    [Serializable]
    public class PlanetProperties : BodyProperties
    {
        // Rock, Ice, True Metals
        private Tuple<float, float, float> composition;
        private float radius = 0f; // Earths
        // Atmosphere may not always be present
        private AtmosphereProperties atmosphere = null;

        public PlanetProperties(int seedValue = 0, string name = null, decimal? age = null, decimal? mass = null, decimal? hillSphere = 0m, int[] orbitLine = null, double? siderealDayLength = null, float? axialTilt = null) : base(seedValue, name, age, mass, hillSphere, orbitLine, siderealDayLength, axialTilt)
        {
            // Default composition: 40% Rock, 40% Ice, 20% True Metals
            composition = new Tuple<float, float, float>(40f, 40f, 20f);
        }

        public override string GetInfo()
        {
            return base.GetInfo() + $"\n" + atmosphere?.GetInfo();
        }

        #region Getters and Setters

        public Tuple<float, float, float> Composition
        {
            get => composition;
            set
            {
                if (value.Item1 + value.Item2 + value.Item3 != 100f)
                {
                    throw new ArgumentException("The composition values must add up to 100.");
                }
                composition = value;
            }
        }

        public float Rock
        {
            get => composition.Item1;
            set
            {
                value = Math.Clamp(value, 0f, 100f);
                float remaining = 100f - value;
                float ice = Math.Clamp(composition.Item2, 0f, remaining);
                float metals = remaining - ice;
                composition = new Tuple<float, float, float>(value, ice, metals);
            }
        }

        public float Ice
        {
            get => composition.Item2;
            set
            {
                value = Math.Clamp(value, 0f, 100f);
                float remaining = 100f - value;
                float rock = Math.Clamp(composition.Item1, 0f, remaining);
                float metals = remaining - rock;
                composition = new Tuple<float, float, float>(rock, value, metals);
            }
        }

        public float Metals
        {
            get => composition.Item3;
            set
            {
                value = Math.Clamp(value, 0f, 100f);
                float remaining = 100f - value;
                float rock = Math.Clamp(composition.Item1, 0f, remaining);
                float ice = remaining - rock;
                composition = new Tuple<float, float, float>(rock, ice, value);
            }
        }

        public AtmosphereProperties Atmosphere
        {
            get => atmosphere;
            set => atmosphere = value;
        }

        #endregion
    }
}