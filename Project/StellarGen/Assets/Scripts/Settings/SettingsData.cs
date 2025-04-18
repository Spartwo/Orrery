using UnityEngine;
using System;

namespace Settings
{
    [Serializable]
    internal class SettingsData
    {
        // Generation settings
        public bool GenerateOnStartup { get; set; } = true;

        // Graphics settings
        public bool EnableScanlines { get; set; } = false;
        public bool EnableFlicker { get; set; } = false;
        public bool EnableGlow { get; set; } = true;
        public string ColorPalette { get; set; } = "Default";

        // Performance settings
        public int FrameRateLimit { get; set; } = 60;
        public bool VSyncEnabled { get; set; } = true;

        // Debug settings
        public bool ShowDebugLogs { get; set; } = false;
    }
}
