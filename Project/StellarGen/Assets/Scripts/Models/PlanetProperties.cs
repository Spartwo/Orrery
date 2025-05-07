using System;

namespace Models
{
    [Serializable]
    public class PlanetProperties : BodyProperties
    {
        private SurfaceProperties composition;
        private float radius = 0f; // Earths
        private AtmosphereProperties atmosphere;

        public PlanetProperties(int seedValue = 0, string name = null, decimal? age = null, decimal? mass = null, decimal? hillSphere = 0m, int[] orbitLine = null, double? siderealDayLength = null, float? axialTilt = null)
            : base(seedValue, name, age, mass, hillSphere, orbitLine, siderealDayLength, axialTilt)
        {
            this.composition = new SurfaceProperties();
            this.atmosphere = new AtmosphereProperties(0);
        }

        public override string GetInfo()
        {
            return base.GetInfo() + $"Radius: {radius} earth radii\n {composition?.GetInfo()} \n {atmosphere?.GetInfo()}";
        }

        #region Getters and Setters
        public float Radius
        {
            get => radius;
            set => radius = value;
        }
        public SurfaceProperties Composition
        {
            get => composition;
            set => composition = value;
        }

        public AtmosphereProperties Atmosphere
        {
            get => atmosphere;
            set => atmosphere = value;
        }

        #endregion
    }
}