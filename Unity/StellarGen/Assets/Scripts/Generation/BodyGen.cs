
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
    public static class BodyGen
    {

        #region Childcare

        /// <summary>
        /// Base method to generate planets, moons, etc
        /// </summary>
        /// <param name="children">The elements being passed downwards from the inherited classes</param>
        public static List<BaseProperties> GenerateChildren(BaseProperties body)
        {
            List<BaseProperties> childBodies = new List<BaseProperties>();
            return childBodies;
        }

        public static List<BaseProperties> GenerateMinorChildren(BaseProperties body)
        {
            // Generate a list of minor bodies (e.g., asteroids, comets) based on the parent body
            List<BaseProperties> minorBodies = new List<BaseProperties>();
            return minorBodies;
        }
        #endregion
    }

}