using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using System;
using System.Collections.Generic;
using System.Reflection;
using Tests;

namespace Tests
{
    public class TestController : MonoBehaviour
    {

        [Header("UI Tests")]
        public bool runWindowTests = true;
        public bool runPauseTests = true;

        [Header("Generation Tests")]
        public bool runStarTests = true;
        public bool runPlanetTests = true;
        public bool runRockyBodyTests = true;
        public bool runBeltTests = true;

        [Header("I/O Tests")]
        public bool runSystemTests = true;
        public bool runSettingsTests = true;

        [Header("Physics Tests")]
        public bool runOrbitTests = true;

        private UITests testSuiteA;
        private GenerationTests testSuiteB;
        private SerialisationTests testSuiteC;
        private PhysicsTests testSuiteD;



        void Start()
        {
            testSuiteA = new UITests();
            testSuiteB = new GenerationTests();
            testSuiteC = new SerialisationTests();
            testSuiteD = new PhysicsTests();

            StartCoroutine(RunTests());
        }

        /// <summary>
        /// Check the booleans to find and assign appropriate test routines
        /// More easily expandible than hardcoded checks
        /// </summary>
        private IEnumerator RunTests()
        {
            bool testsEnabled = false;

            // List of test coroutines that have a true bool
            List<IEnumerator> runningTests = new List<IEnumerator>();
            int validTests = 0;

            // Use reflection to find all public bool fields
            FieldInfo[] fields = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (FieldInfo field in fields)
            {
                if (field.FieldType == typeof(bool) && (bool)field.GetValue(this))
                {
                    testsEnabled = true;
                    // Capitalise run into Run as a method name
                    string methodName = "Run" + field.Name.Substring(3);

                    MethodInfo testMethod = GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);

                    if (testMethod != null)
                    {
                        try
                        {
                            runningTests.Add((IEnumerator)testMethod.Invoke(this, null));
                            validTests++;
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(GetType().Name, $"Error invoking test '{methodName}': {ex.Message}");
                        }
                    }
                    else
                    {
                        Logger.LogWarning(GetType().Name, $"No matching test method found for '{field.Name}'. Expected method name: '{methodName}'");
                    }
                }
            }

            if (testsEnabled) Logger.Log(GetType().Name, "Starting Unit Tests...");

            // Add listener for test completion
            Action onTestComplete = () => { validTests--; };
            // Execute all valid tests as coroutines
            foreach (IEnumerator test in runningTests)
            {
                StartCoroutine(RunTestCoroutine(test, onTestComplete));
            }

            while (validTests > 0)
            {
                yield return null;
            }

            if (testsEnabled) Logger.Log(GetType().Name, "All selected tests completed.");
        }

        /// <summary>
        /// Runs a test coroutine and marks it as complete when finished.
        /// </summary>
        private IEnumerator RunTestCoroutine(IEnumerator test, Action onComplete)
        {
            yield return StartCoroutine(test);
            onComplete?.Invoke();
        }

        #region Individual Test Coroutines
        // Individual test coroutines
        private IEnumerator RunWindowTests()
        {
            Logger.Log("TestController", "Running Window Tests...");
            testSuiteA.Test_Window_Opens();
            Logger.Log("TestController", "Window Tests Complete.");
            yield return null;
        }

        private IEnumerator RunPauseTests()
        {
            Logger.Log("TestController", "Running Pause Tests...");
            testSuiteA.Test_Pause_Functionality();
            Logger.Log("TestController", "Pause Tests Complete.");
            yield return null;
        }

        private IEnumerator RunStarTests()
        {
            Logger.Log("TestController", "Running Star Tests...");
            testSuiteB.Test_Star_Creation();
            Logger.Log("TestController", "Star Tests Complete.");
            yield return null;
        }

        private IEnumerator RunPlanetTests()
        {
            Logger.Log("TestController", "Running Planet Tests...");
            testSuiteB.Test_Planet_Creation();
            Logger.Log("TestController", "Planet Tests Complete.");
            yield return null;
        }

        private IEnumerator RunRockyBodyTests()
        {
            Logger.Log("TestController", "Running Rocky Body Tests...");
            //testSuiteB.Test_Rocky_Body_Formation();
            Logger.Log("TestController", "Rocky Body Tests Complete.");
            yield return null;
        }

        private IEnumerator RunBeltTests()
        {
            Logger.Log("TestController", "Running Asteroid Belt Tests...");
            //testSuiteB.Test_Asteroid_Belt_Creation();
            Logger.Log("TestController", "Asteroid Belt Tests Complete.");
            yield return null;
        }

        private IEnumerator RunSystemTests()
        {
            Logger.Log("TestController", "Running System Serialization Tests...");
            testSuiteC.Test_System_Serialization();
            Logger.Log("TestController", "System Serialization Tests Complete.");
            yield return null;
        }

        private IEnumerator RunSettingsTests()
        {
            Logger.Log("TestController", "Running Settings Serialization Tests...");
            testSuiteC.Test_Settings_Serialization();
            Logger.Log("TestController", "Settings Serialization Tests Complete.");
            yield return null;
        }

        private IEnumerator RunOrbitTests()
        {
            Logger.Log("TestController", "Running Orbital Mechanics Tests...");
            testSuiteD.Test_Orbital_Mechanics();
            Logger.Log("TestController", "Orbital Mechanics Tests Complete.");
            yield return null;
        }
        #endregion
    }
}