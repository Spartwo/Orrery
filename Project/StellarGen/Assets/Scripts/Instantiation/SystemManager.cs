using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using Models;
using StellarGenHelpers;
using System.Numerics;
using System;
using Object = UnityEngine.Object;
using Universe;

namespace SystemGen
{
    public class SystemManager : MonoBehaviour
    {
        private readonly string assetsFolder = Application.streamingAssetsPath;
        [SerializeField] private string loadedSystemFile;
        [SerializeField] public SystemProperties systemProperties;
        [SerializeField] private Object starPrefab;
        [SerializeField] private Object planetPrefab;
        [SerializeField] private Object beltPrefab;
        [SerializeField] private GameObject nameField;
        [SerializeField] private GameObject infoField;
        [SerializeField] private GameObject cameraController;
        [SerializeField] private SystemListLoad systemsList;

        [SerializeField] public float orbitScale = 100f;
        [SerializeField] public float objectScale = 0.1f;
        [SerializeField] public bool useLogScaling = true;
        [SerializeField] public float lineScale = 1f;

        public void RecieveSystem(string fileAddress, string systemName)
        {
            Logger.Log("SystemManager", $"Recieved system {systemName}");
            loadedSystemFile = fileAddress;
            // Set the name of the system
            nameField.GetComponent<InputField>().text = systemName;
        }

        public void SetName(string name)
        {
            nameField.GetComponent<InputField>().text = name;
            Logger.Log("SystemManager", $"System name set to {name}");
        }

        public void RandomiseName()
        {
            // Generate a random name for the system
            nameField.GetComponent<InputField>().text = RandomUtils.GenerateSystemName(); 
            Logger.Log("SystemManager", $"Randomised system name to {name}");
        }

        public void SetInfoBox(string text)
        {             
            // Set the text of the info box
            infoField.GetComponent<Text>().text = text; 
            Logger.Log("SystemManager", $"Info box set to new data");
        }

        public void GenerateSystem()
        {
            GetComponent<SystemGenerator>().StartGeneration(nameField.GetComponent<InputField>().text);
        }

        public void SaveSystem()
        {
            systemProperties = new SystemProperties(nameField.GetComponent<InputField>().text);

            List<StarProperties> stellarBodies = new List<StarProperties>();
            List<BodyProperties> solidBodies = new List<BodyProperties>();
            List<BeltProperties> belts = new List<BeltProperties>();

            // Get the properties classes from every instantiated object
            foreach (StarManager star in Object.FindObjectsByType(typeof(StarManager), FindObjectsSortMode.None))
            {
                stellarBodies.Add(star.GetComponent<StarManager>().star);
            }
            foreach (GameObject planet in Object.FindObjectsByType(typeof(BodyManager), FindObjectsSortMode.None))
            {
                solidBodies.Add(planet.GetComponent<BodyManager>().body);
            }
            foreach (GameObject belt in Object.FindObjectsByType(typeof(BeltManager), FindObjectsSortMode.None))
            {
                belts.Add(belt.GetComponent<BeltManager>().belt);
            }

            systemProperties.systemAge = stellarBodies[0].Age;

            systemProperties.stellarBodies = stellarBodies;
            systemProperties.solidBodies = solidBodies;
            systemProperties.belts = belts;

            string filePath = $"{assetsFolder}/Star_Systems/{systemProperties.seedInput}.json";

            Logger.Log("System Manager", $"Saving system to {filePath}.json");

            // Save the System Properties to the JSON file
            JsonUtils.SerializeToJsonFile(systemProperties, filePath);
        }
        public void LoadSystem()
        {
            StartCoroutine(LoadSystemCoroutine());

        }
        IEnumerator LoadSystemCoroutine()
        {
            // Make sure camera isn't deleted
            cameraController.GetComponent<CameraMovement>().UpdateSelectedBody(GameObject.Find("0").transform);
            
            yield return null;

            // Destroy existing managers
            foreach (var manager in Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (manager is BodyManager || manager is BeltManager || manager is StarManager)
                    Object.Destroy(manager.gameObject);
            }

            yield return null;

            Logger.Log("SystemManager", $"Loading system from {assetsFolder}/Star_Systems/{loadedSystemFile}");
            // Load the System Properties from the JSON file
            systemProperties = JsonUtils.Load($"{assetsFolder}/Star_Systems/{loadedSystemFile}");

            // Set the barycentre mass
            GameObject centreOfMass = GameObject.Find("0");
            decimal barycentreMass = 0m;
            // Calculate the sum mass of all the stars
            foreach (StarProperties star in systemProperties.stellarBodies)
            {
                barycentreMass += star.Mass;
            }
            // Standardised as earth masses
            centreOfMass.GetComponent<Rigidbody>().mass = PhysicsUtils.RawToEarthMass(barycentreMass);

            foreach (StarProperties star in systemProperties.stellarBodies)
            {
                Logger.Log("SystemManager", $"Loading star {star.SeedValue}");
                // Instantiate the star prefab and set its properties
                GameObject starObject = Instantiate(starPrefab) as GameObject;
                starObject.GetComponent<StarManager>().star = star;
                starObject.GetComponent<StarManager>().SetStarProperties();
                starObject.GetComponent<StarManager>().FindParent();

                // Set the binary status of the star
                starObject.GetComponent<OrbitManager>().SetAsRoot(systemProperties.stellarBodies.Count <= 1);
                Debug.Log($"Star {star.SeedValue} is {systemProperties.stellarBodies.Count > 1} a binary star");
                starObject.GetComponent<OrbitManager>().LoadOrbit(star.Orbit, star.OrbitLine);

                //starObject.GetComponent<OrbitManager>().enabled = false;
                starObject.GetComponent<StarManager>().RecalculateColour();
            }
            foreach (BodyProperties planet in systemProperties.solidBodies)
            {
                Logger.Log("SystemManager", $"Loading planet {planet.SeedValue}");
                // Instantiate the planet prefab and set its properties
                GameObject planetObject = Instantiate(planetPrefab) as GameObject;
                planetObject.GetComponent<BodyManager>().body = planet;
                planetObject.GetComponent<BodyManager>().ApplyData();
                planetObject.GetComponent<BodyManager>().FindParent();

                // Set the binary status of the star
                planetObject.GetComponent<OrbitManager>().SetAsRoot(false);
                planetObject.GetComponent<OrbitManager>().LoadOrbit(planet.Orbit, planet.OrbitLine);
                planetObject.GetComponent<BodyManager>().RecalculateColour();
            }
            foreach (BeltProperties belt in systemProperties.belts)
            {
                Logger.Log("SystemManager", $"Loading belt {belt.SeedValue}");
                break;
                // Instantiate the belt prefab and set its properties
                GameObject beltObject = Instantiate(beltPrefab) as GameObject;
                beltObject.GetComponent<BeltManager>().belt = belt;
                beltObject.GetComponent<BeltManager>().ApplyData();
                beltObject.GetComponent<BeltManager>().FindParent();
            }

            yield return null;

            cameraController.GetComponent<CameraMovement>().UpdateBodyList();
        }
    }
}