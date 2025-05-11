using NUnit.Framework;
using System;
using System.Collections.Generic;
using Models;
using StellarGenHelpers;
using SystemGen;

namespace SystemGen.Tests
{
    public class BeltGenTests
    {
        [Test]
        public void Generate_WithZeroSeed_SetsNonZeroSeedAndProperties()
        {
            // Arrange
            var star = StarGen.Generate(25000);
            var orbit = new OrbitalProperties(1000m, 0f, 0f, 0f, 0f);
            decimal totalMass = 1000m;
            float lowerEdge = 1f;
            float upperEdge = 2f;

            // Act
            var belt = BeltGen.Generate(0, star, orbit, totalMass, lowerEdge, upperEdge);

            // Assert
            Assert.NotNull(belt);
            Assert.AreNotEqual(0, belt.SeedValue);
            Assert.AreEqual(orbit, belt.Orbit);
            Assert.AreEqual(1000m, belt.Mass);
            Assert.AreEqual(PhysicsUtils.ConvertToMetres(lowerEdge), belt.LowerEdge);
            Assert.AreEqual(PhysicsUtils.ConvertToMetres(upperEdge), belt.UpperEdge);
            Assert.NotNull(belt.MeanComposition);
        }

        [Test]
        public void Generate_WithFixedSeed_IsDeterministic()
        {
            // Arrange
            var star = StarGen.Generate(5);
            var orbit = new OrbitalProperties(2000m, 0f, 0f, 0f, 0f);
            decimal totalMass = 500m;

            // Act
            var b1 = BeltGen.Generate(42, star, orbit, totalMass, 1f, 3f);
            var b2 = BeltGen.Generate(42, star, orbit, totalMass, 1f, 3f);

            // Assert
            Assert.AreEqual(b1.SeedValue, b2.SeedValue);
            Assert.AreEqual(b1.LowerEdge, b2.LowerEdge);
            Assert.AreEqual(b1.UpperEdge, b2.UpperEdge);
            Assert.AreEqual(b1.MeanComposition.Inner.Rock, b2.MeanComposition.Inner.Rock);
            Assert.AreEqual(b1.MeanComposition.Centre.Ice, b2.MeanComposition.Centre.Ice);
            Assert.AreEqual(b1.MeanComposition.Outer.Metals, b2.MeanComposition.Outer.Metals);
        }

        [Test]
        public void GenerateMinorChildren_ReturnsEmptyList()
        {
            // Arrange
            var dummyPlanet = new BodyProperties(1);

            // Act
            var moons = BeltGen.GenerateMinorChildren(dummyPlanet);

            // Assert
            Assert.NotNull(moons);
            Assert.IsEmpty(moons);
        }

        [Test]
        public void MeanComposition_SumsToOneHundredPercent()
        {
            // Arrange
            var star = StarGen.Generate(7);
            var orbit = new OrbitalProperties(1500m, 0f, 0f, 0f, 0f);
            var belt = BeltGen.Generate(7, star, orbit, 2000m, 2f, 5f);

            // Act
            var comp = belt.MeanComposition;
            float sumInner = comp.Inner.Rock + comp.Inner.Ice + comp.Inner.Metals;
            float sumCentre = comp.Centre.Rock + comp.Centre.Ice + comp.Centre.Metals;
            float sumOuter = comp.Outer.Rock + comp.Outer.Ice + comp.Outer.Metals;

            // Assert sums within tolerance
            Assert.That(sumInner, Is.InRange(99.9f, 100.1f));
            Assert.That(sumCentre, Is.InRange(99.9f, 100.1f));
            Assert.That(sumOuter, Is.InRange(99.9f, 100.1f));
        }

        [Test]
        public void MassReduction_AfterComposition_LessThanOrEqualTotalMass()
        {
            // Arrange
            var star = StarGen.Generate(9);
            var orbit = new OrbitalProperties(1200m, 0f, 0f, 0f, 0f);
            decimal totalMass = 3000m;

            // Act
            var belt = BeltGen.Generate(9, star, orbit, totalMass, 3f, 6f);

            // Assert
            Assert.LessOrEqual(belt.Mass, totalMass);
            Assert.Greater(belt.Mass, 0m);
        }
    }
}