using NUnit.Framework;
using Models;
using System.Linq;
using System;

namespace Models.Tests
{
    public class AtmospherePropertiesTests
    {
        private AtmosphereProperties atmosphere;

        [SetUp]
        public void Setup()
        {
            atmosphere = new AtmosphereProperties(1000m);
        }

        [Test]
        public void SetElementPercentage_AddsNewElement_WhenNotPresent()
        {
            atmosphere.SetElementPercentage(Models.AtmosphereProperties.O2, 25f);

            float result = atmosphere.GetElementPercentage(Models.AtmosphereProperties.O2);
            Assert.AreEqual(25f, result);
        }

        [Test]
        public void SetElementPercentage_UpdatesExistingElement()
        {
            atmosphere.SetElementPercentage(Models.AtmosphereProperties.O2, 25f);
            atmosphere.SetElementPercentage(Models.AtmosphereProperties.O2, 40f);

            float result = atmosphere.GetElementPercentage(Models.AtmosphereProperties.O2);
            Assert.AreEqual(40f, result);
        }

        [Test]
        public void SetElementPercentage_AdjustsOtherElements_WhenOver100Percent()
        {
            atmosphere.SetElementPercentage(Models.AtmosphereProperties.O2, 70f);
            atmosphere.SetElementPercentage(Models.AtmosphereProperties.N, 30f);
            atmosphere.SetElementPercentage(Models.AtmosphereProperties.CO2, 20f); // Should reduce others

            float total = atmosphere.elementPercentages.Sum(p => p.Item2);
            Assert.LessOrEqual(total, 100f);
        }

        [Test]
        public void GetAtmosphereGasConstant_ComputesCorrectWeightedAverage()
        {
            atmosphere.SetElementPercentage(Models.AtmosphereProperties.H2, 50f);
            atmosphere.SetElementPercentage(Models.AtmosphereProperties.He, 50f);

            double expected = (Models.AtmosphereProperties.H2.GasConstant * 0.5) +
                              (Models.AtmosphereProperties.He.GasConstant * 0.5);
            double actual = atmosphere.GetAtmosphereGasConstant();

            Assert.AreEqual(expected, actual, 0.01);
        }

        [TestCase(273.15f, 1f, ExpectedResult = Element.Phase.Liquid)]
        [TestCase(90f, 1f, ExpectedResult = Element.Phase.Solid)]
        [TestCase(400f, 0.5f, ExpectedResult = Element.Phase.Gas)]
        public Element.Phase PhaseDetermination_WaterVapour(float tempK, float pressureAtm)
        {
            return AtmosphereProperties.H2O.GetPhase(tempK, pressureAtm);
        }

        [TestCase(50f, 1f, ExpectedResult = Element.Phase.Solid)]
        [TestCase(100f, 2f, ExpectedResult = Element.Phase.Liquid)]
        [TestCase(500f, 0.1f, ExpectedResult = Element.Phase.Gas)]
        public Element.Phase PhaseDetermination_Oxygen(float tempK, float pressureAtm)
        {
            return AtmosphereProperties.O2.GetPhase(tempK, pressureAtm);
        }

        [Test]
        public void GetInfo_ReturnsCorrectString()
        {
            atmosphere.SetElementPercentage(AtmosphereProperties.H2, 80f);
            atmosphere.SetElementPercentage(AtmosphereProperties.He, 20f);

            string info = atmosphere.GetInfo();

            Assert.IsTrue(info.Contains("#loc_Hydrogen: 80"));
            Assert.IsTrue(info.Contains("#loc_Helium: 20"));
        }

        [Test]
        public void SetElementPercentage_ShouldThrowOnNegativePercentage()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                atmosphere.SetElementPercentage(AtmosphereProperties.CO2, -5f));
            Assert.That(ex.Message, Does.Contain("must be between 0 and 100"));
        }

        [Test]
        public void SetElementPercentage_ShouldThrowOnPercentageOver100()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                atmosphere.SetElementPercentage(AtmosphereProperties.H2O, 150f));
            Assert.That(ex.Message, Does.Contain("must be between 0 and 100"));
        }

        [Test]
        public void GetElementPercentage_ShouldReturnZeroIfElementNotPresent()
        {
            float percent = atmosphere.GetElementPercentage(AtmosphereProperties.He);
            Assert.AreEqual(0f, percent);
        }
    }
}
