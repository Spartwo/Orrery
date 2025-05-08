
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using StellarGenHelpers;
using System.Linq;
using UnityEngine;

namespace Models
{
    [Serializable]
    public class BeltProperties : BaseProperties
    {
        // Make the struct public to match the accessibility of the property
        public struct BeltComposition
        {
            public SurfaceProperties Inner;
            public SurfaceProperties Centre;
            public SurfaceProperties Outer;
            public BeltComposition(SurfaceProperties Inner, SurfaceProperties Centre, SurfaceProperties Outer) : this()
            {
                this.Inner = Inner;
                this.Centre = Centre;
                this.Outer = Outer;
            }
        }

        [JsonProperty("Belt Composition")] private BeltComposition meanComposition;
        [JsonProperty("Inner Edge (m)")] private decimal lowerEdge;
        [JsonProperty("Outer Edge (m)")] private decimal upperEdge;

        public BeltProperties(int seedValue = 0, decimal? lowerEdge = 0m, decimal? upperEdge = 0m, string name = null, decimal? age = null, decimal? mass = null, decimal? hillSphere = 0m, int[] orbitLine = null)
            : base(seedValue, name, age, mass, hillSphere, orbitLine)
        {
            this.lowerEdge = lowerEdge ?? 0m;  // Default to 0 if not provided
            this.upperEdge = upperEdge ?? 0m;  // Default to 0 if not provided
            base.Sidereal = null; // No rotation for belts
            base.Name = name ?? "Unnamed Belt";
        }

        public override string GetInfo()
        {
            var baseInfo = base.GetInfo();
            var compositionInfo = $"Mean Composition: {meanComposition.Inner.GetInfo()}, {meanComposition.Centre.GetInfo()}, {meanComposition.Outer.GetInfo()}";
            var rangeInfo = $"Inner Range: {PhysicsUtils.ConvertToAU(lowerEdge)} AU, Outer Range: {PhysicsUtils.ConvertToAU(upperEdge)} AU";
            return $"{baseInfo}\n{compositionInfo}\n{rangeInfo}";
        }

        public void SetCompositon(SurfaceProperties inner, SurfaceProperties centre, SurfaceProperties outer)
        {
            meanComposition.Inner = inner;
            meanComposition.Centre = centre;
            meanComposition.Outer = outer;
        }
        public BeltComposition MeanComposition
        {
            get => meanComposition;
            set => meanComposition = value;
        }
        public decimal UpperEdge
        {
            get => upperEdge;
            set => upperEdge = value;
        }

        public decimal LowerEdge
        {
            get => lowerEdge;
            set => lowerEdge = value;
        }
    }
}