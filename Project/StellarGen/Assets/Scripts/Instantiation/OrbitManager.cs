
using System.Collections.Generic;
using System;
using UnityEngine;
using System.IO;
using System.Linq;
using constants = StellarGenHelpers.PhysicalConstants;
using StellarGenHelpers;
using UnityEditor;
using Models;
using Universe;
using System.Numerics;
using Vector3 = UnityEngine.Vector3;


public class OrbitManager : MonoBehaviour
{

    // Orbital Keplerian Parameters
    [SerializeField] private OrbitalProperties orbit;
    [SerializeField] Transform parent;
    [SerializeField] private double meanAnomaly;
    [SerializeField] BigInteger meanLongitude;

    // Line render values
    LineRenderer orbitRenderer;
    [SerializeField] [Range(0, 360)]  int orbitResolution;
    [SerializeField] Color colorStart;
    [SerializeField] Color colorEnd;
    private UnityEngine.Vector3[] orbitalPoints;

    // Settings
    private float accuracyTolerance = 1e-6f;
    private int maxIterations = 5;           //usually converges after 3-5 iterations.

    // Numbers which only change if orbit or mass changes
    [HideInInspector] [SerializeField] double mu;
    [HideInInspector] [SerializeField] double n, cosLOAN, sinLOAN, sinI, cosI, trueAnomalyConstant;

   // private void OnValidate() => 
    public void LoadOrbit(OrbitalProperties orbit, Transform parentObject, int[] orbitLine)
    {
        string parentValue;
        if (parentObject != null)
        {
            parentValue = parentObject.name;
        }
        else
        {
            parentValue = "null";
        }

        // Pass the provided orbit and parent object to the class
        parent = parentObject;
        this.orbit = orbit;

        // Calculate constants with retrieved info
        CalculateSemiConstants();
        
        // Set the orbit line colour gradient
        orbitRenderer = GetComponent<LineRenderer>();

        colorStart = ColourUtils.ArrayToColor(orbitLine);
        colorStart.a = 0f;
        colorEnd = ColourUtils.ArrayToColor(orbitLine);
    }
    public double F(float E, float e, float M)  //Function f(x) = 0
    {
        return (M - E + e * Math.Sin(E));
    }
    public double DF(float E, float e)      //Derivative of the function
    {
        return ((-1f) + e * Math.Cos(E));
    }
    public void CalculateSemiConstants()    //Numbers that only need to be calculated once if the orbit doesn't change.
    {
        mu = (float)(constants.GRAV * PhysicsUtils.(parent.gameObject.GetComponent<Rigidbody>().mass));
        n = (float)Math.Sqrt(mu / (float)PhysicsUtils.DecimalPow(orbit.SemiMajorAxis, 3));
        trueAnomalyConstant = (float)Math.Sqrt((1 + orbit.Eccentricity) / (1 - orbit.Eccentricity));
        cosLOAN = (float)Math.Cos(orbit.LongitudeOfAscending);
        sinLOAN = (float)Math.Sin(orbit.LongitudeOfAscending);
        cosI = (float)Math.Cos(orbit.Inclination);
        sinI = (float)Math.Sin(orbit.Inclination);
    }

    float eccentricAnomalyTrail;
    void Update()
    {
        CalculateSemiConstants();

        // Set the position of the object in the scene
        CalculatePosition(out float x, out float y, out float z);
        transform.position = new Vector3(x, y, z)*10f + parent.position;
    }

    private void LateUpdate()
    {
        OrbitDraw();
    }

    private void CalculatePosition(out float x, out float y, out float z)
    {
        BigInteger currentTime = transform.root.GetComponent<Timekeep>().TimeInSeconds;

        BigInteger meanAnomalyGuess = BigInteger.Multiply((BigInteger)n, currentTime - meanLongitude);

        float E1 = (float)meanAnomalyGuess;   // Initial guess
        float difference = 1f;
        for (int i = 0; difference > accuracyTolerance && i < maxIterations; i++)
        {
            float E0 = E1;
            E1 = (float)(E0 - F(E0, orbit.Eccentricity, E1) / DF(E0, orbit.Eccentricity));
            difference = Mathf.Abs(E1 - E0);
        }
        float eccentricAnomaly = E1;
        eccentricAnomalyTrail = E1;

        float trueAnomaly = 2 * Mathf.Atan((float)(trueAnomalyConstant * Mathf.Tan(eccentricAnomaly / 2)));
        decimal distance = orbit.SemiMajorAxis * (decimal)(1 - orbit.Eccentricity * Mathf.Cos(eccentricAnomaly));

        double cosAOPPlusTA = Math.Cos(orbit.PeriArgument + trueAnomaly);
        double sinAOPPlusTA = Math.Sin(orbit.PeriArgument + trueAnomaly);

        decimal posX = distance * (decimal)((cosLOAN * cosAOPPlusTA) - (sinLOAN * sinAOPPlusTA * cosI));
        decimal posY = distance * (decimal)((sinLOAN * cosAOPPlusTA) + (cosLOAN * sinAOPPlusTA * cosI));
        decimal posZ = distance * (decimal)(sinI * sinAOPPlusTA);


        x = PhysicsUtils.ConvertToAU(posX);
        y = PhysicsUtils.ConvertToAU(posY);
        z = PhysicsUtils.ConvertToAU(posZ);
    }
   

    private void OrbitDraw()
    {
        // Declare orbital points array
        orbitalPoints = new Vector3[orbitResolution];
        // Declare orbital focus position
        Vector3 pos = parent.transform.position;
        float orbitFraction = 1f / orbitResolution;

        for (int i = 0; i < orbitResolution; i++)
        {
            float eccentricAnomaly = (float)(eccentricAnomalyTrail + i * orbitFraction * PhysicalConstants.TAU);

            float trueAnomaly = 2 * Mathf.Atan((float)trueAnomalyConstant * Mathf.Tan(eccentricAnomaly / 2));
            float distance = (float)orbit.SemiMajorAxis * (1 - orbit.Eccentricity * Mathf.Cos(eccentricAnomaly));

            float cosAOPPlusTA = Mathf.Cos(orbit.PeriArgument + trueAnomaly);
            float sinAOPPlusTA = Mathf.Sin(orbit.PeriArgument + trueAnomaly);

            float x = (float)(distance * ((cosLOAN * cosAOPPlusTA) - (sinLOAN * sinAOPPlusTA * cosI)));
            float z = (float)(distance * ((sinLOAN * cosAOPPlusTA) + (cosLOAN * sinAOPPlusTA * cosI)));
            float y = (float)(distance * (sinI * sinAOPPlusTA));

            float meanAnomaly = eccentricAnomaly - orbit.Eccentricity * Mathf.Sin(eccentricAnomaly);
            
            orbitalPoints[i] = pos + new Vector3(x, y, z)/149.597870691f;
        }
        
        orbitRenderer.positionCount = orbitResolution;
        orbitRenderer.SetPositions(orbitalPoints);
        
        float LineWidth = Vector3.Distance(GameObject.Find("MainCam").transform.position, GameObject.Find("Camera_Focus").transform.position)/1000;
            
        
        colorStart.a = 0.1f; 
        //Apply properties to the orbit line display, end colour is already transparent
        orbitRenderer.startColor = colorStart;
        orbitRenderer.SetWidth(LineWidth, LineWidth);
    }

   
}
