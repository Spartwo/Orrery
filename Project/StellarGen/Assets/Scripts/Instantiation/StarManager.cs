using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using StellarGenHelpers;
using Models;
using SystemGen;

namespace SystemGen
{
    public class StarManager : MonoBehaviour
    {
        [SerializeField] public StarProperties star;

        decimal InnerLine, AridLine, CentreLine, OuterHabitableLine, FrostLine;

        [SerializeField] GameObject AridDisk, HabitableDisk, FrostDisk, StarIndicators;

        public StarManager(StarProperties star)
        {
            this.star = star;

        }

        public void Update()
        {
            // Point the indicators towards the camera
            StarIndicators.transform.LookAt(GameObject.Find("Main_Camera").transform.position);

            UpdateScale();
            UpdateStarProperties();
        }

        public void CalculateLines()
        {
            // Set boundaries of various visible temperature zones
            CentreLine = PhysicsUtils.ConvertToMetres((float)Math.Sqrt(star.Luminosity));
            AridLine = decimal.Multiply(CentreLine, 0.95m);
            OuterHabitableLine = decimal.Multiply(CentreLine, 1.35m);
            FrostLine = decimal.Multiply(CentreLine, 4.8m);
            InnerLine = star.SublimationRadius;

            Debug.Log($"Star {star.Name} has a habitable zone of {OuterHabitableLine} game metres, an arid zone of {AridLine} game metres, and a frost line of {FrostLine} game metres.");

        }

        public void RecalculateColour()
        {
            Color color = PhysicsUtils.DetermineSpectralColor(star.Temperature);
            // Relay to the orbit line
            star.OrbitLine = ColourUtils.ColorToArray(color);
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

            // Set light properties
            Light starlight = transform.GetChild(2).GetComponent<Light>();
            starlight.range = (float)(FrostLine * 30);
            starlight.color = color;

            Logger.Log("StarManager", $"Star {star.Name} has a temperature of {star.Temperature} K and a colour of {color}");
        }

        public void UpdateStarProperties()
        {
            SystemManager systemManager = GameObject.Find("Game_Controller").GetComponent<SystemManager>();
            float boundScale = systemManager.orbitScale / 4f;
            bool useLogScaling = systemManager.useLogScaling;

            float innerLine = PhysicsUtils.GetWorldDistance(PhysicsUtils.ConvertToAU(InnerLine), useLogScaling, boundScale);
            float frostLine = PhysicsUtils.GetWorldDistance(PhysicsUtils.ConvertToAU(FrostLine), useLogScaling, boundScale);
            float habitableLine = PhysicsUtils.GetWorldDistance(PhysicsUtils.ConvertToAU(OuterHabitableLine), useLogScaling, boundScale);
            float aridLine = PhysicsUtils.GetWorldDistance(PhysicsUtils.ConvertToAU(AridLine), useLogScaling, boundScale);

            // Set arid zone bounds
            AridDisk.transform.localScale = new Vector3(aridLine, aridLine, aridLine);
            // Set habitable zone bounds
            HabitableDisk.transform.localScale = new Vector3(habitableLine, habitableLine, habitableLine);
            // Set frost line bounds
            FrostDisk.transform.localScale = new Vector3(frostLine, frostLine, frostLine);
        }
        public void UpdateScale()
        {
            float scale = GameObject.Find("Game_Controller").GetComponent<SystemManager>().objectScale;

            float diameter = scale * star.Radius * 218;
            // Set size of the body itself relative to earth=1
            transform.GetChild(0).localScale = new Vector3(diameter, diameter, diameter);
            // Set size of double click colliders
            SphereCollider[] colliders = GetComponentsInChildren<SphereCollider>();
            colliders[0].radius = diameter / 2f;
            colliders[1].radius = diameter * 50f;
        }

        public void SetStarProperties()
        {
            // Pame the root object after the stars unique idenfitier
            gameObject.name = star.SeedValue.ToString();

            CalculateLines();

            float scale = GameObject.Find("Game_Controller").GetComponent<SystemManager>().objectScale;

            float diameter = scale * star.Radius * 2;

            // Set size of the star itself relative to earth=1
            transform.GetChild(0).localScale = new Vector3(diameter, diameter, diameter);
            // Set size of double click colliders
            SphereCollider[] colliders = GetComponentsInChildren<SphereCollider>();
            colliders[0].radius = diameter / 25;
            colliders[1].radius = diameter * 2f;

            // All bodies are weighed where 1 = Earth
            float massInEarth = PhysicsUtils.RawToEarthMass(star.Mass);
            // get rigidbody and apply the mass
            transform.GetComponent<Rigidbody>().mass = massInEarth;
        }
        public void FindParent()
        {
            // Find the parent object of the body
            int parentID = star.Parent;
            // Find the parent object by its ID
            GameObject parentObject = GameObject.Find(parentID.ToString());

            transform.SetParent(parentObject.transform, false);

            Logger.Log("StarManager", $"Star {star.Name} has a parent of {star.Parent}");
        }

        #region UI Stuff

        public void DisplayInfo()
        {
            // Get the UI text component
            SystemManager infoController = GameObject.Find("Game_Conroller").GetComponent<SystemManager>();
            // Set the text to display the star's information
            infoController.SetInfoBox(star.GetInfo());
        }

        #endregion
    }
}