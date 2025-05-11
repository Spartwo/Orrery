using NUnit.Framework;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Settings;

namespace Models.Tests
{
    
    public class SiderealPropertiesTests
    {
        [Test]
        public void Constructor_ValidInput_InitializesCorrectly()
        {
            var sidereal = new SiderealProperties(23.5, 25.2f);

            Assert.AreEqual(23.5, sidereal.SiderealDayLength);
            Assert.AreEqual(25.2f, sidereal.AxialTilt);
        }

        [Test]
        public void Constructor_InvalidDayLength_ClampsToMinimum()
        {
            var sidereal = new SiderealProperties(0, 10f);

            Assert.AreEqual(0.001, sidereal.SiderealDayLength);
        }

        [Test]
        public void Setter_SiderealDayLength_ClampsToMinimum()
        {
            var sidereal = new SiderealProperties(10, 0);
            sidereal.SiderealDayLength = -50;

            Assert.AreEqual(0.001, sidereal.SiderealDayLength);
        }

        [Test]
        public void Setter_AxialTilt_ValidValue_SetsCorrectly()
        {
            var sidereal = new SiderealProperties(10, 0);
            sidereal.AxialTilt = 45f;

            Assert.AreEqual(45f, sidereal.AxialTilt);
        }

        [Test]
        public void Setter_AxialTilt_Above180_CorrectsToRange()
        {
            var sidereal = new SiderealProperties(10, 0);
            sidereal.AxialTilt = 200f;

            Assert.AreEqual(160f, sidereal.AxialTilt); // Should be corrected to 180 - (200 - 180) = 160
        }

        [Test]
        public void Setter_AxialTilt_NegativeValue_CorrectsToRange()
        {
            var sidereal = new SiderealProperties(10, 0);
            sidereal.AxialTilt = -45f;

            Assert.AreEqual(45f, sidereal.AxialTilt); // Should be corrected to 45
        }

        [Test]
        public void Setter_AxialTilt_ZeroValue_SetsCorrectly()
        {
            var sidereal = new SiderealProperties(10, 0);
            sidereal.AxialTilt = 0f;

            Assert.AreEqual(0f, sidereal.AxialTilt);
        }

        [Test]
        public void Setter_AxialTilt_Exact180Value_SetsCorrectly()
        {
            var sidereal = new SiderealProperties(10, 0);
            sidereal.AxialTilt = 180f;

            Assert.AreEqual(180f, sidereal.AxialTilt);
        }

        [Test]
        public void Setter_AxialTilt_LargeNegativeValue_CorrectsToRange()
        {
            var sidereal = new SiderealProperties(10, 0);
            sidereal.AxialTilt = -500f;

            Assert.AreEqual(140f, sidereal.AxialTilt); // Corrects to  -500 + 360 = 140
        }

        [Test]
        public void GetInfo_ReturnsFormattedString()
        {
            LocalisationProvider.LoadLoc("en-ie");

            var sidereal = new SiderealProperties(10, 15);

            string info = sidereal.GetInfo();
            Assert.IsTrue(info.Contains("Sidereal Day Length: 10"));
            Assert.IsTrue(info.Contains("Axial Tilt: 15"));
        }
    }
}