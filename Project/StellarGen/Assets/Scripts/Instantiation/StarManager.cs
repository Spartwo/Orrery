using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Linq;

public class StarManager : MonoBehaviour
{
    /*

    // set boundaries of various visible temperature zones
    float CenterLine = Mathf.Sqrt(Luminosity);
    float AridLine = CenterLine * 0.95f;
    float OuterHabitableLine = CenterLine * 1.35f;
    float FrostLine = CenterLine * 4.8f;
    float Heliopause = CenterLine * 75f;

    
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
        


        //Get the Renderer component from the new cube
        var StellarSurfaceTemp = transform.GetChild(0).GetComponent<Renderer>();
        //Call SetColor using the shader property name "_Color" and setting the color to red
        StellarSurfaceTemp.material.SetColor("_Color", StellarSurface);
        StellarSurfaceTemp.material.SetColor("_EmissionColor", StellarSurface);

        //set light properties
        Light Starlight = transform.GetChild(2).GetComponent<Light>();
        Starlight.range = FrostLine*BoundScale*20;
        Starlight.color = StellarSurface;

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

    /// <summary>
    /// Converts the given temperature into a color by sampling a gradient image.
    /// </summary>
    /// <param name="temperature">The temperature of the star in Kelvin.</param>
    /// <returns>A System.Drawing.Color sampled from the gradient based on the given temperature.</returns>
    // Done with help from GPT, unity has a gradient system but this is more detached
    public Color DetermineSpectralColor(double temperature)
    {
        // Load the PNG file as a byte array
        byte[] fileData = File.ReadAllBytes("Assets/Materials/gradient.png");

        
        Texture2D texture = new Texture2D(2, 2);
        if (!texture.LoadImage(fileData))
        {
            Debug.LogError("Failed to load PNG file.");
            return Color.magenta;
        }

        // Extract the width of the image
        int imageWidth = texture.width;

        // Validate the temperature input incase of manual mass edits
        if (temperature < 1000 || temperature > 11000)
        {
            Debug.LogWarning("Temperature out of expected range (1000K to 11000K). Clamping to valid range.");
            temperature = Mathf.Clamp((float)temperature, 1000, 11000);
        }

        // Scale the temperature to the image index range (0 to imageWidth - 1)
        int pixelIndex = Mathf.RoundToInt((float)((temperature - 1000) / (11000 - 1000) * (imageWidth - 1)));

        // Extract all pixels from the texture
        Color[] colors = texture.GetPixels();

        // Return the corresponding color
        return colors[pixelIndex];

    }
}
