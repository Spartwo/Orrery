using NUnit.Framework;
using SystemGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;
using StellarGenHelpers;

namespace SystemGen.Tests
{
    public class PlanetGenTests
    {
        private StarProperties GetMockStar()
        {
            return new StarProperties
            {
                Mass = 1.0m * (decimal)PhysicalConstants.SOLAR_MASS, // 1 solar mass
                Age = 4.6m, // billion years
            };
        }

        private OrbitalProperties GetMockOrbit()
        {
            return new OrbitalProperties
            (
                1.0m * (decimal)PhysicalConstants.AU, // Semi-major axis in AU
                0.0167f, // Eccentricity
                0f, // Longitude of ascending node
                0f, // Inclination
                0f // Argument of periapsis
            );
        }

        [Test]
        public void GeneratePlanet_ReturnsValidBody()
        {
            var star = GetMockStar();
            var orbit = GetMockOrbit();
            var body = PlanetGen.Generate(42, star, orbit, 1.0m, 1.5f, 0.01m * (decimal)PhysicalConstants.SOLAR_MASS);

            Assert.IsNotNull(body);
            Assert.Greater(body.Mass, 0);
            Assert.Greater(body.Radius, 0);
        }

        [Test]
        public void GeneratePlanet_IsDeterministicForSameSeed()
        {
            var star = GetMockStar();
            var orbit = GetMockOrbit();

            var body1 = PlanetGen.Generate(12345, star, orbit, 1.5m, 2.0f, 0.02m *  (decimal)PhysicalConstants.SOLAR_MASS);
            var body2 = PlanetGen.Generate(12345, star, orbit, 1.5m, 2.0f, 0.02m * (decimal)PhysicalConstants.SOLAR_MASS);

            Assert.AreEqual(body1.Mass, body2.Mass);
            Assert.AreEqual(body1.Radius, body2.Radius);
            Assert.AreEqual(body1.Sidereal.SiderealDayLength, body2.Sidereal.SiderealDayLength);
        }

        [Test]
        public void GeneratePlanet_SiderealDayLength_IsPositive()
        {
            var star = GetMockStar();
            var orbit = GetMockOrbit();
            var body = PlanetGen.Generate(54321, star, orbit, 1.2m, 1.5f, 0.03m * (decimal)PhysicalConstants.SOLAR_MASS);

            Assert.Greater(body.Sidereal.SiderealDayLength, 0);
        }

      
    }

}