using Models;
using NUnit.Framework;
using StellarGenHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarGenHelpers.Tests
{
    
    public class PhysicsUtilsTests
    {

        [Test]
        public void DecimalPow_FractionalExponent_TruncatesToIntegerPart()
        {
            // exponent 2.7 → loops twice → base^2
            Assert.AreEqual(9m, PhysicsUtils.DecimalPow(3m, 2.7m));
        }

        [Test]
        public void ConvertToMetres_And_ConvertToAU_RoundTrip()
        {
            float au = 1.23f;
            decimal metres = PhysicsUtils.ConvertToMetres(au);
            float back = PhysicsUtils.ConvertToAU(metres);
            Assert.AreEqual(au, back, 1e-3f);
        }

        [Test]
        public void EarthMassToRaw_And_RawToEarthMass_RoundTrip()
        {
            float em = 3.5f;
            decimal raw = PhysicsUtils.EarthMassToRaw(em);
            float back = PhysicsUtils.RawToEarthMass(raw);
            Assert.AreEqual(em, back, 1e-3f);
        }

        [Test]
        public void SolMassToRaw_And_RawToSolMass_RoundTrip()
        {
            float sm = 0.5f;
            decimal raw = PhysicsUtils.SolMassToRaw(sm);
            float back = PhysicsUtils.RawToSolMass(raw);
            Assert.AreEqual(sm, back, 1e-3f);
        }

        [Test]
        public void CheckOrbit_StableWhenDistanceLessThanHillSphere()
        {
            // Create two bodies with simple circular orbits
            var parent = new BodyProperties(seedValue: 1, mass: 100m);
            var child = new BodyProperties(seedValue: 2, mass: 1m);
            child.Orbit = new OrbitalProperties(1_000_000m, 0f, 0f, 0f, 0f);
            parent.Orbit = new OrbitalProperties(1_000_000m, 0f, 0f, 0f, 0f);

            var dist = 0.1m;
            Assert.IsTrue(PhysicsUtils.CheckOrbit(child, parent, dist));
        }

        [Test]
        public void CalculateHillSphere_CalculatesPositiveRadius()
        {
            var a = new BodyProperties(seedValue: 1, mass: 10m);
            var b = new BodyProperties(seedValue: 1, mass: 100m);
            var hill = PhysicsUtils.CalculateHillSphere(a, b, 1m);
            Assert.Greater(hill, 0m);
        }

        [Test]
        public void ConstructOrbitProperties_SameSeed_IsDeterministic()
        {
            var o1 = PhysicsUtils.ConstructOrbitProperties(42, 1f, 0.1f, 15f);
            var o2 = PhysicsUtils.ConstructOrbitProperties(42, 1f, 0.1f, 15f);

            Assert.AreEqual(o1.SemiMajorAxis, o2.SemiMajorAxis);
            Assert.AreEqual(o1.Eccentricity, o2.Eccentricity);
            Assert.AreEqual(o1.Inclination, o2.Inclination);
            Assert.AreEqual(o1.LongitudeOfAscending, o2.LongitudeOfAscending);
            Assert.AreEqual(o1.PeriArgument, o2.PeriArgument);
        }

        [Test]
        public void ConstructOrbitProperties_ValuesWithinExpectedRanges()
        {
            var orbit = PhysicsUtils.ConstructOrbitProperties(1, 2f, 0.3f, 20f);

            Assert.GreaterOrEqual(orbit.SemiMajorAxis, 1m); // meters ≥ 1
            Assert.GreaterOrEqual(orbit.Eccentricity, 0.001f);
            Assert.LessOrEqual(orbit.Eccentricity, 0.6f);    // 0.3*2 = 0.6 max
            Assert.GreaterOrEqual(orbit.Inclination, 0f);
            Assert.LessOrEqual(orbit.Inclination, 20f);
            Assert.GreaterOrEqual(orbit.LongitudeOfAscending, 0f);
            Assert.LessOrEqual(orbit.LongitudeOfAscending, 359f);
            Assert.GreaterOrEqual(orbit.PeriArgument, 0f);
            Assert.LessOrEqual(orbit.PeriArgument, 359f);
        }
    }
}