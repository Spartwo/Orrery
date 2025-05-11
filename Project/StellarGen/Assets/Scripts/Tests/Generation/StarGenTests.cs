using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using StellarGenHelpers;
using Models;
using SystemGen;
using UnityEngine;

namespace SystemGen.Tests
{
    public class StarGenTests
    {
        [Test]
        public void Generate_WithZeroSeed_ProducesNonZeroSeedAndStar()
        {
            // Zero seed should be replaced by random non-zero
            var star = StarGen.Generate(0);

            Assert.NotNull(star);
            Assert.AreNotEqual(0, star.SeedValue);
            Assert.Greater(star.StellarMass, 0);
        }

        [Test]
        public void Generate_WithFixedSeed_IsDeterministic()
        {
            int seed = 12345;
            var s1 = StarGen.Generate(seed);
            var s2 = StarGen.Generate(seed);
            Assert.AreEqual(s1.SeedValue, s2.SeedValue);
            Assert.AreEqual(s1.StellarMass, s2.StellarMass);
        }

        [Test]
        public void GenerateStellarMass_WithinExpectedRange()
        {
            // Access private method via reflection
            MethodInfo method = typeof(StarGen).GetMethod("GenerateStellarMass", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(method);

            for (int seed = 0; seed < 10; seed++)
            {
                float mass = (float)method.Invoke(null, new object[] { seed });
                Assert.GreaterOrEqual(mass, 0.1f, "Mass should be at least 0.1");
                Assert.LessOrEqual(mass, 5f, "Mass should be at most 5");
            }
        }

        [Test]
        public void GenerateKuiperBelt_ProducesValidEdges()
        {
            var star = StarGen.Generate(42);
            var belt = StarGen.GenerateKuiperBelt(star);
            Assert.NotNull(belt);
            Assert.Greater(belt.LowerEdge, 0m);
            Assert.Greater(belt.UpperEdge, belt.LowerEdge);
            // Orbit should be set
            Assert.NotNull(belt.Orbit);
        }

        [Test]
        public void CalculateOrbitalPositions_DescendingUntilSublimationRadius()
        {
            // Use reflection to invoke CalculateOrbitalPositions
            MethodInfo method = typeof(StarGen).GetMethod("CalculateOrbitalPositions", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(method);

            float sublimation = 1f;
            float kuiperEdge = 10f;
            var positions = (List<float>)method.Invoke(null, new object[] { sublimation, kuiperEdge });
            Assert.IsTrue(positions.Count > 0);
            // All positions should be >= sublimation
            Assert.IsTrue(positions.All(p => p >= sublimation));
            // Sorted descending
            for (int i = 1; i < positions.Count; i++)
                Assert.Less(positions[i], positions[i - 1]);
        }
    }
}