using NUnit.Framework;
using System;
using System.Collections.Generic;
using SystemGen;
using Models;

namespace SystemGen.Tests
{
    public class BodyGenTests
    {
        // A minimal stub for BaseProperties
        private class DummyBody : BaseProperties
        {
            public DummyBody(int seed = 1) : base(seed) { }
        }

        [Test]
        public void GenerateChildren_ReturnsEmptyList()
        {
            // Arrange
            var body = new DummyBody(seed: 42);

            // Act
            var children = BodyGen.GenerateChildren(body);

            // Assert
            Assert.NotNull(children, "GenerateChildren should never return null.");
            Assert.IsEmpty(children, "GenerateChildren should return an empty list by default.");
        }

        [Test]
        public void GenerateMinorChildren_ReturnsEmptyList()
        {
            // Arrange
            var body = new DummyBody(seed: 99);

            // Act
            var minor = BodyGen.GenerateMinorChildren(body);

            // Assert
            Assert.NotNull(minor, "GenerateMinorChildren should never return null.");
            Assert.IsEmpty(minor, "GenerateMinorChildren should return an empty list by default.");
        }

        [Test]
        public void Methods_DoNotModifyInputBody()
        {
            // Arrange
            var body = new DummyBody(seed: 7);
            var originalName = body.Name;
            body.Name = "TestBody";

            // Act
            var children = BodyGen.GenerateChildren(body);
            var minor = BodyGen.GenerateMinorChildren(body);

            // Assert that body properties remain unchanged
            Assert.AreEqual("TestBody", body.Name, "GenerateChildren should not modify the input body.");
            Assert.AreEqual(7, body.SeedValue, "SeedValue should remain unchanged.");
        }
    }
}