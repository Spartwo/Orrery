using NUnit.Framework;
using Models;
using System.Linq;
using System;
using System.Diagnostics;
using UnityEngine;
using Settings;

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
            atmosphere.SetElementPercentage(AtmosphereProperties.O2, (short)25f);

            float result = atmosphere.GetElementPercentage(AtmosphereProperties.O2);
            Assert.AreEqual(25f, result);
        }

        [Test]
        public void SetElementPercentage_UpdatesExistingElement()
        {
            atmosphere.SetElementPercentage(AtmosphereProperties.O2, (short)25f);
            atmosphere.SetElementPercentage(AtmosphereProperties.O2, (short)40f);

            float result = atmosphere.GetElementPercentage(AtmosphereProperties.O2);
            Assert.AreEqual(40f, result);
        }

        [Test]
        public void SetElementPercentage_AdjustsOtherElements_WhenOver100Percent()
        {
            atmosphere.SetElementPercentage(AtmosphereProperties.O2, (short)70f);
            atmosphere.SetElementPercentage(AtmosphereProperties.N, (short)30f);
            atmosphere.SetElementPercentage(AtmosphereProperties.CO2, (short)20f); // Should reduce others

            float total = atmosphere.Elements.Sum(p => p.Percentile);
            Assert.LessOrEqual(total, 100f);
        }

        [Test]
        public void GetAtmosphereGasConstant_ComputesCorrectWeightedAverage()
        {
            atmosphere.SetElementPercentage(AtmosphereProperties.H2, (short)50f);
            atmosphere.SetElementPercentage(AtmosphereProperties.He, (short)50f);

            double expected = 2881;
            double actual = atmosphere.GetAtmosphereGasConstant();

            Assert.AreEqual(expected, actual, 0.1);
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
            LocalisationProvider.LoadLoc("en-ie");

            atmosphere.SetElementPercentage(AtmosphereProperties.H2, (short)80f);
            atmosphere.SetElementPercentage(AtmosphereProperties.He, (short)20f);

            string info = atmosphere.GetInfo();

            UnityEngine.Debug.Log(atmosphere.GetInfo());

            Assert.IsTrue(info.Contains("Hydrogen: 78"));
            Assert.IsTrue(info.Contains("Helium: 20"));
        }

        [Test]
        public void ExceedsJeanEscape_ReturnsTrue_WhenEscapeVelocityExceedsThermalVelocity()
        {
            // Test case where escape velocity is expected to exceed thermal velocity.

            // Given values for temperature, surface mass, and radius
            short temperature = 5000;  // High temperature (in Kelvin)
            decimal surfaceMass = 5.972E18m;  // Mass of Earth
            float radius = 1f;  // 1 Earth radius

            // Act
            bool result = AtmosphereProperties.H2.ExceedsJeanEscape(temperature, surfaceMass, radius);

            // Assert
            Assert.IsTrue(result, "Escape velocity should exceed thermal velocity for this case.");
        }

        [Test]
        public void ExceedsJeanEscape_ReturnsFalse_WhenThermalVelocityExceedsEscapeVelocity()
        {
            // Test case where thermal velocity exceeds escape velocity.

            // Given values for temperature, surface mass, and radius
            short temperature = 40;  // Low temperature (in Kelvin)
            decimal surfaceMass = 5.972E18m;  // Mass of Earth
            float radius = 1f;  // 1 Earth radius

            // Act
            bool result = AtmosphereProperties.H2.ExceedsJeanEscape(temperature, surfaceMass, radius);

            // Assert
            Assert.IsFalse(result, "Escape velocity should not exceed thermal velocity for this case.");
        }

        [Test]
        public void ExceedsJeanEscape_ReturnsTrue_ForSmallPlanetWithHighTemperature()
        {
            // Test case for small planet with very high temperature.
            // Escape velocity should still exceed thermal velocity despite small mass.

            // Given values for temperature, surface mass, and radius
            short temperature = 1000;  // Very high temperature (in Kelvin)
            decimal surfaceMass = 1.0E15m;  // Very small mass (in kg)
            float radius = 0.1f;  // 0.1 Earth radius (smaller planet)

            // Act
            bool result = AtmosphereProperties.H2.ExceedsJeanEscape(temperature, surfaceMass, radius);

            // Assert
            Assert.IsTrue(result, "Escape velocity should exceed thermal velocity even for small planets at high temperatures.");
        }

        [Test]
        public void ExceedsJeanEscape_True_ForEarthlike()
        {
            // Test case for an earthlike body

            // Given values for temperature, surface mass, and radius
            short temperature = 288;  
            decimal surfaceMass = 5.972e18m;  
            float radius = 1f;  

            // Act
            bool result = AtmosphereProperties.H2.ExceedsJeanEscape(temperature, surfaceMass, radius);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ExceedsJeanEscape_ReturnsFalse_ForColdSmallPlanet()
        {
            // Test case for small planet with low temperature.
            // Thermal velocity should exceed escape velocity.

            // Given values for temperature, surface mass, and radius
            short temperature = 40;  // Very low temperature (in Kelvin)
            decimal surfaceMass = 1.0E17m;  // Very small mass
            float radius = 0.1f;  // 0.1 Earth radius (smaller planet)

            // Act
            bool result = AtmosphereProperties.N.ExceedsJeanEscape(temperature, surfaceMass, radius);

            // Assert
            Assert.IsFalse(result, "Escape velocity should not exceed thermal velocity for cold, small planets.");
        }

        [Test]
        public void ExceedsJeanEscape_ReturnsTrue_ForLargeGasGiant()
        {
            // Test case for large gas giant with high temperature.
            // Escape velocity should exceed thermal velocity due to high mass and size.

            // Given values for temperature, surface mass, and radius
            short temperature = 3000;  // Moderate temperature (in Kelvin)
            decimal surfaceMass = 1.898E21m;  // Mass of Jupiter
            float radius = 11f;  // Jupiter's radius (11 Earth radii)

            // Act
            bool result = AtmosphereProperties.H2.ExceedsJeanEscape(temperature, surfaceMass, radius);

            // Assert
            Assert.IsTrue(result, "Escape velocity should exceed thermal velocity for large gas giants.");
        }

        [Test]
        public void ExceedsJeanEscape_ReturnsFalse_ForLowTemperatureAndGasGiant()
        {
            // Test case for gas giant with low temperature.
            // The escape velocity should not exceed thermal velocity due to low temperature.

            // Given values for temperature, surface mass, and radius
            short temperature = 100;  // Low temperature (in Kelvin)
            decimal surfaceMass = 1.898E21m;  // Mass of Jupiter in
            float radius = 11f;  // Jupiter's radius (11 Earth radii)

            // Act
            bool result = AtmosphereProperties.H2.ExceedsJeanEscape(temperature, surfaceMass, radius);

            // Assert
            Assert.IsFalse(result, "Escape velocity should not exceed thermal velocity for gas giants at low temperatures.");
        }

        [Test]
        public void ExceedsJeanEscape_ReturnsTrue_ForVeryHighMass()
        {
            // Test case for very high mass planet with a moderate temperature.
            // Escape velocity should exceed thermal velocity because of high mass.

            // Given values for temperature, surface mass, and radius
            short temperature = 300;  // Moderate temperature (in Kelvin)
            decimal surfaceMass = 2.99E19m;  // Extremely high mass
            float radius = 5f;  // 5 Earth radii

            // Act
            bool result = AtmosphereProperties.H2.ExceedsJeanEscape(temperature, surfaceMass, radius);

            // Assert
            Assert.IsTrue(result, "Escape velocity should exceed thermal velocity for very high mass planets.");
        }
    }
}
