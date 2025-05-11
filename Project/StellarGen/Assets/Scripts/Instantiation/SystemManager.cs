using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using Models;
using StellarGenHelpers;
using System.Numerics;

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
        }

        public void SetInfoBox(string text)
        {             
            // Set the text of the info box
            infoField.GetComponent<Text>().text = text;
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
            foreach (GameObject star in GameObject.FindGameObjectsWithTag("Star"))
            {
                stellarBodies.Add(star.GetComponent<StarProperties>());
            }
            foreach (GameObject planet in GameObject.FindGameObjectsWithTag("Planet"))
            {
                solidBodies.Add(planet.GetComponent<BodyProperties>());
            }
            foreach (GameObject belt in GameObject.FindGameObjectsWithTag("Belt"))
            {
                belts.Add(belt.GetComponent<BeltProperties>());
            }

            systemProperties.systemAge = stellarBodies[0].Age;

            // Save the System Properties to the JSON file
            JsonUtils.SerializeToJsonFile(systemProperties, systemProperties.seedInput);
        }
        public void LoadSystem()
        {
            // Load the System Properties from the JSON file
            systemProperties = JsonUtils.DeserializeFromJsonFile<SystemProperties>(systemFile);

            foreach (StarProperties star in systemProperties.stellarBodies)
            {
                // Instantiate the star prefab and set its properties
                GameObject starObject = Instantiate(starPrefab) as GameObject;
                starObject.GetComponent<StarManager>().star = star;
                starObject.GetComponent<StarManager>().SetStarProperties();
                starObject.GetComponent<StarManager>().FindParent();
                starObject.GetComponent<StarManager>().RecalculateColour();
            }
            foreach (BodyProperties planet in systemProperties.solidBodies)
            {
                // Instantiate the planet prefab and set its properties
                GameObject planetObject = Instantiate(planetPrefab) as GameObject;
                planetObject.GetComponent<BodyManager>().body = planet;
                planetObject.GetComponent<BodyManager>().ApplyData();
                planetObject.GetComponent<BodyManager>().FindParent();
            }
            foreach (BeltProperties belt in systemProperties.belts)
            {
                // Instantiate the belt prefab and set its properties
                GameObject beltObject = Instantiate(beltPrefab) as GameObject;
                beltObject.GetComponent<BeltManager>().belt = beltObject;
                beltObject.GetComponent<BeltManager>().ApplyData();
                beltObject.GetComponent<BeltManager>().FindParent();
            }
        }
    }
}