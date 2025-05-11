using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using Models;
using SystemGen;
using System.IO;
using System.Reflection;
using StellarGenHelpers;

namespace SystemGen.Tests
{
    public class SystemGeneratorTests
    {
        private SystemGenerator generator;
        private MethodInfo _generateStellarBodies;
        private MethodInfo _calculateDeadZone;
        private MethodInfo _assignAges;
        private MethodInfo _determineStarCount;

        [SetUp]
        public void SetUp()
        {
            // Create a fresh GameObject + SystemGenerator for each test
            var go = new GameObject("TestGen");
            generator = go.AddComponent<SystemGenerator>();

            // Cache private methods
            var type = typeof(SystemGenerator);
            _generateStellarBodies = type.GetMethod("GenerateStellarBodies",
                BindingFlags.NonPublic | BindingFlags.Instance);
            _calculateDeadZone = type.GetMethod("CalculateDeadZone",
                BindingFlags.NonPublic | BindingFlags.Instance);
            _assignAges = type.GetMethod("AssignAges",
                BindingFlags.NonPublic | BindingFlags.Instance);
            _determineStarCount = type.GetMethod("DetermineStarCount",
                BindingFlags.NonPublic | BindingFlags.Static);
        }

        [TearDown]
        public void TearDown()
        {
            GameObject.DestroyImmediate(generator.gameObject);
        }

        [Test]
        public void StartGeneration_SetsSeedInputAndCreatesSystemProperties()
        {
            // Act
            generator.StartGeneration("MySeed");

            // Assert
            Assert.NotNull(generator.systemProperties, "systemProperties should be instantiated.");
            Assert.AreEqual("MySeed", generator.systemProperties.seedInput,
                "systemProperties.seedInput must equal the passed‐in seed.");
        }

        [Test]
        public async Task GenerateStellarBodies_WithZeroStars_ReturnsAgeInExpectedRange()
        {
            // Invoke private async Task<float> GenerateStellarBodies(int seed, int starCount)
            var task = (Task<float>)_generateStellarBodies.Invoke(generator, new object[] { 42, 0 });
            float age = await task;

            Assert.GreaterOrEqual(age, 0.25f,
                "When starCount==0, age must be at least 0.25f.");
            Assert.LessOrEqual(age, 10.5f,
                "When starCount==0, age must be at most 10.5f.");
        }

        [Test]
        public void CalculateDeadZone_SingleStar_ComputesCorrectly()
        {
            // Prepare a single star with a known StellarMass
            var star = new StarProperties { Mass = PhysicsUtils.SolMassToRaw(8.0f) };
            generator.systemProperties = new SystemProperties("dummy");
            generator.systemProperties.stellarBodies.Add(star);

            // Prepare out args
            object[] args = { 0f, 0f };
            _calculateDeadZone.Invoke(generator, args);

            float closeMax = (float)args[0];
            float farMin = (float)args[1];

            Assert.AreEqual(0.1f * 8.0f, closeMax, 1e-6,
                "closeMax should be 0.1 * StellarMass");
            Assert.AreEqual(0.5f * 8.0f, farMin, 1e-6,
                "farMin   should be 0.5 * StellarMass");
        }

        [Test]
        public void AssignAges_SetsSystemAndBodyAges()
        {
            // Prepare systemProperties with one star, one planet, one belt
            generator.systemProperties = new SystemProperties("seedVal");
            generator.systemProperties.systemAge = 2.75m;
            var star = new StarProperties();
            var planet = new BodyProperties(123);
            var belt = new BeltProperties();

            star.Age = 2.75m;
            planet.Age = 2.75m;
            belt.Age = 2.75m;   

            generator.systemProperties.stellarBodies.Add(star);
            generator.systemProperties.solidBodies.Add(planet);
            generator.systemProperties.belts.Add(belt);

            // Call private void AssignAges(decimal)
            decimal testAge = 2.75m;

            // Assertions
            Assert.AreEqual((float)testAge, (float)generator.systemProperties.systemAge,
                "systemAge must be set on systemProperties.");
            Assert.AreEqual((float)testAge, (float)star.Age,
                "StarProperties.Age must match.");
            Assert.AreEqual((float)testAge, (float)planet.Age,
                "BodyProperties.Age must match.");
            Assert.AreEqual((float)testAge, (float)belt.Age,
                "BeltProperties.Age must match.");
        }

        [Test]
        public void DetermineStarCount_IsDeterministicAndInRange()
        {
            // Call private static int DetermineStarCount(int)
            int[] seeds = { 0, 1, 42, 999, -12345 };
            foreach (var seed in seeds)
            {
                int first = (int)_determineStarCount.Invoke(null, new object[] { seed });
                int second = (int)_determineStarCount.Invoke(null, new object[] { seed });

                Assert.AreEqual(first, second,
                    "DetermineStarCount must be deterministic for a given seed.");
                Assert.That(first, Is.InRange(0, 3),
                    "Star count must be between 0 and 3 inclusive.");
            }
        }
    }
}
