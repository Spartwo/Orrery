using NUnit.Framework;
using Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Settings.Tests
{
    
    public class LocalisationProviderTests
    {
        private const string TestLocFolderPath = "Assets/StreamingAssets/localisation/";
        private const string TestFileName = "test_loc";

        [SetUp]
        public void Setup()
        {
            // Ensure the test localisation folder exists
            if (!Directory.Exists(TestLocFolderPath))
            {
                Directory.CreateDirectory(TestLocFolderPath);
            }

            // Create a test localisation file
            string testFilePath = $"{TestLocFolderPath}{TestFileName}.loc";
            File.WriteAllText(testFilePath, "{\"DisplayName\": \"Test Language\"}");
        }

        [TearDown]
        public void Teardown()
        {
            // Clean up test localisation files
            string testFilePath = $"{TestLocFolderPath}{TestFileName}.loc";
            if (File.Exists(testFilePath))
            {
                //File.Delete(testFilePath);
            }
        }
    }
}