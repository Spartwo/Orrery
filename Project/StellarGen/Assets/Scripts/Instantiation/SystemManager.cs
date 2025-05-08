using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Linq;
using Models;

namespace SystemGen
{
    public class SystemManager : MonoBehaviour
    {
        //public knowledge variables that will be accessed by UI
        [HideInInspector] public string BodySearchTerm;
        [SerializeField] public BaseProperties Body;
        public string ParentObject;
        public string BodyName;
        [SerializeField][Range(0f, 70f)] float RotationRate;
        [HideInInspector] public string SystemFileName;

        // Start is called before the first frame update
        void Start()
        {
            //name the root object after the stars unique idenfitier
            gameObject.name = BodySearchTerm;

            //recombine the system file directory address
            string SystemFileAddress = Application.streamingAssetsPath
                + "/Star_Systems/"
                + SystemFileName
                + ".system";


            ApplyData();
        }

        // Update is called once per frame
        void Update()
        {
            ApplyData();
            RotateBody();
        }
        void RotateBody()
        {
            /*
            //rotation of body
            rotationrate
            float Rate = GameObject.Find("Barycenter").GetComponent<Timekeep>().GameSpeed;
            transform.Rotate(0, (Rate / 300) / (600 * Time.deltaTime), 0);
            */
        }


        // ApplyData is called by UI 
        public void ApplyData()
        {
            /*
           // float Diameter = 2 * Body.Radius;


            //set size of the star itself relative to earth=1
            transform.GetChild(0).localScale = new Vector3(Diameter / 100, Diameter / 100, Diameter / 100);
            //set size of double click collider
            transform.GetComponent<SphereCollider>().radius = Diameter;
            //All bodies are weighed where 1 = Earth
            float MassInEarth = BodyMass;
            //get rigidbody and apply the mass
            transform.GetComponent<Rigidbody>().mass = MassInEarth / 10000;*/
        }
        /*

        /// <summary>
        /// Adds a new child body to the list of child bodies.
        /// </summary>
        /// <param name="newChild">The new body to be added to the child list</param>
        public void AddPlanet(PlanetProperties newChild)
        {
            // Check if a body with the same seedValue isn't already present
            if (!Body.ChildBodies.Any(child => child.SeedValue == newChild.SeedValue))
            {
                Body.ChildBodies.Add(newChild);
            }
        }

        /// <summary>
        /// Removes a child body from the list by its seed value.
        /// </summary>
        /// <param name="seedValue">The seed value of the body to be removed</param>
        /// <returns>The removed body if found, otherwise null</returns>
        public BaseProperties RemovePlanet(int seedValue)
        {
            // Find the body with the matching seedValue
            PlanetProperties bodyToRemove = Body.ChildBodies.FirstOrDefault(child => child.SeedValue == seedValue);

            if (bodyToRemove != null)
            {
                // Remove the found body from the list
                Body.ChildBodies.Remove(bodyToRemove);
                return bodyToRemove;
            }
            else
            {
                Logger.LogWarning(GetType().Name, $"Child body {seedValue} not found.");
                return null;
            }
        }

        /// <summary>
        /// Adds a new child body to the list of child bodies.
        /// </summary>
        /// <param name="newChild">The new body to be added to the child list</param>
        public void AddBelt(BeltProperties newChild)
        {
            // Check if a body with the same seedValue isn't already present
            if (!Body.Belts.Any(child => child.SeedValue == newChild.SeedValue))
            {
                Body.Belts.Add(newChild);
            }
        }

        /// <summary>
        /// Removes a child body from the list by its seed value.
        /// </summary>
        /// <param name="seedValue">The seed value of the body to be removed</param>
        /// <returns>The removed body if found, otherwise null</returns>
        public BeltProperties RemoveBelt(int seedValue)
        {
            // Find the body with the matching seedValue
            BeltProperties bodyToRemove = Body.Belts.FirstOrDefault(child => child.SeedValue == seedValue);

            if (bodyToRemove != null)
            {
                // Remove the found body from the list
                Body.Belts.Remove(bodyToRemove);
                return bodyToRemove;
            }
            else
            {
                Logger.LogWarning(GetType().Name, $"Child body {seedValue} not found.");
                return null;
            }
        }*/
    }
}