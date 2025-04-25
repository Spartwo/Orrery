
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
            this.seedValue = seedValue;
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

        // Unified value in kilotons
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
                    orbitLine = ColourUtils.ColorToArray(RandomUtils.RandomColor(seedValue));
                }

                // Return a color made from the RGB elements of the array
                return orbitLine;
            }
            set
            {

                Logger.Log("SystemGenerator", $"Setting Colour To {value[0]},{value[1]},{value[2]}");
                orbitLine = value;
            }
        }

        /// <summary>
        /// Adds a new child body to the list of child bodies.
        /// </summary>
        /// <param name="newChild">The new body to be added to the child list</param>
        public void AddChild(BodyProperties newChild)
        {
            // Check if a body with the same seedValue isn't already present
            if (!childBodies.Any(child => child.SeedValue == newChild.SeedValue))
            {
                childBodies.Add(newChild);
            }
        }

        /// <summary>
        /// Removes a child body from the list by its seed value.
        /// </summary>
        /// <param name="seedValue">The seed value of the body to be removed</param>
        /// <returns>The removed body if found, otherwise null</returns>
        public BodyProperties RemoveChild(int seedValue)
        {
            // Find the body with the matching seedValue
            BodyProperties bodyToRemove = childBodies.FirstOrDefault(child => child.SeedValue == seedValue);

            if (bodyToRemove != null)
            {
                // Remove the found body from the list
                childBodies.Remove(bodyToRemove);
                return bodyToRemove;
            }
            else
            {
                Logger.LogWarning(GetType().Name, $"Child body {seedValue} not found.");
                return null;
            }
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
                childBodies = value;
            }
        }
        #endregion
    }
}