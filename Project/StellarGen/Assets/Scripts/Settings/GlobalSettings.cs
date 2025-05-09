using System;
using System.IO;
using UnityEngine;
using JsonUtils = StellarGenHelpers.JsonUtils;

namespace Settings
{
    public static class GlobalSettings
    {
        private static readonly string settingsFilePath = $"{Application.streamingAssetsPath}/settings.json";
        private static SettingsData settings;
        public static bool IsLoaded { get; private set; } = false;

        /// <summary>
        /// Loads settings from the JSON file or creates a default one if missing.
        /// </summary>
        public static void LoadSettings()
        {
            if (File.Exists(settingsFilePath))
            {
                settings = JsonUtils.DeserializeJsonFromFile<SettingsData>(settingsFilePath);
                Logger.Log("SettingsManager", "Loaded settings from file.");
            }
            else
            {
                settings = new SettingsData(); // Load default values
                SaveSettings();
                Logger.Log("SettingsManager", "Created new settings file with default values.");
            }

            IsLoaded = true;
            ApplySettings();
        }

        /// <summary>
        /// Saves the current settings to the JSON file.
        /// </summary>
        public static void SaveSettings()
        {
            JsonUtils.SerializeToJsonFile(settings, settingsFilePath);
            Logger.Log("SettingsManager", "Saved settings to file.");
        }

        /// <summary>
        /// Ensures settings are loaded before accessing values.
        /// </summary>
        private static void EnsureLoaded()
        {
            if (!IsLoaded)
            {
                LoadSettings();
            }
        }

        /// <summary>
        /// Applies necessary settings (e.g., frame rate, VSync) on load.
        /// </summary>
        public static void ApplySettings()
        {
            Application.targetFrameRate = settings.FrameRateLimit;
            QualitySettings.vSyncCount = settings.VSyncEnabled ? 1 : 0;
            LocalisationProvider.LoadLoc(settings.GameLanguage);
        }

        #region Getters and Setters

        // --- Generation Settings ---
        public static bool GenerateOnStartup
        {
            get { EnsureLoaded(); return settings.GenerateOnStartup; }
            set { settings.GenerateOnStartup = value; SaveSettings(); }
        }

        // --- Graphics Settings ---
        public static bool EnableScanlines
        {
            get { EnsureLoaded(); return settings.EnableScanlines; }
            set { settings.EnableScanlines = value; SaveSettings(); }
        }

        public static bool EnableFlicker
        {
            get { EnsureLoaded(); return settings.EnableFlicker; }
            set { settings.EnableFlicker = value; SaveSettings(); }
        }

        public static bool EnableGlow
        {
            get { EnsureLoaded(); return settings.EnableGlow; }
            set { settings.EnableGlow = value; SaveSettings(); }
        }

        public static string ColorPalette
        {
            get { EnsureLoaded(); return settings.ColorPalette; }
            set { settings.ColorPalette = value; SaveSettings(); }
        }

        // --- Performance Settings ---
        public static int FrameRateLimit
        {
            get { EnsureLoaded(); return settings.FrameRateLimit; }
            set { settings.FrameRateLimit = Mathf.Clamp(value, 24, 240); SaveSettings(); ApplySettings(); }
        }

        public static bool VSyncEnabled
        {
            get { EnsureLoaded(); return settings.VSyncEnabled; }
            set { settings.VSyncEnabled = value; SaveSettings(); ApplySettings(); }
        }

        // --- Debug Settings ---
        public static bool ShowDebugLogs
        {
            get { EnsureLoaded(); return settings.ShowDebugLogs; }
            set { settings.ShowDebugLogs = value; SaveSettings(); }
        }
        #endregion
    }
}
