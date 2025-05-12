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

namespace SystemGen
{
    public class SystemManager : MonoBehaviour
    {
        private string systemFile = Application.streamingAssetsPath;
        [SerializeField] public SystemProperties systemProperties;
        [SerializeField] private Object starPrefab;
        [SerializeField] private Object planetPrefab;
        [SerializeField] private Object beltPrefab;
        [SerializeField] private InputField nameField;
        [SerializeField] private GameObject infoField;
        [SerializeField] private GameObject cameraController;

        public void RecieveSystem(string fileAddress, string systemName)
        {
            Logger.Log("SystemManager", $"Recieved system {systemName} from {fileAddress}");
            // Set the system properties
            this.systemFile = fileAddress;
            // Set the name of the system
            nameField.text = systemName;
        }

        public void SetName(string name)
        {
            nameField.text = name;
            Logger.Log("SystemManager", $"System name set to {nameField.text}");
        }

        public void RandomiseName()
        {
            // Generate a random name for the system
            nameField.text = RandomUtils.GenerateSystemName(); 
            Logger.Log("SystemManager", $"Randomised system name to {nameField.text}");
        }

        public void SetInfoBox(string text)
        {             
            // Set the text of the info box
            infoField.GetComponent<Text>().text = text; 
            Logger.Log("SystemManager", $"Info box set to {text}");
        }

        public void GenerateSystem()
        {
            Transform.FindObjectOfType<SystemGenerator>().StartGeneration(nameField.text);
        }

        public void SaveSystem()
        {
            systemProperties = new SystemProperties(nameField.text);

            List<StarProperties> stellarBodies = new List<StarProperties>();
            List<BodyProperties> solidBodies = new List<BodyProperties>();
            List<BeltProperties> belts = new List<BeltProperties>();

            // Get the properties classes from every instantiated object
            foreach (GameObject star in Object.FindObjectsOfTypeIncludingAssets(typeof(StarProperties)).Cast<GameObject>())
            {
                stellarBodies.Add(star.GetComponent<StarProperties>());
            }
            foreach (GameObject planet in Object.FindObjectsOfTypeIncludingAssets(typeof(BodyProperties)).Cast<GameObject>())
            {
                solidBodies.Add(planet.GetComponent<BodyProperties>());
            }
            foreach (GameObject belt in Object.FindObjectsOfTypeIncludingAssets(typeof(BeltProperties)).Cast<GameObject>())
            {
                belts.Add(belt.GetComponent<BeltProperties>());
            }

            systemProperties.systemAge = stellarBodies[0].Age;

            // Save the System Properties to the JSON file
            JsonUtils.SerializeToJsonFile(systemProperties, systemProperties.seedInput);
        }
        public void LoadSystem()
        {
            // Find all OrbitManager components in the loaded scene
            var orbiters = Object.FindObjectsOfType<OrbitManager>();
            foreach (var orb in orbiters)
            {
                // Destroy the entire GameObject
                Destroy(orb.gameObject);
            }

            Logger.Log("SystemManager", $"Loading system from {systemFile}");
            // Load the System Properties from the JSON file
            systemProperties = JsonUtils.Load(systemFile);

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
                Logger.Log("SystemManager", $"Loading star {star.GetInfo()}");
                //break;
                // Instantiate the star prefab and set its properties
                GameObject starObject = Instantiate(starPrefab) as GameObject;
                starObject.GetComponent<StarManager>().star = star;
                starObject.GetComponent<StarManager>().SetStarProperties();
                starObject.GetComponent<StarManager>().FindParent();
                starObject.GetComponent<StarManager>().RecalculateColour();
            }
            foreach (BodyProperties planet in systemProperties.solidBodies)
            {
                Logger.Log("SystemManager", $"Loading planet {planet.GetInfo()}");
                //break;
                // Instantiate the planet prefab and set its properties
                GameObject planetObject = Instantiate(planetPrefab) as GameObject;
                planetObject.GetComponent<BodyManager>().body = planet;
                planetObject.GetComponent<BodyManager>().ApplyData();
                planetObject.GetComponent<BodyManager>().FindParent();
            }
            foreach (BeltProperties belt in systemProperties.belts)
            {
                Logger.Log("SystemManager", $"Loading belt {belt.GetInfo()}");
                //break;
                // Instantiate the belt prefab and set its properties
                GameObject beltObject = Instantiate(beltPrefab) as GameObject;
                beltObject.GetComponent<BeltManager>().belt = belt;
                beltObject.GetComponent<BeltManager>().ApplyData();
                beltObject.GetComponent<BeltManager>().FindParent();
            }
        }
    }
}