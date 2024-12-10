using System;
using System.Collections.Generic;
using Random = System.Random;
using UnityEngine;

public class Planet : Body
{
    public Planet(int seedValue) : base(seedValue)
    {
        // No need to set seedValue again because it is already set by the base class constructor
    }

    void PlanetGen()
    {

    }

    public void GenerateChildren()
    {
        base.GenerateChildren(new List<Body>());
    }

    private void PlanetGen(string ParentBody, string ParentBodyName, int BodyNumber, float MeanEccentricity, float MaxInclination, float OrbitScale)
    {



        float Mass = 1;
        float RotationRate = 24;
        float AxialTilt = 0;


        //call the orbit generation method and add to the saved print data
        //BodyOrbitArray.Add(SetOrbit(OrbitalPositions[i] * OrbitScale, MeanEccentricity, MaxInclination));
        //remove SMA index from the array
        //OrbitalPositions.RemoveAt(i);
    }


    /// <summary>
    /// Generates a stellar mass based on probability values along a curve
    /// {{0, 0.1}, {0.16, 0.2}, {0.47, 0.5}, {0.7, 0.75}, {0.9, 1.5}, {0.96, 3.5}, {1, 5}}
    /// </summary>
    /// <param name="seedValue">The numerical seed for this </param>
    /// <returns>A stellar mass in sols</returns>
    private void GeneratePlanetaryComposition()
    {
       /* // Instantiate random number generator 
        Random rand = new Random(seedValue);

        // Get a random percentile
        float graphX = (float)rand.NextDouble();
        // Convert it to stellar masses between 0.1 and 5 using the formula
        float graphY = (0.1f + ((-0.374495f * graphX) / (-1.073858f + graphX)));

        Debug.Log("Sols Mass: " + graphY);
        return graphY;


        //print body data to file
        for (int i = 0; i < BodyCount; i++)
        {
            string PlanetaryData = "\nBODY_" + (i + 1) + " {"
            + BodyDataArray[i]
            + "\n"
            + BodyOrbitArray[i]
            + "\n\tCOMPOSITION {"
            //+ BodyCompArray[i]
            + "\n\t}"
            + "\n\tATMOSPHERE {"
            //+ BodyAtmoArray[i]
            + "\n\t}\n}\n";
            File.AppendAllText(SystemFileName, PlanetaryData);
        }*/
    }
}

// Orbital properties are a subvalue of all bodies
[Serializable]
public class SiderialProperties
{
    private double semiMajorAxis; // Semi-Major Axis
    private float eccentricity; // Eccentricity
    private float longitudeOfAscending; // Longitude of Ascending Node
    private float inclination; // Inclination
    private float periArgument; // Argument of Periapsis

    // Constructor
    public SiderialProperties(double semiMajorAxis, float eccentricity, float longitudeOfAscending, float inclination, float periArgument)
    {
        this.SemiMajorAxis = Math.Max(semiMajorAxis, 0.0001);
        this.Eccentricity = Math.Clamp(eccentricity, 0f, 0.9999f); ;
        this.LongitudeOfAscending = longitudeOfAscending;
        this.Inclination = inclination;
        this.PeriArgument = periArgument;
    }

    #region Getter and Setters
    public double SemiMajorAxis
    {
        get => semiMajorAxis;
        set => semiMajorAxis = Math.Max(value, 0.0001);
    }

    public float Eccentricity
    {
        get => eccentricity;
        // Eccentricity cannot be more than 1
        set => eccentricity = Math.Clamp(value, 0f, 0.9999f);
    }

    public float LongitudeOfAscending
    {
        get => longitudeOfAscending;
        set => longitudeOfAscending = value;
    }

    public float Inclination
    {
        get => inclination;
        set => inclination = value;
    }

    public float PeriArgument
    {
        get => periArgument;
        set => periArgument = value;
    }
    #endregion
}


