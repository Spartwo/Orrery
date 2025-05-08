
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using RandomUtils = StellarGenHelpers.RandomUtils;
using ColourUtils = StellarGenHelpers.ColourUtils;
using System.Linq;
using UnityEngine;

namespace Models
{
    [Serializable]
    public class BeltProperties : BodyProperties
    {
        private SurfaceProperties meanComposition;
        public BeltProperties(int seedValue = 0, string name = null, decimal? age = null, decimal? mass = null, decimal? hillSphere = 0m, int[] orbitLine = null) : base(seedValue, name, age, mass, hillSphere, orbitLine)
        {
            base.Name = name ?? "Unnamed Belt";
            this.meanComposition = new SurfaceProperties();
        }
        public override string GetInfo()
        {
            return base.GetInfo();
        }
    }
}