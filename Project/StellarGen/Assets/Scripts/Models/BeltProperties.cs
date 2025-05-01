
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
        private decimal mass;
        private int[] orbitLine;
        private OrbitalProperties orbit;
        public BeltProperties(int seedValue = 0, string name = null, decimal? age = null, decimal? mass = null, decimal? hillSphere = 0m, int[] orbitLine = null) : base(seedValue, name, age, mass, hillSphere, orbitLine)
        {
            this.mass = mass ?? 0m;  // Default to 0 if not provided
            this.orbitLine = orbitLine ?? new int[] { 255, 255, 255 };  // Default to white
        }
        public override string GetInfo()
        {
            return base.GetInfo() + $"\n" +
                   $"Mass: {Mass} Earth masses\n" +
                   $"Hill Sphere: {HillSphere} AU";
        }
    }
}