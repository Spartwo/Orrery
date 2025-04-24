
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using RandomUtils = StellarGenHelpers.RandomUtils;
using ColorUtils = StellarGenHelpers.ColorUtils;

namespace Models
{
    [Serializable]
    public class BodyProperties
    {
        protected int seedValue;
        private int parent;

        private string name;
        private bool customName;

        private decimal age;
        private decimal mass;

        private decimal hillSphere;
        private int[] orbitLine;
        private OrbitalProperties orbit;

        private List<BodyProperties> childBodies = new List<BodyProperties>();

        /// <summary>
        /// Generates initial properties from a default state
        /// <param name="seedValue">The seed being passed into the body pre-adjustment</param>
        /// </summary>
        public BodyProperties(int seedValue = 0, string name = null, decimal? age = null, decimal? mass = null, decimal? hillSphere = 0m, int[] orbitLine = null)
        {
            if (seedValue == 0)
            {
                // If no seed is provided then pick one at random
                this.seedValue = RandomUtils.RandomInt(0, int.MaxValue);
            }
            else
            {
                // If it is provided then adjust its value to avoid intersections
                this.seedValue = RandomUtils.TweakSeed(seedValue);
            }

            this.name = name ?? "Unnamed Body";  // Default to "Unnamed Body" if not provided
            this.customName = false; // New or Generated names are never custom
            this.age = age ?? 0m;  // Default to 0 if not provided
            this.mass = mass ?? 0m;  // Default to 0 if not provided
            this.hillSphere = hillSphere ?? 0m;  // Default to 0 if not provided

            this.orbitLine = orbitLine ?? new int[] { 255, 255, 255 };  // Default to white
        }
        public virtual string GetInfo()
        {
            return $"Name: {Name}\n" +
                   (parent != 0 ? $"Parent ID: {parent}\n" : string.Empty) +
                   $"Age: {Age} billion years\n" +
                   $"Mass: {Mass} Earth masses\n" +
                   $"Hill Sphere: {HillSphere} AU" + 
                   Orbit?.GetInfo();
        }

        #region Getters and Setters

        // Body Name for distinction
        public string Name
        {
            get => name;
            set => name = value;
        }
        public bool CustomName
        {
            get => customName;
            set => customName = value;
        }

        // SeedValue of body being orbited
        public int Parent
        {
            get => parent;
            set => parent = value;
        }
        // Unique identifier and generation seed
        public int SeedValue
        {
            get => seedValue;
            private set => seedValue = value;
        }

        // Unified value in earth masses
        public decimal Mass
        {
            get => mass;
            set => mass = value;
        }

        // The range to which this body is the main gravitational point
        [JsonIgnore]
        public decimal HillSphere
        {
            get => hillSphere;
            set => hillSphere = value;
        }

        // Age stored in billions

        public decimal Age
        {
            get => age;
            set => age = value;
        }

        // Orbital Line gets and sets translates from serializeable float array to a color
        public int[] OrbitLine
        {
            get
            {
                // If the array is empty generate a color first
                if (orbitLine == null)
                {
                    orbitLine = ColorUtils.ColorToArray(RandomUtils.RandomColor(seedValue));
                }

                // Return a color made from the RGB elements of the array
                return orbitLine;
            }
            set => orbitLine = value;
        }

        // Orbital Data
        public OrbitalProperties Orbit
        {
            get => orbit;
            set => orbit = value;
        }

        // Child Body Array
        public List<BodyProperties> ChildBodies
        {
            get => childBodies;
            set
            {
                // Provide a default empty list if the list or new list are null
                if (value == null || childBodies == null)
                {
                    childBodies = new List<BodyProperties>();
                }
                Logger.Log(GetType().Name, "Adding " + value.Count + " Children");
                childBodies = value;
            }
        }

        #endregion
    }
}