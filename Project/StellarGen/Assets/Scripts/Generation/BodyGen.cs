
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using RandomUtils = StellarGenHelpers.RandomUtils;
using PhysicsUtils = StellarGenHelpers.PhysicsUtils;
using ColorUtils = StellarGenHelpers.ColorUtils;
using CONST = StellarGenHelpers.PhysicalConstants;
using UnityEngine;
using System.Linq;
using Models;

namespace SystemGen
{
    public class BodyGen
    {

        #region Childcare

        /// <summary>
        /// Base method to generate planets, moons, etc
        /// </summary>
        /// <param name="children">The elements being passed downwards from the inherited classes</param>
        public void GenerateChildren(List<BodyGen> children = null)
        {

        }

        /// <summary>
        /// Generates an orbital color for a body, either randomly or based on its temperature.
        /// </summary>
        /// <param name="seedValue">The seed used for generating a random color if no temperature is provided.</param>
        /// <param name="temperature">
        /// (Optional) The temperature of the body in Kelvin. If provided, the color is determined 
        /// based on the spectral characteristics of the given temperature.
        /// </param>
        /// <returns>An integer array representing the RGB values of the generated color.</returns>
        public static int[] GenerateOrbitalColour(int seedValue, float? temperature = null)
        {
            int[] orbitLine;

            if (temperature.HasValue)
            {
                Color spectralColor = PhysicsUtils.DetermineSpectralColor((int)temperature.Value);
                orbitLine = ColorUtils.ColorToArray(spectralColor);
            }
            else
            {
                orbitLine = ColorUtils.ColorToArray(RandomUtils.RandomColor(seedValue));
            }

            Logger.Log("System Generation", $"Orbit Colour: {string.Join(", ", orbitLine)}");
            return orbitLine;
        }

       /* /// <summary>
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
        public BodyGen RemoveChild(int seedValue)
        {
            // Find the body with the matching seedValue
            BodyGen bodyToRemove = childBodies.FirstOrDefault(child => child.SeedValue == seedValue);

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
        }*/

        #endregion
    }

}