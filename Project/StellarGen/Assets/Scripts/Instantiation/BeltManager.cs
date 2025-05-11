using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using Models;
using Universe;
using StellarGenHelpers;

namespace SystemGen
{
    public class BodyManager : MonoBehaviour
    {
        // Public knowledge variables that will be accessed by UI
        [SerializeField] public BodyProperties body;
        public GameObject parentObject;
        public string bodyName;
        [SerializeField][Range(0f, 70f)] float rotationRate;

        // Start is called before the first frame update
        void Start()
        {
            ApplyData();
        }

        // Update is called once per frame
        void Update()
        {
            RotateBody();
        }
        void RotateBody()
        {
            //rotation of body
            float rotationRate = GameObject.Find("Barycenter").GetComponent<Timekeep>().GameSpeed;
            transform.Rotate(0, (rotationRate / 300) / (600 * Time.deltaTime), 0);
            
        }

        public void FindParent()
        {
            // Find the parent object of the body
            if (body.Parent != 0)
            {
                int parentID = body.Parent;
                // Find the parent object by its ID
                GameObject parentObject = GameObject.Find(parentID.ToString());

                if (parentObject != null)
                {
                    transform.SetParent(parentObject.transform, false);
                }
            }
        }


        // ApplyData is called by UI 
        public void ApplyData()
        {
            // Pame the root object after the stars unique idenfitier
            gameObject.name = body.SeedValue.ToString();
            float diameter = body.Radius * 2;
            // Set size of the star itself relative to earth=1
            transform.GetChild(0).localScale = new Vector3(diameter / 100, diameter / 100, diameter / 100);
            // Set size of double click collider
            transform.GetComponent<SphereCollider>().radius = diameter;
            // All bodies are weighed where 1 = Earth
            float massInEarth = PhysicsUtils.RawToEarthMass(body.Mass);
            // Get rigidbody and apply the mass
            transform.GetComponent<Rigidbody>().mass = massInEarth / 10000;
        }
    }
}