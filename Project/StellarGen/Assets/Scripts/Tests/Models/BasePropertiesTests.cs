using NUnit.Framework;
using Models;
using System;

namespace Models.Tests
{
    public class BasePropertiesTests
    {
        [Test]
        public void Constructor_DefaultValues_ShouldInitializeWithDefaults()
        {
            var props = new BaseProperties();

            Assert.AreEqual(0, props.SeedValue);
            Assert.AreEqual("Unnamed Body", props.Name);
            Assert.IsFalse(props.CustomName);
            Assert.AreEqual(0m, props.Age);
            Assert.AreEqual(0m, props.Mass);
            Assert.AreEqual(0m, props.HillSphere);
            Assert.AreEqual(3, props.OrbitLine.Length);
        }

        [Test]
        public void Constructor_WithParameters_ShouldSetValues()
        {
            var orbitLine = new[] { 1, 2, 3 };
            var props = new BaseProperties(42, "TestName", 4.5m, 1000m, 5m, orbitLine);

            Assert.AreEqual(42, props.SeedValue);
            Assert.AreEqual("TestName", props.Name);
            Assert.IsFalse(props.CustomName);
            Assert.AreEqual(4.5m, props.Age);
            Assert.AreEqual(1000m, props.Mass);
            Assert.AreEqual(5m, props.HillSphere);
            Assert.AreEqual(orbitLine, props.OrbitLine);
        }

        [Test]
        public void Property_GettersAndSetters_WorkAsExpected()
        {
            var props = new BaseProperties();

            props.Name = "NewName";
            props.CustomName = true;
            props.Parent = 99;
            props.Mass = 3000m;
            props.HillSphere = 2.5m;
            props.Age = 6.7m;
            props.OrbitLine = new[] { 10, 20, 30 };
            props.Rotation = 14.2;
            props.AxialTilt = 88f;

            Assert.AreEqual("NewName", props.Name);
            Assert.IsTrue(props.CustomName);
            Assert.AreEqual(99, props.Parent);
            Assert.AreEqual(3000m, props.Mass);
            Assert.AreEqual(2.5m, props.HillSphere);
            Assert.AreEqual(6.7m, props.Age);
            CollectionAssert.AreEqual(new[] { 10, 20, 30 }, props.OrbitLine);
            Assert.AreEqual(14.2, props.Rotation);
            Assert.AreEqual(88f, props.AxialTilt);
        }

        [Test]
        public void OrbitLine_Null_InitializesToDefaultWhite()
        {
            var props = new BaseProperties();
            props.OrbitLine = null;

            var result = props.OrbitLine;

            Assert.NotNull(result);
            Assert.AreEqual(3, result.Length);
            Assert.IsTrue(result[0] >= 0 && result[0] <= 255);
        }

        [Test]
        public void GetInfo_WithoutParentAndOrbit_ReturnsExpectedInfo()
        {
            var props = new BaseProperties(12, "Planet X", 1.2m, 2000m, 0.5m);

            var info = props.GetInfo();

            StringAssert.Contains("Planet X", info);
            StringAssert.Contains("1.2", info);
            StringAssert.Contains("0.5", info);
            StringAssert.DoesNotContain("Parent ID", info);
        }

        [Test]
        public void GetInfo_WithParentAndOrbit_ReturnsFullInfo()
        {
            var props = new BaseProperties(1, "TestPlanet", 1.5m, 800m, 1.0m);
            props.Parent = 99;
            props.Orbit = new OrbitalProperties(1, 0.01f, 0, 0, 180); // Assuming OrbitalProperties has GetInfo()

            var info = props.GetInfo();

            StringAssert.Contains("TestPlanet", info);
            StringAssert.Contains("Parent ID: 99", info);
            StringAssert.Contains("1.5", info);
            StringAssert.Contains("1.0", info);
            StringAssert.Contains("Orbit", info); // Assuming Orbit.GetInfo returns a string containing
        }
    }
}
