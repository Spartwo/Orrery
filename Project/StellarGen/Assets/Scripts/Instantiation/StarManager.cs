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
        }

        public void CalculateLines()
        {
            // Set boundaries of various visible temperature zones
            CentreLine = PhysicsUtils.ConvertToMetres((float)Math.Sqrt(star.Luminosity));
            AridLine = decimal.Multiply(CentreLine, 0.95m);
            OuterHabitableLine = decimal.Multiply(CentreLine, 1.35m);
            FrostLine = decimal.Multiply(CentreLine, 4.8m);
            InnerLine = star.SublimationRadius;
        }

        public void RecalculateColour()
        {
            Color color = PhysicsUtils.DetermineSpectralColor(star.Temperature);
            // Relay to the orbit line
            star.OrbitLine = ColourUtils.ColorToArray(color);

            // Get the Renderer component from the new cube
            Renderer stellarSurface = transform.GetChild(0).GetComponent<Renderer>();
            // Call SetColor using the shader property name "_Color" and setting the color to red
            stellarSurface.material.SetColor("_Color", color);
            stellarSurface.material.SetColor("_EmissionColor", color);


            // Set light properties
            Light starlight = transform.GetChild(2).GetComponent<Light>();
            starlight.range = (float)(FrostLine * 30);
            starlight.color = color;
        }

        public void SetStarProperties()
        {
            // Pame the root object after the stars unique idenfitier
            gameObject.name = star.SeedValue.ToString();

            CalculateLines();

            int boundScale = 250;
            float innerLine = PhysicsUtils.ConvertToAU(InnerLine) * boundScale;
            float frostLine = PhysicsUtils.ConvertToAU(FrostLine) * boundScale;
            float habitableLine = PhysicsUtils.ConvertToAU(OuterHabitableLine) * boundScale;
            float aridLine = PhysicsUtils.ConvertToAU(AridLine) * boundScale;

            Debug.Log($"Star {star.Name} has a habitable zone of {habitableLine} game metres, an arid zone of {aridLine} game metres, and a frost line of {frostLine} game metres.");

            // Set radiation zone bounds
            //transform.GetChild(1).GetChild(3).localScale = new Vector3(innerLine, innerLine, innerLine);
            // Set arid zone bounds
            AridDisk.transform.localScale = new Vector3(aridLine, aridLine, aridLine);
            // Set habitable zone bounds
            HabitableDisk.transform.localScale = new Vector3(habitableLine, habitableLine, habitableLine);
            // Set frost line bounds
            FrostDisk.transform.localScale = new Vector3(frostLine, frostLine, frostLine);



            float diameter = star.Radius * 2;

            // set size of the star itself relative to earth=1
            transform.GetChild(0).localScale = new Vector3(diameter * 10.9f, diameter * 10.9f, diameter * 10.9f);
            // set size of double click collider
            transform.GetComponent<SphereCollider>().radius = diameter * 109f;
            // All bodies are weighed where 1 = Earth
            float massInEarth = PhysicsUtils.RawToEarthMass(star.Mass);
            // get rigidbody and apply the mass
            transform.GetComponent<Rigidbody>().mass = massInEarth;
        }
        public void FindParent()
        {
            // Find the parent object of the body
            if (star.Parent != 0)
            {
                int parentID = star.Parent;
                // Find the parent object by its ID
                GameObject parentObject = GameObject.Find(parentID.ToString());

                if (parentObject != null)
                {
                    transform.SetParent(parentObject.transform, false);
                    transform.GetComponent<Orbiter>().LoadOrbit(star.Orbit, transform.parent, star.OrbitLine);
                }
            }
            else
            {
                // If no parent, set the star as the root object
                transform.SetParent(GameObject.Find("BarryCentra").transform);
                transform.GetComponent<Orbiter>().LoadOrbit(star.Orbit, null, star.OrbitLine);
            }

        }
    }
}