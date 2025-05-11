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
    
    public class BodyPropertiesTests
    {
        [Test]
        public void Constructor_InitializesDefaultsCorrectly()
        {
            var body = new BodyProperties(seedValue: 123, name: "TestPlanet");

            Assert.AreEqual("TestPlanet", body.Name);
            Assert.AreEqual(0f, body.Radius);
            Assert.IsNotNull(body.Atmosphere);
            Assert.IsNotNull(body.Composition);
        }

        [Test]
        public void Radius_SetAndGet_WorksCorrectly()
        {
            var body = new BodyProperties();
            body.Radius = 1.2f;

            Assert.AreEqual(1.2f, body.Radius);
        }

        [Test]
        public void Composition_SetAndGet_WorksCorrectly()
        {
            var body = new BodyProperties();
            var newComposition = new SurfaceProperties();
            body.Composition = newComposition;

            Assert.AreSame(newComposition, body.Composition);
        }

        [Test]
        public void Atmosphere_SetAndGet_WorksCorrectly()
        {
            var body = new BodyProperties();
            var newAtmosphere = new AtmosphereProperties(0);
            body.Atmosphere = newAtmosphere;

            Assert.AreSame(newAtmosphere, body.Atmosphere);
        }

        [Test]
        public void GetInfo_ReturnsFormattedString()
        {
            LocalisationProvider.LoadLoc("en-ie");

            var body = new BodyProperties(seedValue: 0, name: "InfoPlanet", mass: 5.972e24m);
            body.Radius = 1.0f;

            string info = body.GetInfo();

            Assert.IsTrue(info.Contains("1"));
            Assert.IsTrue(info.Contains("1"));
        }
    }
}