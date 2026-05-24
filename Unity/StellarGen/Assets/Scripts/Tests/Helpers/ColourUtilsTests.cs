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
    
    public class ColourUtilsTests
    {
        [Test]
        public void RGBtoColor_ConvertsCorrectly()
        {
            var c = ColourUtils.RGBtoColor(128, 64, 255);
            Assert.AreEqual(128 / 255f, c.r);
            Assert.AreEqual(64 / 255f, c.g);
            Assert.AreEqual(255 / 255f, c.b);
            Assert.AreEqual(1f, c.a);
        }

        [Test]
        public void ArrayToColor_WithoutAlpha_DefaultsToOpaque()
        {
            var c = ColourUtils.ArrayToColor(new[] { 0, 128, 255 });
            Assert.AreEqual(0f, c.r);
            Assert.AreEqual(128 / 255f, c.g);
            Assert.AreEqual(1f, c.a);
        }

        [Test]
        public void ArrayToColor_WithAlpha_ParsesAlpha()
        {
            var c = ColourUtils.ArrayToColor(new[] { 10, 20, 30 }, a: 128);
            Assert.AreEqual(10 / 255f, c.r);
            Assert.AreEqual(20 / 255f, c.g);
            Assert.AreEqual(30 / 255f, c.b);
            Assert.AreEqual(128 / 255f, c.a);
        }

        [Test]
        public void ColorToArray_RoundTrip()
        {
            var original = new Color(0.1f, 0.5f, 0.9f);
            var arr = ColourUtils.ColorToArray(original);
            Assert.AreEqual((int)(0.1f * 255), arr[0]);
            Assert.AreEqual((int)(0.5f * 255), arr[1]);
            Assert.AreEqual((int)(0.9f * 255), arr[2]);
        }

        [TestCase("#FF0000", 1f, 0f, 0f, 1f)]
        [TestCase("#00FF0080", 0f, 1f, 0f, 128 / 255f)]
        [TestCase("FF00FF", 1f, 0f, 1f, 1f)]
        public void HexToColor_ValidHex_ParsesCorrectly(string hex, float r, float g, float b, float a)
        {
            var c = ColourUtils.HexToColor(hex);
            Assert.AreEqual(r, c.r, 1e-3);
            Assert.AreEqual(g, c.g, 1e-3);
            Assert.AreEqual(b, c.b, 1e-3);
            Assert.AreEqual(a, c.a, 1e-3);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("#ABC")]
        public void HexToColor_InvalidHex_Throws(string hex)
        {
            Assert.Throws<ArgumentException>(() => ColourUtils.HexToColor(hex));
        }

        [Test]
        public void ColorToHex_RoundTrip()
        {
            var original = new Color(0.2f, 0.4f, 0.6f, 0.8f);
            var hex = ColourUtils.ColorToHex(original);
            var round = ColourUtils.HexToColor(hex);
            Assert.AreEqual(original.r, round.r, 1e-3);
            Assert.AreEqual(original.g, round.g, 1e-3);
            Assert.AreEqual(original.b, round.b, 1e-3);
            Assert.AreEqual(original.a, round.a, 1e-3);
        }
    }
}