using System.IO;
using System;
using UnityEngine;

public static class Logger
{
    public enum LogLevel
    {
        LOG,
        TEST,
        WARNING,
        ERROR
    }

    // The file path where the logs will be saved
    private static string logFilePath = $"{Application.streamingAssetsPath}/Logs/Orrery.log";
    private static string errorFilePath = $"{Application.streamingAssetsPath}/Logs/Error.log";

    /// <summary>
    /// Logs a message to the console and appends it to the log file, including a timestamp.
    /// </summary>
    /// <param name="message">The message to be logged.</param>
    /// <param name="source">The process causing the message.</param>
    /// <param name="level">The categorisation of the log.</param>
    public static void Log(string source, string message, LogLevel level = LogLevel.LOG)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        string logMessage = $"[{timestamp} {level}] {source}: {message}";

        // Log the message to the console
        Debug.Log(message);

        // Append to log file
        AppendToLogFile(logMessage, level == LogLevel.ERROR);
    }

    // Log an error
    public static void LogError(string message, string source)
    {
        Log(message, source, LogLevel.ERROR);
    }

    // Log a warning
    public static void LogWarning(string message, string source)
    {
        Log(message, source, LogLevel.WARNING);
    }

    // Log a test
    public static void LogTest(string message, string source)
    {
        Log(message, source, LogLevel.TEST);
    }

    /// <summary>
    /// Clears the log file at the start of the program to ensure a fresh log.
    /// </summary>
    public static void ClearLogFile()
    {
        try
        {
            if (File.Exists(logFilePath))
            {
                File.Delete(logFilePath);
            }
            // Ensure the directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));
            AppendToLogFile("Program Initialised\n-----------------------------", true);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error clearing log file: {ex.Message}");
        }
    }

    /// <summary>
    /// Appends a log entry to the log file with a timestamp.
    /// </summary>
    /// <param name="logEntry">The log entry to be written to the file.</param>
    private static void AppendToLogFile(string logEntry, bool logInError)
    {
        try
        {
            // Append to the log file. The file is created if it doesn't exist.
            using (StreamWriter sw = new StreamWriter(logFilePath, append: true))
            {
                sw.WriteLine(logEntry.ToString());
            }

            // If it's an error message store in the long-term file
            if (logInError)
            {
                using (StreamWriter sw = new StreamWriter(errorFilePath, append: true))
                {
                    sw.WriteLine(logEntry.ToString());
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error writing to log file: {ex.Message}");
        }
    }
}