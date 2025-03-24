using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using System;
using System.Collections.Generic;
using System.Reflection;
using Tests;

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
        Debug.Log("Starting Unit Tests...");

        // List of test coroutines that have a true bool
        List<IEnumerator> runningTests = new List<IEnumerator>();
        int validTests = 0;

        // Use reflection to find all public bool fields
        FieldInfo[] fields = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            if (field.FieldType == typeof(bool) && (bool)field.GetValue(this))
            { 
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
                        Debug.LogError($"Error invoking test '{methodName}': {ex.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning($"No matching test method found for '{field.Name}'. Expected method name: '{methodName}'");
                }
            }
        }

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

        Debug.Log("All selected tests completed.");
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
        Debug.Log("Running Window Tests...");
        testSuiteA.Test_Window_Opens();
        Debug.Log("Window Tests Complete.");
        yield return null;
    }

    private IEnumerator RunPauseTests()
    {
        Debug.Log("Running Pause Tests...");
        testSuiteA.Test_Pause_Functionality();
        Debug.Log("Pause Tests Complete.");
        yield return null;
    }

    private IEnumerator RunStarTests()
    {
        Debug.Log("Running Star Tests...");
        testSuiteB.Test_Star_Creation();
        Debug.Log("Star Tests Complete.");
        yield return null;
    }

    private IEnumerator RunPlanetTests()
    {
        Debug.Log("Running Planet Tests...");
        testSuiteB.Test_Planet_Creation();
        Debug.Log("Planet Tests Complete.");
        yield return null;
    }

    private IEnumerator RunRockyBodyTests()
    {
        Debug.Log("Running Rocky Body Tests...");
        //testSuiteB.Test_Rocky_Body_Formation();
        Debug.Log("Rocky Body Tests Complete.");
        yield return null;
    }

    private IEnumerator RunBeltTests()
    {
        Debug.Log("Running Asteroid Belt Tests...");
        //testSuiteB.Test_Asteroid_Belt_Creation();
        Debug.Log("Asteroid Belt Tests Complete.");
        yield return null;
    }

    private IEnumerator RunSystemTests()
    {
        Debug.Log("Running System Serialization Tests...");
        testSuiteC.Test_System_Serialization();
        Debug.Log("System Serialization Tests Complete.");
        yield return null;
    }

    private IEnumerator RunSettingsTests()
    {
        Debug.Log("Running Settings Serialization Tests...");
        testSuiteC.Test_Settings_Serialization();
        Debug.Log("Settings Serialization Tests Complete.");
        yield return null;
    }

    private IEnumerator RunOrbitTests()
    {
        Debug.Log("Running Orbital Mechanics Tests...");
        testSuiteD.Test_Orbital_Mechanics();
        Debug.Log("Orbital Mechanics Tests Complete.");
        yield return null;
    }
    #endregion
}
