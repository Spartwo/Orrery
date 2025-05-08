using UnityEngine;
using NUnit.Framework;
using System;
using SystemGen;
using Models;

namespace Tests
{
    public class GenerationTests
    {
        private BodyProperties testPlanet;
        private StarProperties testStar;

        [SetUp]
        public void Setup()
        {
           /* // Create test instances with typical values
            testPlanet = new Planet
            {
                RotationPeriod = 86400m, // 24 hours
                AxialTilt = 23.44f,
                SemiMajorAxis = 1.496e11m, // 1 AU in meters
                Eccentricity = 0.0167f,
                Inclination = 0.00005f,
                Radius = 6.371e6m // Earth radius in meters
            };

            testStar = new Star
            {
                Radius = 6.96e8m, // Sun radius in meters
                Luminosity = 3.828e26m, // Solar luminosity in watts
                RotationPeriod = 2160000m // Approx. 25 days
            };

            generator = new SystemGenerator();*/
        }

        // =========================
        // Planet Generation Tests
        // =========================

        [Test]
        public void Test_Planet_Creation()
        {
            Assert.NotNull(testPlanet);
        }

        [Test]
        public void Test_Planet_Valid_Rotation_Period()
        {
            //Assert.Greater(testPlanet.RotationPeriod, 0);
        }

        [Test]
        public void Test_Planet_Valid_Axial_Tilt()
        {
            //Assert.IsTrue(testPlanet.AxialTilt >= 0f && testPlanet.AxialTilt <= 180f);
        }

        [Test]
        public void Test_Planet_Valid_Orbital_Elements()
        {
           // Assert.Greater(testPlanet.SemiMajorAxis, 0);
           // Assert.IsTrue(testPlanet.Eccentricity >= 0f && testPlanet.Eccentricity < 1f);
           // Assert.Greater(testPlanet.Radius, 0);
        }

        [Test]
        public void Test_Planet_Equatorial_Velocity()
        {
            //decimal velocity = testPlanet.GetEquatorialRotationalVelocity();
            //Assert.Greater(velocity, 0);
        }

        // =========================
        // Star Generation Tests
        // =========================

        [Test]
        public void Test_Star_Creation()
        {
            Assert.NotNull(testStar);
        }

        [Test]
        public void Test_Star_Valid_Radius()
        {
            //Assert.Greater(testStar.Radius, 0);
        }

        [Test]
        public void Test_Star_Valid_Luminosity()
        {
            //Assert.Greater(testStar.Luminosity, 0);
        }

        [Test]
        public void Test_Star_Valid_Rotation_Period()
        {
            //Assert.Greater(testStar.RotationPeriod, 0);
        }

        // =========================
        // System Generation Tests
        // =========================

        [Test]
        public void Test_System_Generator_Creates_Valid_Star()
        {
            /*Star generatedStar = generator.GenerateStar();
            Assert.NotNull(generatedStar);
            Assert.Greater(generatedStar.Radius, 0);
            Assert.Greater(generatedStar.Luminosity, 0);*/
        }

        [Test]
        public void Test_System_Generator_Creates_Valid_Planet()
        {
            /*Planet generatedPlanet = generator.GeneratePlanet();
            Assert.NotNull(generatedPlanet);
            Assert.Greater(generatedPlanet.SemiMajorAxis, 0);
            Assert.Greater(generatedPlanet.Radius, 0);*/
        }

        [Test]
        public void Test_System_Generator_Creates_System_With_Planets()
        {
           /* var system = generator.GenerateSystem();
            Assert.NotNull(system);
            Assert.NotNull(system.Star);
            Assert.Greater(system.Planets.Count, 0);*/
        }
    }
}