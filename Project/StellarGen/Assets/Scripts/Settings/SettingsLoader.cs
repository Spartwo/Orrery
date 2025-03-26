using System;
using UnityEngine;

namespace Settings
{
    public class SettingsLoader : MonoBehaviour
    {
        void Awake()
        {
            GlobalSettings.LoadSettings();
        }
    }
}
