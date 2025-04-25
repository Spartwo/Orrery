
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using RandomUtils = StellarGenHelpers.RandomUtils;
using PhysicsUtils = StellarGenHelpers.PhysicsUtils;
using ColourUtils = StellarGenHelpers.ColourUtils;
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
        public virtual void GenerateChildren(List<BodyGen> children)
        {

        }

    
        

        /*/// <summary>
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