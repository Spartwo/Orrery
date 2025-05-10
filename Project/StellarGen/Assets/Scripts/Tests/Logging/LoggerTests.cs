using NUnit.Framework;
using System.IO;
using System;
using UnityEngine;

namespace Tests
{
    public class LoggerTests
    {
        private string logPath;
        private string errorPath;

        [SetUp]
        public void SetUp()
        {
            logPath = Path.Combine(Application.streamingAssetsPath, "Logs", "Orrery.log");
            errorPath = Path.Combine(Application.streamingAssetsPath, "Logs", "Error.log");

            // Ensure a clean slate
            if (File.Exists(logPath)) File.Delete(logPath);
            if (File.Exists(errorPath)) File.Delete(errorPath);

            Directory.CreateDirectory(Path.GetDirectoryName(logPath));
        }

        [Test]
        public void Log_WritesToLogFile()
        {
            Logger.Log("TestComponent", "General message");

            Assert.That(File.Exists(logPath), Is.True, "Log file not created.");

            var content = File.ReadAllText(logPath);
            StringAssert.Contains("TestComponent", content);
            StringAssert.Contains("General message", content);
        }

        [Test]
        public void LogError_WritesToBothLogAndErrorFiles()
        {
            Logger.LogError("TestComponent", "Error occurred");

            Assert.That(File.Exists(logPath), Is.True);
            Assert.That(File.Exists(errorPath), Is.True);

            var logContent = File.ReadAllText(logPath);
            var errorContent = File.ReadAllText(errorPath);

            StringAssert.Contains("Error occurred", logContent);
            StringAssert.Contains("Error occurred", errorContent);
        }

        [Test]
        public void LogWarning_DoesNotWriteToErrorFile()
        {
            Logger.LogWarning("TestComponent", "Potential issue");

            Assert.That(File.Exists(logPath), Is.True);
            Assert.That(File.Exists(errorPath), Is.False, "Warning shouldn't go into error log.");
        }

        [Test]
        public void ClearLogFile_DeletesOldLogAndStartsFresh()
        {
            // Write something
            Logger.Log("Init", "Booting system");

            Assert.That(File.Exists(logPath), Is.True);
            string preClearContent = File.ReadAllText(logPath);
            Assert.That(preClearContent.Length > 0, Is.True);

            Logger.ClearLogFile();

            string postClearContent = File.ReadAllText(logPath);
            StringAssert.Contains("Program Initialised", postClearContent);
        }
    }
}