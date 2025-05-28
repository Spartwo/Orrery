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
using UnityEngine.Experimental.GlobalIllumination;

namespace SystemGen
{
    public class BeltManager : MonoBehaviour
    {
        // Public knowledge variables that will be accessed by UI
        [SerializeField] public BeltProperties belt;
        public GameObject parentObject;

        private float lastOrbitScale;
        private bool lastLogState;

        private LineRenderer orbitRenderer;
        public Material lineMaterial;
        [SerializeField][Range(0, 360)] int orbitResolution;
        // Scaling
        [SerializeField] private SystemManager controller;
        private float orbitScale = 100f;
        private bool useLogScaling = false;

        private void Update()
        {
            UpdateScaling();
        
            // Regenerate the grid if the scaling has changed
            if (orbitScale != lastOrbitScale || useLogScaling != lastLogState)
            {
                GenerateGrid();
            }
        }
        private void GenerateGrid()
        {
            Logger.Log("UI", $"Building Orbital Grid");
            lastOrbitScale = orbitScale;
            lastLogState = useLogScaling;
            DrawOrbitRing();

        }

        public void FindParent()
        {
            // Find the parent object of the body
            int parentID = belt.Parent;
            // Find the parent object by its ID
            parentObject = GameObject.Find(parentID.ToString());

            transform.SetParent(parentObject.transform, false);
            transform.position = parentObject.transform.position;
        }

        // ApplyData is called by UI 
        public void ApplyData()
        {
            // Pame the root object after the stars unique idenfitier
            gameObject.name = belt.SeedValue.ToString();

            controller = GameObject.Find("Game_Controller").GetComponent<SystemManager>();
            orbitRenderer = gameObject.GetComponent<LineRenderer>();

            Logger.Log("Belt Manager", $"Establishing Belt {belt.SeedValue.ToString()}");

            float massInEarth = PhysicsUtils.RawToEarthMass(belt.Mass);
            // Get rigidbody and apply the mass
            transform.GetComponent<Rigidbody>().mass = massInEarth / 10000;
        }

        private void DrawOrbitRing()
        {
            float innerRadiusAU = PhysicsUtils.ConvertToAU(belt.LowerEdge);
            float outerRadiusAU = PhysicsUtils.ConvertToAU(belt.UpperEdge);
            float thicknessAU = outerRadiusAU - innerRadiusAU;

            // Convert AU to world units
            float worldInner = PhysicsUtils.GetWorldDistance(innerRadiusAU, useLogScaling, orbitScale);
            float worldOuter = PhysicsUtils.GetWorldDistance(outerRadiusAU, useLogScaling, orbitScale);
            float thickness = worldOuter - worldInner;
            float centerRadius = (worldOuter + worldInner) / 2f;

            transform.localRotation = Quaternion.Euler(90, 0, 0); // Rotate XY to XZ

            // Update line width to thickness with minimum clamp
            float minThickness = 0.01f;
            orbitRenderer.widthMultiplier = Mathf.Max(thickness, minThickness);

            orbitRenderer.useWorldSpace = false;
            orbitRenderer.positionCount = orbitResolution + 1;
            orbitRenderer.alignment = LineAlignment.TransformZ;
            orbitRenderer.loop = true;
            orbitRenderer.material = lineMaterial;

            Color lineColour = ColourUtils.ArrayToColor(belt.OrbitLine);
            lineColour.a = 0.02f;
            orbitRenderer.startColor = lineColour;
            orbitRenderer.endColor = lineColour;

            // Set points in XZ plane, Y=0
            for (int i = 0; i <= orbitResolution; i++)
            {
                float angle = (2 * Mathf.PI / orbitResolution) * i;
                float x = Mathf.Cos(angle) * centerRadius;
                float y = Mathf.Sin(angle) * centerRadius;
                orbitRenderer.SetPosition(i, new Vector3(x, y, 0));
            }
        }
        public void UpdateScaling()
        {
            orbitScale = controller.orbitScale;
            useLogScaling = controller.useLogScaling;
        }
    }
}