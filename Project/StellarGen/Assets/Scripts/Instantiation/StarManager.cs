using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Linq;
using StellarGenHelpers;
using Models;

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
        aridLine = Decimal.Multiply(centreLine,  0.95m);
        outerHabitableLine = Decimal.Multiply(centreLine, 1.35m);
        frostLine = Decimal.Multiply(centreLine, 4.8m);
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

    /*
    int BoundScale = 250;
        //set radiation zone bounds
        transform.GetChild(1).GetChild(3).localScale = new Vector3(Heliopause * BoundScale, Heliopause * BoundScale, Heliopause * BoundScale);
        //set arid zone bounds
        transform.GetChild(1).GetChild(2).localScale = new Vector3(AridLine * BoundScale, AridLine * BoundScale, AridLine * BoundScale);
        //set habitable zone bounds
        transform.GetChild(1).GetChild(1).localScale = new Vector3(OuterHabitableLine * BoundScale, OuterHabitableLine * BoundScale, OuterHabitableLine * BoundScale);
        //set frost line bounds
        transform.GetChild(1).GetChild(0).localScale = new Vector3(FrostLine * BoundScale, FrostLine * BoundScale, FrostLine * BoundScale);
        //point the indicators towards the camera
        transform.GetChild(1).transform.LookAt(GameObject.Find("MainCam").transform.position);
        


        //set size of the star itself relative to earth=1
        transform.GetChild(0).localScale = new Vector3(Diameter * 10.9f, Diameter * 10.9f, Diameter * 10.9f);
        //set size of double click collider
        transform.GetComponent<SphereCollider>().radius = Diameter*109f;
        //All bodies are weighed where 1 = Earth
        float MassInEarth = StarMass * 333030;
        //get rigidbody and apply the mass
        transform.GetComponent<Rigidbody>().mass = MassInEarth;
        try {
            //apply the mass to the shared barycentre
            GameObject.Find(StarSearchTerm + "_FOCUS").GetComponent<Rigidbody>().mass = MassInEarth;
        } catch {
            //single body systems will catch
        }
    } 
    */


}
