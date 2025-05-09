using NUnit.Framework;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StellarGenHelpers;
using Settings;

namespace Models.Tests
{
    public class StarPropertiesTests
    {
        [Test]
        public void Constructor_SetsAllDefaults_WhenNoArgsProvided()
        {
            var star = new StarProperties();

            Assert.AreEqual("Unnamed Star", star.Name);
            Assert.AreEqual(0f, star.Radius);
            Assert.AreEqual(0f, star.Luminosity);
            Assert.AreEqual(0f, star.BaseLuminosity);
            Assert.AreEqual(0, star.Temperature);
            Assert.AreEqual(0m, star.Lifespan);
        }

        [Test]
        public void GetInfo_ReturnsExpectedString()
        {
            LocalisationProvider.LoadLoc("en-ie");

            var star = new StarProperties(
                name: "TestStar",
                mass: PhysicsUtils.SolMassToRaw(1f),
                luminosity: 1f,
                baseLuminosity: 1f,
                radius: 1f,
                lifespan: 10m,
                temperature: 5778
            );

            string info = star.GetInfo();

            Assert.IsTrue(info.Contains("Stellar Mass: 1"));
            Assert.IsTrue(info.Contains("Luminosity: 1"));
            Assert.IsTrue(info.Contains("Surface Temperature: 5778"));
            Assert.IsTrue(info.Contains("Radius: 1"));
            Assert.IsTrue(info.Contains("Lifespan: 10"));
        }

        [Test]
        public void GenerateStarProperties_SetsPhysicalValuesCorrectly()
        {
            var star = new StarProperties(age: 1m); // simulate 1 billion year old star
            star.GenerateStarProperties(1f); // solar-mass star

            Assert.Greater(star.Temperature, 0);
            Assert.Greater(star.Radius, 0f);
            Assert.Greater(star.Luminosity, 0f);
            Assert.Greater(star.BaseLuminosity, 0f);
            Assert.Greater(star.Lifespan, 0m);
            Assert.AreEqual(1f, star.StellarMass, 0.001f);
        }

        [Test]
        public void GenerateAgedStarProperties_RespectsExistingMass()
        {
            var star = new StarProperties(age: 1m);
            star.GenerateStarProperties(1.2f); // simulate manual generation
            float originalLuminosity = star.Luminosity;

            star.GenerateAgedStarProperties(); // re-generates based on raw mass

            Assert.AreEqual(PhysicsUtils.SolMassToRaw(1.2f), star.Mass);
            Assert.AreEqual(1.2f, star.StellarMass, 0.001f);
            Assert.AreNotEqual(originalLuminosity, 0f);
        }

        [Test]
        public void GenerateStarProperties_LowMassStar_ShouldStillComputeValidValues()
        {
            var star = new StarProperties(age: 1m);
            star.GenerateStarProperties(0.08f); // borderline for hydrogen fusion

            Assert.Greater(star.Temperature, 0);
            Assert.Greater(star.Luminosity, 0);
            Assert.Greater(star.Radius, 0);
            Assert.Greater(star.Lifespan, 0m);
        }

        [Test]
        public void GenerateStarProperties_HighMassStar_ProducesShortLifespan()
        {
            var star = new StarProperties(age: 1m);
            star.GenerateStarProperties(10f);

            Assert.Less(star.Lifespan, 10m); // massive stars burn faster
            Assert.Greater(star.Temperature, 10000); // likely hot O-type
        }

        [Test]
        public void BaseLuminosity_ShouldBeLessThanLuminosity_WhenStarHasAged()
        {
            var star = new StarProperties(age: 5m); // star has aged
            star.GenerateStarProperties(1f);

            Assert.Less(star.BaseLuminosity, star.Luminosity);
        }

        [Test]
        public void GenerateStarProperties_IsDeterministic_ForSameInput()
        {
            var star1 = new StarProperties(age: 1m);
            var star2 = new StarProperties(age: 1m);

            star1.GenerateStarProperties(1f);
            star2.GenerateStarProperties(1f);

            Assert.AreEqual(star1.Temperature, star2.Temperature);
            Assert.AreEqual(star1.Radius, star2.Radius);
            Assert.AreEqual(star1.Luminosity, star2.Luminosity);
        }

        [Test]
        public void GetInfo_ShouldIncludeAllFields()
        {
            LocalisationProvider.LoadLoc("en-ie");

            var star = new StarProperties(age: 1m);
            star.GenerateStarProperties(1f);
            var info = star.GetInfo();

            StringAssert.Contains("Stellar Mass:", info);
            StringAssert.Contains("Luminosity:", info);
            StringAssert.Contains("Surface Temperature:", info);
            StringAssert.Contains("Radius:", info);
            StringAssert.Contains("Lifespan:", info);
        }
    }
}