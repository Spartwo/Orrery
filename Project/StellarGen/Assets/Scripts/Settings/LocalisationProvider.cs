using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using StellarGenHelpers;
using Unity.VisualScripting;
using UnityEngine;

namespace Settings
{
    public class LocalisationProvider
    {
        // Cached localisation data for the current language.
        private static Dictionary<string, string> localisationCache
        private static string cachedLanguage;

        private static readonly string localisationFolderPath = $"{Application.streamingAssetsPath}/localisation/";
        private static SettingsData settings;
        public static bool IsLoaded { get; private set; } = false;

        /// <summary>
        /// Loads localisation data from the JSON file
        /// </summary>
        public static void LoadLoc(string file)
        {
            string locFile = $"{localisationFolderPath}{file}.json";
            if (File.Exists(locFile))
            {
                settings = JsonUtils.DeserializeJsonFromFile<SettingsData>(locFile);
                Logger.Log("SettingsManager", "Loaded localisation");
            }
            else
            {
                Logger.LogError("SettingsManager", $"Couldn't find loc file {file}");
                break;
            }

            IsLoaded = true;
            ApplySettings();
        }

        // Returns the available loc files and their display names.
        public static List<(string, string)> GetLocOptions()
        {
            List values = new List<(string, string)>();

            string files = Directory.GetFiles(localisationFolderPath, "*.json");
            foreach (var file in files)
            {
                try
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    string jsonContent = File.ReadAllText(file);
                    var jsonData = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);

                    if (jsonData != null && jsonData.ContainsKey("DisplayName"))
                    {
                        string displayName = jsonData["DisplayName"];
                        values.Add((fileName, displayName));
                    }
                    else
                    {
                        Logger.LogWarning("LocalisationProvider", $"File {fileName} does not contain a 'DisplayName' key.");
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
        string GetLocalizedString(string key);

        /// <summary>
        /// Forces a reload of localisation data.
        /// </summary>
        void ReloadLocalisation();
    }
}
