using NUnit.Framework;
using Models;

namespace Models.Tests
{
    public class BeltPropertiesTests
    {
        [Test]
        public void Constructor_DefaultsAssignedCorrectly()
        {
            var belt = new BeltProperties(seedValue: 123);

            Assert.AreEqual("Unnamed Belt", belt.Name);
            Assert.AreEqual(0m, belt.LowerEdge);
            Assert.AreEqual(0m, belt.UpperEdge);
            Assert.IsNull(belt.Sidereal);
            Assert.AreEqual(123, belt.SeedValue);
        }

        [Test]
        public void MeanComposition_AssignmentIsCorrect()
        {
            var inner = new SurfaceProperties(60f, 30f, 10f);
            var center = new SurfaceProperties(50f, 30f, 20f);
            var outer = new SurfaceProperties(40f, 40f, 20f);

            var belt = new BeltProperties();
            belt.SetCompositon(inner, center, outer);

            var comp = belt.MeanComposition;
            Assert.AreEqual(60f, comp.Inner.Rock);
            Assert.AreEqual(30f, comp.Centre.Ice);
            Assert.AreEqual(20f, comp.Outer.Metals);
        }

        [Test]
        public void GetInfo_ContainsExpectedTextSections()
        {
            var inner = new SurfaceProperties(60f, 20f, 20f);
            var center = new SurfaceProperties(50f, 25f, 25f);
            var outer = new SurfaceProperties(30f, 40f, 30f);

            var belt = new BeltProperties(name: "Asteroid Ring", lowerEdge: 5e10m, upperEdge: 8e10m);
            belt.SetCompositon(inner, center, outer);

            string info = belt.GetInfo();

            Assert.IsTrue(info.Contains("Asteroid Ring"));
            Assert.IsTrue(info.Contains("Mean Composition:"));
            Assert.IsTrue(info.Contains("Surface Composition: Rock:")); // Each section will be printed
            Assert.IsTrue(info.Contains("Inner Range:"));
            Assert.IsTrue(info.Contains("Outer Range:"));
            Assert.IsTrue(info.Contains("AU")); // Assuming PhysicsUtils.ConvertToAU outputs with "AU"
        }

        [Test]
        public void PropertySetters_AssignValuesCorrectly()
        {
            var belt = new BeltProperties();
            belt.LowerEdge = 1e9m;
            belt.UpperEdge = 2e9m;

            Assert.AreEqual(1e9m, belt.LowerEdge);
            Assert.AreEqual(2e9m, belt.UpperEdge);
        }
    }
}
