using NUnit.Framework;
using Models;
using StellarGenHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Settings;


namespace Models.Tests
{
    
    public class SurfacePropertiesTests
    {
        [Test]
        public void Constructor_DefaultValues_Test()
        {
            // Act
            var surfaceProperties = new SurfaceProperties();

            // Assert
            Assert.AreEqual(40f, surfaceProperties.Rock, "Default rock percentage should be 40%");
            Assert.AreEqual(40f, surfaceProperties.Ice, "Default ice percentage should be 40%");
            Assert.AreEqual(20f, surfaceProperties.Metals, "Default metals percentage should be 20%");
            Assert.AreEqual(0m, surfaceProperties.TotalSolidMass, "Default total solid mass should be 0");
        }

        [Test]
        public void SetComposition_Normalization_Test()
        {
            // Arrange
            var surfaceProperties = new SurfaceProperties();

            // Act
            surfaceProperties.SetComposition(50f, 30f, 20f);

            // Assert
            Assert.AreEqual(50f, surfaceProperties.Rock, "Rock percentage should be normalized to 50%");
            Assert.AreEqual(30f, surfaceProperties.Ice, "Ice percentage should be normalized to 30%");
            Assert.AreEqual(20f, surfaceProperties.Metals, "Metals percentage should be normalized to 20%");
        }

        [Test]
        public void CalculateDensity_Test()
        {
            // Arrange
            var surfaceProperties = new SurfaceProperties(50f, 30f, 20f);

            // Act
            var density = surfaceProperties.CalculateDensity();

            // Assert
            Assert.AreEqual(
                (50f * PhysicalConstants.ROCK_DENSITY +
                 30f * PhysicalConstants.ICE_DENSITY +
                 20f * PhysicalConstants.METAL_DENSITY) / 100f,
                density,
                "Density calculation should match the expected value."
            );
        }

        [Test]
        public void GetInfo_Test()
        {
            LocalisationProvider.LoadLoc("en-ie");

            // Arrange
            var surfaceProperties = new SurfaceProperties(50f, 30f, 20f);

            // Act
            var info = surfaceProperties.GetInfo();

            // Assert
            StringAssert.Contains("Rock: 50%", info, "Info should contain the correct rock percentage.");
            StringAssert.Contains("Ice: 30%", info, "Info should contain the correct ice percentage.");
            StringAssert.Contains("Metals: 20%", info, "Info should contain the correct metals percentage.");
        }

        [Test]
        public void Property_Setters_Normalization_Test()
        {
            // Arrange
            var surfaceProperties = new SurfaceProperties();

            surfaceProperties.SetComposition(40f, 30f, 30f);
            // Act
            surfaceProperties.Rock = 60f;

            // Assert
            Assert.AreEqual(60f, surfaceProperties.Rock, "Rock percentage should be normalized to 60%");
            Assert.AreEqual(20f, surfaceProperties.Ice, "Ice percentage should be normalized to 20%");
            Assert.AreEqual(20f, surfaceProperties.Metals, "Metals percentage should be normalized to 20%");
        }

        [Test]
        public void Property_Composition_Normalization_Test()
        {
            // Arrange
            var surfaceProperties = new SurfaceProperties();

            surfaceProperties.SetComposition(60f, 30f, 30f);

            // Assert
            Assert.AreEqual(50f, surfaceProperties.Rock, "Rock percentage should be normalized to 50%");
            Assert.AreEqual(25f, surfaceProperties.Ice, "Ice percentage should be normalized to 25%");
            Assert.AreEqual(25f, surfaceProperties.Metals, "Metals percentage should be normalized to 25%");
        }
    }
}