
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
    [SerializeField] double meanLongitude;
    public bool isRootBody = false; 
    // Line render values
    private LineRenderer orbitRenderer;
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
    public void LoadOrbit(OrbitalProperties orbit, int[] orbitLine)
    {
        // Pass the provided orbit and parent object to the class
        this.orbit = orbit;
        this.parent = transform.parent;

        string parentValue;
        if (parent != null)
        {
            parentValue = parent.name;
        }
        else
        {
            parentValue = "null";
        }

        Logger.Log("Orbit Manager", $"Establishing Orbit for {parentValue}");

        // Set the orbit line colour gradient
        orbitRenderer = GetComponent<LineRenderer>();

        if (!isRootBody)
        {
            // Calculate constants with retrieved info
            CalculateSemiConstants();

            colorStart = ColourUtils.ArrayToColor(orbitLine);
            colorStart.a = 0f;
            colorEnd = ColourUtils.ArrayToColor(orbitLine);
        }
        else
        {
            Destroy(orbitRenderer);
        }

            
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
        bool parentHasVisibleMesh = parent != null && parent.GetComponent<MeshRenderer>() != null;
        double parentMass;

        // Binaries deduct their own mass from that of the barycentre
        if (parentHasVisibleMesh)
        {
            parentMass = parent.gameObject.GetComponent<Rigidbody>().mass;
        }
        else
        {
            parentMass = parent.gameObject.GetComponent<Rigidbody>().mass - GetComponent<Rigidbody>().mass;
        }

        mu = constants.GRAV * parentMass;
        n = (double)Math.Sqrt(mu / Mathf.Pow(PhysicsUtils.ConvertToAU(orbit.SemiMajorAxis), 3));
        trueAnomalyConstant = (float)Math.Sqrt((1 + orbit.Eccentricity) / (1 - orbit.Eccentricity));
        cosLOAN = (float)Math.Cos(orbit.LongitudeOfAscending / (360 / PhysicalConstants.TAU));
        sinLOAN = (float)Math.Sin(orbit.LongitudeOfAscending / (360 / PhysicalConstants.TAU));
        cosI = (float)Math.Cos(orbit.Inclination / (360 / PhysicalConstants.TAU));
        sinI = (float)Math.Sin(orbit.Inclination / (360 / PhysicalConstants.TAU));
        Logger.Log("Orbit Manager", $"Orbit constants calculated: mu = {mu}, n = {n}, trueAnomalyConstant = {trueAnomalyConstant}, cosLOAN = {cosLOAN}, sinLOAN = {sinLOAN}, cosI = {cosI}, sinI = {sinI}");
    }

    float eccentricAnomalyTrail;
    void Update()
    {
        //CalculateSemiConstants();

        // Set the position of the object in the scene
        CalculatePosition(out float x, out float y, out float z);

        transform.position = new Vector3(y * 100f, z * 100f, x * 100f) + parent.transform.position;
    }

    private void LateUpdate()
    {
        if (!isRootBody) { OrbitDraw(); }
    }

    private void CalculatePosition(out float x, out float y, out float z)
    {
        double currentTime = GameObject.Find("Game_Controller").GetComponent<Timekeep>().TimeInSeconds;

        float meanAnomalyGuess = (float)(n * (currentTime - meanLongitude));

        Debug.Log($"Mean Anomaly Guess: {meanAnomalyGuess}, n: {n}, currentTime: {currentTime}, meanLongitude: {meanLongitude}");

        float E1 = meanAnomalyGuess;   // Initial guess
        float difference = 1f;
        for (int i = 0; difference > accuracyTolerance && i < maxIterations; i++)
        {
            float E0 = E1;
            E1 = (float)(E0 - F(E0, orbit.Eccentricity, meanAnomalyGuess) / DF(E0, orbit.Eccentricity));
            difference = Mathf.Abs(E1 - E0);
        }
        float eccentricAnomaly = E1;
        eccentricAnomalyTrail = E1;

        float trueAnomaly = 2 * Mathf.Atan((float)(trueAnomalyConstant * Mathf.Tan(eccentricAnomaly / 2)));
        decimal distance = orbit.SemiMajorAxis * (decimal)(1 - (orbit.Eccentricity * Mathf.Cos(eccentricAnomaly)));

        double cosAOPPlusTA = Math.Cos((orbit.PeriArgument / (360 / PhysicalConstants.TAU)) + trueAnomaly);
        double sinAOPPlusTA = Math.Sin((orbit.PeriArgument / (360 / PhysicalConstants.TAU)) + trueAnomaly);

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
            decimal distance = orbit.SemiMajorAxis * (decimal)(1 - (orbit.Eccentricity * Mathf.Cos(eccentricAnomaly)));

            float cosAOPPlusTA = Mathf.Cos((float)((orbit.PeriArgument / (360 / PhysicalConstants.TAU)) + trueAnomaly));
            float sinAOPPlusTA = Mathf.Sin((float)((orbit.PeriArgument / (360 / PhysicalConstants.TAU)) + trueAnomaly));

            decimal posX = distance * (decimal)((cosLOAN * cosAOPPlusTA) - (sinLOAN * sinAOPPlusTA * cosI));
            decimal posY = distance * (decimal)((sinLOAN * cosAOPPlusTA) + (cosLOAN * sinAOPPlusTA * cosI));
            decimal posZ = distance * (decimal)(sinI * sinAOPPlusTA);

            float x = PhysicsUtils.ConvertToAU(posX);
            float y = PhysicsUtils.ConvertToAU(posY);
            float z = PhysicsUtils.ConvertToAU(posZ);

            float meanAnomaly = (float)(eccentricAnomaly - (orbit.Eccentricity / (360 / PhysicalConstants.TAU)) * Mathf.Sin(eccentricAnomaly));
            
            orbitalPoints[i] = pos + new Vector3(y * 100f, z * 100f, x * 100f);
        }
        
        orbitRenderer.positionCount = orbitResolution;
        orbitRenderer.SetPositions(orbitalPoints);
        
        float LineWidth = Vector3.Distance(GameObject.Find("Main_Camera").transform.position, parent.transform.position)/500;

        colorStart.a = 0.1f; 
        //Apply properties to the orbit line display, end colour is already transparent
        orbitRenderer.startColor = colorStart;
        orbitRenderer.startWidth = LineWidth;
        orbitRenderer.endColor = colorEnd;
        orbitRenderer.endWidth = LineWidth;
    }

    public void SetAsRoot(bool isBinary)
    {
        this.isRootBody = isBinary;
    }
}
