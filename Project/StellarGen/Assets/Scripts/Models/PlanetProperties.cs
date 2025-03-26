using System;
using System.Collections.Generic;
using System.Xml.Linq;
using static UnityEditor.FilePathAttribute;

namespace Models
{

    [Serializable]
    public class PlanetProperties : BodyProperties
    {
        private SiderealProperties rotation;
        public PlanetProperties(int seedValue = 0, string name = null, decimal? age = null, decimal? mass = null, decimal? hillSphere = 0m, int[] orbitLine = null, double? siderealDayLength = null, float? axialTilt = null) : base(seedValue, name, age, mass, hillSphere, orbitLine)
        {
            // Set specific properties for the Planet
            this.rotation = new SiderealProperties(siderealDayLength ?? 24.0, axialTilt ?? 0f);
        }

        public override string GetInfo()
        {
            return base.GetInfo() + $"\n" + rotation?.GetInfo();
        }

        #region Getters and Setters

        // Getter for Sidereal Day Length (Rotation Period)
        public double Rotation
        {
            get => rotation.SiderealDayLength;
            set => rotation.SiderealDayLength = value;
        }

        // Getter for Axial Tilt
        public float AxialTilt
        {
            get => rotation.AxialTilt;
            set => rotation.AxialTilt = value;
        }
        #endregion
    }
}