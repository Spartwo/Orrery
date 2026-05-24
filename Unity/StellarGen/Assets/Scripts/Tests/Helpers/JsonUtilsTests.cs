using NUnit.Framework;
using StellarGenHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarGenHelpers.Tests
{
    public class JsonUtilsTests
    {
        private string _tempFile;
        private string _tempListFile;

        [SetUp]
        public void Setup()
        {
            _tempFile = Path.GetTempFileName();
            _tempListFile = Path.GetTempFileName();
            File.WriteAllLines(_tempFile, new[]
            {
                "{",
                "  \"DisplayName\": \"English\",",
                "  \"HELLO\": \"Hello\",",
                "  \"NUMBER\": 123,",
                "  \"RawValue\": true",
                "}"
            });
        }

        [TearDown]
        public void Teardown()
        {
            if (File.Exists(_tempFile)) File.Delete(_tempFile);
            if (File.Exists(_tempListFile)) File.Delete(_tempListFile);
        }

        private class Dummy { public int X; public string Y; }

        // JsonUtils Tests
        [Test]
        public void DeserializeJsonFromFile_Nonexistent_ReturnsDefault()
        {
            var result = JsonUtils.DeserializeJsonFromFile<Dummy>("no-such-file.json");
            Assert.IsNull(result);
        }

        [Test]
        public void DeserializeFromJsonFile_Nonexistent_ReturnsDefault()
        {
            var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var result = JsonUtils.DeserializeFromJsonFile<Dummy>(path);
            Assert.IsNull(result);
        }

        [Test]
        public void SerializeToJsonFile_IncludesAllMembers()
        {
            var obj = new Dummy { X = 7, Y = "secret" };
            JsonUtils.SerializeToJsonFile(obj, _tempFile);
            var text = File.ReadAllText(_tempFile);
            StringAssert.Contains("\"X\": 7", text);
            StringAssert.Contains("\"Y\": \"secret\"", text);
        }

        [Test]
        public void SerializeToJsonFile_And_DeserializeFromJsonFile_ObjectRoundTrip()
        {
            var original = new Dummy { X = 5, Y = "hello" };
            JsonUtils.SerializeToJsonFile(original, _tempFile);

            Assert.IsTrue(File.Exists(_tempFile));
            var deserialized = JsonUtils.DeserializeFromJsonFile<Dummy>(_tempFile);

            Assert.NotNull(deserialized);
            Assert.AreEqual(5, deserialized.X);
            Assert.AreEqual("hello", deserialized.Y);
        }

        [Test]
        public void SerializeListToJsonFile_WritesJsonArray()
        {
            var list = new List<Dummy> { new Dummy { X = 1, Y = "a" } };
            JsonUtils.SerializeListToJsonFile(list, _tempListFile);
            var content = File.ReadAllText(_tempListFile).TrimStart();
            Assert.IsTrue(content.StartsWith("["), "Expected JSON array to start with '['");
            StringAssert.Contains("\"X\": 1", content);
        }

        [Test]
        public void DeserializeListFromJsonFile_ListRoundTrip()
        {
            var list = new List<Dummy>
            {
                new Dummy{ X = 1, Y = "a"},
                new Dummy{ X = 2, Y = "b"}
            };
            JsonUtils.SerializeListToJsonFile(list, _tempListFile);

            var read = JsonUtils.DeserializeListFromJsonFile<Dummy>(_tempListFile);
            Assert.AreEqual(2, read.Count);
            Assert.AreEqual("a", read[0].Y);
            Assert.AreEqual(2, read[1].X);
        }

        [Test]
        public void DeserializeListFromJsonFile_Nonexistent_ReturnsEmptyList()
        {
            var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var result = JsonUtils.DeserializeListFromJsonFile<Dummy>(path);
            Assert.IsNotNull(result);
            Assert.IsEmpty(result);
        }

        // JsonLineReader Tests
        [Test]
        public void ReadJsonValueAtLine_ValidStringProperty_ReturnsValue()
        {
            string value = JsonUtils.ReadJsonValueAtLine(_tempFile, 2);
            Assert.AreEqual("English", value);
        }

        [Test]
        public void ReadJsonValueAtLine_ValidStringProperty_NoTrailingComma()
        {
            var lines = File.ReadAllLines(_tempFile);
            lines[2] = "  \"HELLO\": \"Hello\"";
            File.WriteAllLines(_tempFile, lines);
            string value = JsonUtils.ReadJsonValueAtLine(_tempFile, 3);
            Assert.AreEqual("Hello", value);
        }

        [Test]
        public void ReadJsonValueAtLine_NonStringValue_ReturnsRawToken()
        {
            string raw = JsonUtils.ReadJsonValueAtLine(_tempFile, 4);
            Assert.AreEqual("123", raw);
        }

        [Test]
        public void ReadJsonValueAtLine_BooleanValue_ReturnsRawToken()
        {
            string raw = JsonUtils.ReadJsonValueAtLine(_tempFile, 5);
            Assert.AreEqual("true", raw);
        }

        [Test]
        public void ReadJsonValueAtLine_LineBeyondEnd_ReturnsNull()
        {
            Assert.AreEqual("", JsonUtils.ReadJsonValueAtLine(_tempFile, 10));
        }

        [Test]
        public void ReadJsonValueAtLine_InvalidLineNumber_Throws()
        {
            Assert.AreEqual("",JsonUtils.ReadJsonValueAtLine(_tempFile, 0));
        }

        [Test]
        public void ReadJsonValueAtLine_FileDoesNotExist_ThrowsFileNotFoundException()
        {
            var missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
            Assert.Throws<FileNotFoundException>(() =>
                JsonUtils.ReadJsonValueAtLine(missing, 1));
        }

        [Test]
        public void ReadJsonValueAtLine_LineWithoutColon_ReturnsNull()
        {
            var lines = File.ReadAllLines(_tempFile);
            lines[2] = "  \"Malformed line without colon\"";
            File.WriteAllLines(_tempFile, lines);
            Assert.AreEqual("", JsonUtils.ReadJsonValueAtLine(_tempFile, 3));
        }
    }
}
