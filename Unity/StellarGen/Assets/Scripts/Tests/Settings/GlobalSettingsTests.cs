using NUnit.Framework;
using Settings;
using StellarGenHelpers;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Settings.Tests
{
    public class GlobalSettingsTests
    {
        private string _tempDir;
        private string _settingsFile;

        [SetUp]
        public void SetUp()
        {
            // Create temporary directory for streamingAssets
            _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDir);
            _settingsFile = Path.Combine(_tempDir, "settings.json");

            // Override the private static settingsFilePath field
            var gsType = typeof(GlobalSettings);
            var pathField = gsType.GetField("settingsFilePath", BindingFlags.NonPublic | BindingFlags.Static);
            pathField.SetValue(null, _settingsFile);

            // Ensure parent directory exists
            var dir = Path.GetDirectoryName(_settingsFile);
            Directory.CreateDirectory(dir);

            // Reset loaded flag
            var loadedProp = gsType.GetProperty("IsLoaded", BindingFlags.Public | BindingFlags.Static);
            loadedProp.SetValue(null, false);

            // Clear the cached settings instance
            var settingsField = gsType.GetField("settings", BindingFlags.NonPublic | BindingFlags.Static);
            settingsField.SetValue(null, null);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }

        [Test]
        public void LoadSettings_CreatesFileWithDefaults()
        {
            // Ensure no file exists
            Assert.False(File.Exists(_settingsFile));

            // Load - should create defaults
            GlobalSettings.LoadSettings();
            Assert.True(File.Exists(_settingsFile), "Settings file should be created.");
            Assert.True(GlobalSettings.IsLoaded);

            // Read back JSON
            var data = JsonUtils.DeserializeFromJsonFile<SettingsData>(_settingsFile);
            Assert.NotNull(data);
            Assert.True(data.GenerateOnStartup, "Default GenerateOnStartup should be true.");
            Assert.AreEqual(1500f, data.FloatingPointThreshold);
            Assert.AreEqual(60, data.FrameRateLimit);
            Assert.True(data.VSyncEnabled);
        }

        [Test]
        public void SetGenerateOnStartup_SavesToFile()
        {
            GlobalSettings.LoadSettings();
            GlobalSettings.GenerateOnStartup = false;
            var data = JsonUtils.DeserializeFromJsonFile<SettingsData>(_settingsFile);
            Assert.False(data.GenerateOnStartup);
        }

        [Test]
        public void FrameRateLimit_ClampedAndSaved()
        {
            GlobalSettings.LoadSettings();
            GlobalSettings.FrameRateLimit = 10;  // below min
            Assert.AreEqual(24, GlobalSettings.FrameRateLimit);
            var dataLow = JsonUtils.DeserializeFromJsonFile<SettingsData>(_settingsFile);
            Assert.AreEqual(24, dataLow.FrameRateLimit);

            GlobalSettings.FrameRateLimit = 300;  // above max
            Assert.AreEqual(240, GlobalSettings.FrameRateLimit);
            var dataHigh = JsonUtils.DeserializeFromJsonFile<SettingsData>(_settingsFile);
            Assert.AreEqual(240, dataHigh.FrameRateLimit);
        }

        [Test]
        public void VSyncEnabled_ToggleSavesAndApplies()
        {
            GlobalSettings.LoadSettings();
            GlobalSettings.VSyncEnabled = false;
            var data = JsonUtils.DeserializeFromJsonFile<SettingsData>(_settingsFile);
            Assert.False(data.VSyncEnabled);
        }

        [Test]
        public void ColorPalette_SetterPersists()
        {
            GlobalSettings.LoadSettings();
            GlobalSettings.ColorPalette = "TestPalette";
            var data = JsonUtils.DeserializeFromJsonFile<SettingsData>(_settingsFile);
            Assert.AreEqual("TestPalette", data.ColorPalette);
        }
    }
}
