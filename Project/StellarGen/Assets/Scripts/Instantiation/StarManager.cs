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

public class StarManager : MonoBehaviour
{
    public StarProperties star;

    decimal aridLine, centreLine, outerHabitableLine, frostLine;

    public StarManager(StarProperties star)
    {
        this.star = star;

    }

    public void CalculateLines()
    {
        // Set boundaries of various visible temperature zones
        centreLine = PhysicsUtils.ConvertToMetres((float)Math.Sqrt(star.Luminosity));
        aridLine = decimal.Multiply(centreLine,  0.95m);
        outerHabitableLine = decimal.Multiply(centreLine, 1.35m);
        frostLine = decimal.Multiply(centreLine, 4.8m);
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
        starlight.range = (float)(frostLine * 30);
        starlight.color = color;
    }

    public void SetStarProperties()
    {
        int boundScale = 250;
        float innerLine = PhysicsUtils.ConvertToAU(star.SublimationRadius) * boundScale;

        float frostLine = PhysicsUtils.ConvertToAU(star.FrostLine) * boundScale;
        float habitableLine = PhysicsUtils.ConvertToAU(star.HabitableZone) * boundScale;
        float aridLine = habitableLine * 0.75f;

        // Set radiation zone bounds
        transform.GetChild(1).GetChild(3).localScale = new Vector3(innerLine, innerLine, innerLine);
        // Set arid zone bounds
        transform.GetChild(1).GetChild(2).localScale = new Vector3(aridLine, aridLine, aridLine);
        // Set habitable zone bounds
        transform.GetChild(1).GetChild(1).localScale = new Vector3(habitableLine, habitableLine, habitableLine);
        // Set frost line bounds
        transform.GetChild(1).GetChild(0).localScale = new Vector3(frostLine, frostLine, frostLine);
        // Point the indicators towards the camera
        transform.GetChild(1).transform.LookAt(GameObject.Find("MainCam").transform.position);
        


        float diameter = star.Radius * 2;

        // set size of the star itself relative to earth=1
        transform.GetChild(0).localScale = new Vector3(diameter * 10.9f, diameter * 10.9f, diameter * 10.9f);
        // set size of double click collider
        transform.GetComponent<SphereCollider>().radius = diameter*109f;
        // All bodies are weighed where 1 = Earth
        float massInEarth = PhysicsUtils.RawToEarthMass(star.Mass);
        // get rigidbody and apply the mass
        transform.GetComponent<Rigidbody>().mass = massInEarth;
    }
}
