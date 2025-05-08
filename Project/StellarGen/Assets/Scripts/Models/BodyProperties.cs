using Newtonsoft.Json;
using StellarGenHelpers;
using System;

namespace Models
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class BodyProperties : BaseProperties
    {
        [JsonProperty("Material Composition (%)")] private SurfaceProperties composition;
        [JsonProperty("Radius (Earths)")] private float radius = 0f; // Earths
        [JsonProperty("Atmospheric Composition (%)")] private AtmosphereProperties atmosphere;

        public BodyProperties(int seedValue = 0, string name = null, decimal? age = null, decimal? mass = null, decimal? hillSphere = 0m, int[] orbitLine = null, double? siderealDayLength = null, float? axialTilt = null)
            : base(seedValue, name, age, mass, hillSphere, orbitLine, siderealDayLength, axialTilt)
        {
            base.Name = name ?? "Unnamed Planet";
            this.composition = new SurfaceProperties();
            this.atmosphere = new AtmosphereProperties(0);
        }

        public override string GetInfo()
        {
            return base.GetInfo() +
                   $"Mass: {PhysicsUtils.RawToEarthMass(base.Mass)} Earth masses\nRadius: {radius} Earth radii\n {composition?.GetInfo()} \n {atmosphere?.GetInfo()}";
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