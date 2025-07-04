﻿
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
using SystemGen;
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
    // Scaling
    [SerializeField] private SystemManager controller;
    private float orbitScale = 100f;
    private bool useLogScaling = false;
    private float lineScale = 1f;

    // Settings
    private float accuracyTolerance = 1e-6f;
    private int maxIterations = 5;           //usually converges after 3-5 iterations.

    // Numbers which only change if orbit or mass changes
    [HideInInspector] [SerializeField] double mu;
    [HideInInspector] [SerializeField] double n, cosLOAN, sinLOAN, sinI, cosI, trueAnomalyConstant;

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

        controller = GameObject.Find("Game_Controller").GetComponent<SystemManager>();

        Logger.Log("Orbit Manager", $"Establishing Orbit around {parentValue}");

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
            Destroy(this);
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
        UpdateScaling();

        //CalculateSemiConstants();

        // Set the position of the object in the scene
        UpdatePosition();
    }

    private void LateUpdate()
    {
        if (!isRootBody) { OrbitDraw(); }
    }

    private void UpdatePosition()
    {
        double currentTime = GameObject.Find("Game_Controller").GetComponent<Timekeep>().TimeInSeconds;

        float meanAnomalyGuess = (float)(n * (currentTime - meanLongitude));

        if (orbit == null)
        {
            Debug.LogError($"{name}: Orbit data is not assigned.");
            return;
        }

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

        // Calculte the position of the object in the worldspace
        decimal posX = distance * (decimal)((cosLOAN * cosAOPPlusTA) - (sinLOAN * sinAOPPlusTA * cosI));
        decimal posY = distance * (decimal)((sinLOAN * cosAOPPlusTA) + (cosLOAN * sinAOPPlusTA * cosI));
        decimal posZ = distance * (decimal)(sinI * sinAOPPlusTA);

        float y;
        float z;
        float x;

        if (useLogScaling)
        {
            // Convert position to AU
            Vector3 unscaled = new Vector3(
                PhysicsUtils.ConvertToAU(posY),
                PhysicsUtils.ConvertToAU(posZ),
                PhysicsUtils.ConvertToAU(posX)
            );

            float radiusCompressed = PhysicsUtils.GetWorldDistance(unscaled.magnitude, useLogScaling, orbitScale);
            Vector3 direction = unscaled.normalized;
            Vector3 scaledPosition = direction * radiusCompressed;

            transform.position = scaledPosition + parent.transform.position;
        }
        else
        {
            y = PhysicsUtils.ConvertToAU(posY) * orbitScale;
            z = PhysicsUtils.ConvertToAU(posZ) * orbitScale;
            x = PhysicsUtils.ConvertToAU(posX) * orbitScale;

            transform.position = new Vector3(y, z, x) + parent.transform.position;
        }
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

            float meanAnomaly = (float)(eccentricAnomaly - (orbit.Eccentricity / (360 / PhysicalConstants.TAU)) * Mathf.Sin(eccentricAnomaly));

            // Calculate the position of the orbital point
            decimal posX = distance * (decimal)((cosLOAN * cosAOPPlusTA) - (sinLOAN * sinAOPPlusTA * cosI));
            decimal posY = distance * (decimal)((sinLOAN * cosAOPPlusTA) + (cosLOAN * sinAOPPlusTA * cosI));
            decimal posZ = distance * (decimal)(sinI * sinAOPPlusTA);

            float y;
            float z;
            float x;

            if (useLogScaling)
            {
                // Convert position to AU
                Vector3 unscaled = new Vector3(
                    PhysicsUtils.ConvertToAU(posY),
                    PhysicsUtils.ConvertToAU(posZ),
                    PhysicsUtils.ConvertToAU(posX)
                );

                float radiusCompressed = PhysicsUtils.GetWorldDistance(unscaled.magnitude, useLogScaling, orbitScale);
                Vector3 direction = unscaled.normalized;
                Vector3 scaledPosition = direction * radiusCompressed;

                orbitalPoints[i] = scaledPosition + parent.transform.position;
            }
            else
            {
                y = PhysicsUtils.ConvertToAU(posY) * orbitScale;
                z = PhysicsUtils.ConvertToAU(posZ) * orbitScale;
                x = PhysicsUtils.ConvertToAU(posX) * orbitScale;

                orbitalPoints[i] = new Vector3(y, z, x) + parent.transform.position;
            }
        }
        
        orbitRenderer.positionCount = orbitResolution;
        orbitRenderer.SetPositions(orbitalPoints);
        
        float LineWidth = (Vector3.Distance(GameObject.Find("Main_Camera").transform.position, parent.transform.position) / (orbitScale * 5f)) * lineScale;

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
    public void UpdateScaling()
    {
        orbitScale = controller.orbitScale;
        useLogScaling = controller.useLogScaling;
        lineScale = controller.lineScale;
    }
}
