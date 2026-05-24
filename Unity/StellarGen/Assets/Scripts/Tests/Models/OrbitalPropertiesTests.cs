using NUnit.Framework;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Tests
{
    
    public class OrbitalPropertiesTests
    {
        [Test]
        public void Constructor_ValidValues_InitializesCorrectly()
        {
            var orbit = new OrbitalProperties(149_597_870_700m, 0.0167f, 348.74f, 7.25f, 114.2f);

            Assert.AreEqual(149_597_870_700m, orbit.SemiMajorAxis);
            Assert.AreEqual(0.0167f, orbit.Eccentricity);
            Assert.AreEqual(348.74f, orbit.LongitudeOfAscending);
            Assert.AreEqual(7.25f, orbit.Inclination);
            Assert.AreEqual(114.2f, orbit.PeriArgument);
        }

        [Test]
        public void Constructor_ClampsEccentricityAndSemiMajorAxis()
        {
            var orbit = new OrbitalProperties(-100m, 5.0f, 0f, 0f, 0f);

            Assert.AreEqual(1m, orbit.SemiMajorAxis); // Floored to 1
            Assert.AreEqual(0.9999f, orbit.Eccentricity); // Clamped to 0.9999f
        }

        [Test]
        public void Setter_SemiMajorAxis_ClampsAndFloors()
        {
            var orbit = new OrbitalProperties(100m, 0f, 0f, 0f, 0f);
            orbit.SemiMajorAxis = -999.99m;

            Assert.AreEqual(1m, orbit.SemiMajorAxis);
        }

        [Test]
        public void Setter_Eccentricity_ClampsToValidRange()
        {
            var orbit = new OrbitalProperties(1m, 0.5f, 0f, 0f, 0f);
            orbit.Eccentricity = 1.5f;

            Assert.AreEqual(0.9999f, orbit.Eccentricity);

            orbit.Eccentricity = -5f;
            Assert.AreEqual(0f, orbit.Eccentricity);
        }

        [Test]
        public void Setter_OtherValues_SetCorrectly()
        {
            var orbit = new OrbitalProperties(1m, 0f, 0f, 0f, 0f);

            orbit.LongitudeOfAscending = 180.5f;
            orbit.Inclination = 45.1f;
            orbit.PeriArgument = 270f;

            Assert.AreEqual(180.5f, orbit.LongitudeOfAscending);
            Assert.AreEqual(45.1f, orbit.Inclination);
            Assert.AreEqual(270f, orbit.PeriArgument);
        }

        [Test]
        public void GetInfo_ReturnsFormattedString()
        {
            var orbit = new OrbitalProperties(149_597_870_700m, 0.02f, 100f, 5f, 50f);

            var info = orbit.GetInfo();
            Assert.IsTrue(info.Contains("Semi-Major Axis:"));
            Assert.IsTrue(info.Contains("Eccentricity: 0.02"));
            Assert.IsTrue(info.Contains("Inclination: 5"));
            Assert.IsTrue(info.Contains("Longitude of Ascending Node: 100"));
            Assert.IsTrue(info.Contains("Argument of Periapsis: 50"));
        }
    }
}