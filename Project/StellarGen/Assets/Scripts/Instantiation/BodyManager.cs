using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using Models;
using Universe;
using StellarGenHelpers;
using System;

namespace SystemGen
{
    public class BodyManager : MonoBehaviour
    {
        // Public knowledge variables that will be accessed by UI
        [SerializeField] public BodyProperties body;
        public GameObject parentObject;

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
            float rotationRate = GameObject.Find("Game_Controller").GetComponent<Timekeep>().gameSpeed;
            transform.Rotate(0, (rotationRate / 300) / (600 * Time.deltaTime) / (float)body.Sidereal.SiderealDayLength, 0);
        }

        public void FindParent()
        {
            // Find the parent object of the body
            int parentID = body.Parent;
            // Find the parent object by its ID
            parentObject = GameObject.Find(parentID.ToString());

            transform.SetParent(parentObject.transform, false);
        }


        // ApplyData is called by UI 
        public void ApplyData()
        {
            // Name the root object after the body's unique idenfitier
            gameObject.name = body.SeedValue.ToString();
            float diameter = body.Radius * 2;

            // Set size of the body itself relative to earth=1
            transform.GetChild(0).localScale = new Vector3(diameter * 30.1f, diameter * 30.1f, diameter * 30.1f);
            // Set size of double click colliders
            SphereCollider[] colliders = GetComponentsInChildren<SphereCollider>();
            colliders[0].radius = diameter * 2;
            colliders[1].radius = diameter * 100f;

            // All bodies are weighed where 1 = Earth
            float massInEarth = PhysicsUtils.RawToEarthMass(body.Mass);
            // Get rigidbody and apply the mass
            transform.GetComponent<Rigidbody>().mass = massInEarth;
        }

        public void RecalculateColour()
        {
            Color color = ColourUtils.ArrayToColor(body.OrbitLine);
            // Relay to the orbit line
            try
            {
                // Get the Renderer component from the new cube
                Renderer stellarSurface = transform.GetChild(0).GetComponent<Renderer>();
                // Call SetColor using the shader property name "_Color" and setting the color to red
                stellarSurface.material.SetColor("_Color", color);
                stellarSurface.material.SetColor("_EmissionColor", color);
                stellarSurface.material.EnableKeyword("_EMISSION");
            }
            catch (Exception e)
            {
                Logger.Log("StarManager", $"Error setting surface colour: {e}");
            }
        }
    }
}