﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using Models;
using Universe;
using StellarGenHelpers;
using System;
using static UnityEngine.UI.CanvasScaler;

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
            UpdateScale();
        }
        void RotateBody()
        {
            //rotation of body
            float rotationRate = GameObject.Find("Game_Controller").GetComponent<Timekeep>().gameSpeed;
            transform.Rotate(0, (rotationRate / 300) / (600 * Time.deltaTime) / (float)body.Rotation, 0);
        }

        public void FindParent()
        {
            // Find the parent object of the body
            int parentID = body.Parent;
            // Find the parent object by its ID
            parentObject = GameObject.Find(parentID.ToString());

            transform.SetParent(parentObject.transform, false);
        }

        public void UpdateScale()
        {
            float scale = GameObject.Find("Game_Controller").GetComponent<SystemManager>().objectScale;

            float diameter = scale * body.Radius * 2;
            // Set size of the body itself relative to earth=1
            transform.GetChild(0).localScale = new Vector3(diameter, diameter, diameter);
            // Set size of double click colliders
            SphereCollider[] colliders = GetComponentsInChildren<SphereCollider>();
            colliders[0].radius = diameter * 2;
            colliders[1].radius = diameter * 100f;
        }

        // ApplyData is called by UI 
        public void ApplyData()
        {
            // Name the root object after the body's unique idenfitier
            gameObject.name = body.SeedValue.ToString();

            // All bodies are weighed where 1 = Earth
            float massInEarth = PhysicsUtils.RawToEarthMass(body.Mass);
            // Get rigidbody and apply the mass
            transform.GetComponent<Rigidbody>().mass = massInEarth;
        }

        public void ApplyColour()
        {
            Color color = ColourUtils.ArrayToColor(body.OrbitLine);
            // Relay to the orbit line
            try
            {
                // Get the Renderer component from the new cube
                Renderer surface = transform.GetChild(0).GetComponent<Renderer>();
                surface.material.SetColor("_Color", color);
            }
            catch (Exception e)
            {
                Logger.Log("StarManager", $"Error setting surface colour: {e}");
            }
        }
    }
}