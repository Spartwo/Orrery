using System;
using System.Collections.Generic;
using System.Xml.Linq;
using static UnityEditor.FilePathAttribute;

namespace Models
{

    [Serializable]
    public class PlanetProperties : BodyProperties
    {
        private Tuple<float, float, float> composition;
        // Atmosphere may not always be present
        private AtmosphereProperties atmosphere;
        public PlanetProperties(int seedValue = 0, string name = null, decimal? age = null, decimal? mass = null, decimal? hillSphere = 0m, int[] orbitLine = null, double? siderealDayLength = null, float? axialTilt = null) : base(seedValue, name, age, mass, hillSphere, orbitLine, siderealDayLength, axialTilt)
        {

        }

        public override string GetInfo()
        {
            return base.GetInfo() + $"\n" + atmosphere?.GetInfo();
        }

        #region Getters and Setters

        
        #endregion
    }
}