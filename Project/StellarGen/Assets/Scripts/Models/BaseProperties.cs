
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
    [JsonObject(MemberSerialization.OptIn)]
    public class BaseProperties
    {
        [JsonProperty("Seed Value")] protected int seedValue;
        [JsonProperty("Parental Seed Value")] private int parent;

        [JsonProperty("Name")] private string name;
        [JsonProperty("Custom Name? (true/false)")] private bool customName;

        [JsonProperty("Age (bYo)")] private decimal age;
        [JsonProperty("Mass (Kilotons)")] private decimal mass;

        [JsonProperty("SOI (m)")] private decimal hillSphere;
        [JsonProperty("Colour (RGB)")] private int[] orbitLine;
        [JsonProperty("Orbit")] private OrbitalProperties orbit;
        [JsonProperty("Rotation")] private SiderealProperties rotation;

        /// <summary>
        /// Generates initial properties from a default state
        /// <param name="seedValue">The seed being passed into the body pre-adjustment</param>
        /// </summary>
        public BaseProperties(int seedValue = 0, string name = null, decimal? age = null, decimal? mass = null, decimal? hillSphere = 0m, int[] orbitLine = null, double? siderealDayLength = null, float? axialTilt = null)
        {
            this.seedValue = seedValue;
            this.name = name ?? "Unnamed Body";  // Default to "Unnamed Body" if not provided
            this.customName = false; // New or Generated names are never custom
            this.age = age ?? 0m;  // Default to 0 if not provided
            this.mass = mass ?? 0m;  // Default to 0 if not provided
            this.hillSphere = hillSphere ?? 0m;  // Default to 0 if not provided

            this.orbitLine = orbitLine ?? new int[] { 255, 255, 255 };  // Default to white
           
            this.rotation = new SiderealProperties(siderealDayLength ?? 24.0, axialTilt ?? 0f);
        }
        public virtual string GetInfo()
        {
            return $"\n{Settings.LocalisationProvider.GetLocalisedString("#loc_Name")}: {Name}\n" +
                   (parent != 0 ? $"{Settings.LocalisationProvider.GetLocalisedString("#loc_ParentID")}: {parent}\n" : string.Empty) +
                   $"{Settings.LocalisationProvider.GetLocalisedString("#loc_Age")}: {Age} bYo\n" +
                   $"{Settings.LocalisationProvider.GetLocalisedString("#loc_HillSphere")}: {HillSphere} AU\n" +
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
                orbitLine = value;
            }
        }

        // Orbital Data
        
        public OrbitalProperties Orbit
        {
            get => orbit;
            set => orbit = value;
        }

        // Sidereal Properties

        public SiderealProperties Sidereal
        {
            get => rotation;
            set => rotation = value;
        }

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