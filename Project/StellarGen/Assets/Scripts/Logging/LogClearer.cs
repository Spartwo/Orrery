
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class LogManager : MonoBehaviour
{
    // Ensure the log file is cleared at the start of each session
    void Start()
    {
        Logger.ClearLogFile();
    }
}