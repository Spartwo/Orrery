using NUnit.Framework;
using StellarGenHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StellarGenHelpers.Tests
{
    
    public class RandomUtilsTests
    {
        [Test]
        public void TweakSeed_ReturnsLessThanOrEqualSeed()
        {
            int seed = 1000;
            int tweaked = RandomUtils.TweakSeed(seed);
            Assert.LessOrEqual(tweaked, seed);
            Assert.GreaterOrEqual(tweaked, 0);
        }

        [Test]
        public void RandomFloat_WithSameSeed_IsDeterministic()
        {
            float a = RandomUtils.RandomFloat(1f, 5f, seed: 42);
            float b = RandomUtils.RandomFloat(1f, 5f, seed: 42);
            Assert.AreEqual(a, b);
        }

        [Test]
        public void RandomFloat_InRange_WithoutSeed()
        {
            for (int i = 0; i < 10; i++)
            {
                float val = RandomUtils.RandomFloat(-10f, 10f);
                Assert.GreaterOrEqual(val, -10f);
                Assert.LessOrEqual(val, 10f);
            }
        }

        [Test]
        public void RandomInt_WithSameSeed_IsDeterministic_AndInRange()
        {
            int a = RandomUtils.RandomInt(0, 5, seed: 99);
            int b = RandomUtils.RandomInt(0, 5, seed: 99);
            Assert.AreEqual(a, b);
            Assert.GreaterOrEqual(a, 0);
            Assert.LessOrEqual(a, 5);
        }

        [Test]
        public void RandomColor_WithSameSeed_IsDeterministic()
        {
            Color c1 = RandomUtils.RandomColor(7);
            Color c2 = RandomUtils.RandomColor(7);
            Assert.AreEqual(c1, c2);
        }

        [Test]
        public void RandomColor_WithoutSeed_ReturnsValidColor()
        {
            Color c = RandomUtils.RandomColor();
            Assert.GreaterOrEqual(c.r, 0f);
            Assert.LessOrEqual(c.r, 1f);
            Assert.GreaterOrEqual(c.g, 0f);
            Assert.LessOrEqual(c.g, 1f);
            Assert.GreaterOrEqual(c.b, 0f);
            Assert.LessOrEqual(c.b, 1f);
        }

        [Test]
        public void GenerateSystemName_ReturnsNameAndNumberPattern()
        {
            var name = RandomUtils.GenerateSystemName();
            // Expect something like "Alpha-123 any name (no hyphens) { dash } 1–3 digits"
            StringAssert.IsMatch(@"^[^-]+-\d{1,3}$", name);
        }
    }
}