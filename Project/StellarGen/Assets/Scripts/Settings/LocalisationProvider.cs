using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using StellarGenHelpers;
using Unity.VisualScripting;
using UnityEngine;

namespace Settings
{
    public static class LocalisationProvider
    {
        // Cached localisation data for the current language.
        private static Dictionary<string, string> localisationCache;

        private static readonly string localisationFolderPath = $"{Application.streamingAssetsPath}/localisation/";
        public static bool IsLoaded { get; private set; } = false;

        /// <summary>
        /// Loads localisation data from the JSON file
        /// </summary>
        public static void LoadLoc(string file)
        {
            string locFile = $"{localisationFolderPath}{file}.loc";
            if (!File.Exists(locFile))
            {
                Logger.LogError("SettingsManager", $"Couldn't find loc file {file}");
                return;
            }

            // Load raw JSON for keys and values
            string json = File.ReadAllText(locFile);
            try
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                localisationCache = data ?? new Dictionary<string, string>();
                Logger.Log("SettingsManager", "Loaded localisation");
                IsLoaded = true;
            }
            catch (Exception ex)
            {
                Logger.LogError("SettingsManager", $"Failed to parse localisation file {file}: {ex.Message}");
            }
        }

        // Returns the available loc files and their display names.
        public static List<(string, string)> GetLocOptions()
        {
            List<(string, string)> values = new List<(string, string)>();

            string[] files = Directory.GetFiles(localisationFolderPath, "*.loc");
            foreach (var file in files)
            {
                try
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    string jsonContent = File.ReadAllText(file);
                    var jsonData = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);

                    if (jsonData != null && jsonData.ContainsKey("#loc_DisplayName"))
                    {
                        string displayName = jsonData["#loc_DisplayName"];
                        values.Add((fileName, displayName));
                    }
                    else
                    {
                        Logger.LogWarning("LocalisationProvider", $"File {fileName} does not contain a '#loc_DisplayName' key.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("LocalisationProvider", $"Error reading localisation file {file}: {ex.Message}");
                }
            }

            return values;
        }

        /// <summary>
        /// Returns the localized text for the given key.
        /// </summary>
        public static string GetLocalisedString(string key)
        {
            if (localisationCache.TryGetValue(key, out string localisedString))
            {
                return localisedString;
            }
            else
            {
                Logger.LogWarning("LocalisationProvider", $"Key '{key}' not found in localisation data.");
                return key; // Return the key itself if not found.
            }
        }

        /// <summary>
        /// Forces a reload of localisation data.
        /// </summary>
        //void ReloadLocalisation();
    }
}
