
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
public class Orbiter : MonoBehaviour
{
    
    struct Constants    //Put somewhere else
    {
        public const float G = 6.67f;
    }

    struct Math         //Put somewhere else
    {
        public const float TAU = 6.28318530718f;
    }

    //Orbital Keplerian Parameters
    [SerializeField] float SemiMajorAxis;        //a - size
    [SerializeField] [Range(0f, 0.99f)]         float Eccentricity;             //e - shape
    [SerializeField] [Range(0f, Math.TAU)]      float Inclination;         //i - tilt
    [SerializeField] [Range(0f, Math.TAU)]      float LongitudeOfAsc;  //n - swivel
    [SerializeField] [Range(0f, Math.TAU)]      float PeriArgument;      //w - position
    [SerializeField] Color OrbitColour;
    [SerializeField] float MeanLongitude;             //L - offset
    [SerializeField] Transform Parent;
    private float MeanAnomaly;
    //line render values
    LineRenderer OrbitRenderer;
    [SerializeField] [Range(0, 360)]  int OrbitResolution;
    //give the render line its properties
    Color ColorS;
    Color ColorE;
    private Vector3[] OrbitalPoints;
    //system loaddata
    protected List<string> SystemDataArray;
    //Settings
    private float AccuracyTolerance = 1e-6f;
    private int MaxIterations = 5;           //usually converges after 3-5 iterations.

    //Numbers which only change if orbit or mass changes
    [HideInInspector] [SerializeField] float mu;
    [HideInInspector] [SerializeField] float n, cosLOAN, sinLOAN, sinI, cosI, trueAnomalyConstant;

   // private void OnValidate() => 
    public void LoadOrbit(int LineReadIndex, string SystemFileAddress, Transform ParentObject)
    {   
        Parent = ParentObject;
        //parse all lines of the system file to a list for reading
        SystemDataArray = File.ReadAllLines(SystemFileAddress).ToList();

        //get saved body data from system file
        SemiMajorAxis = ReturnFileValue(LineReadIndex+1)*149597.870691f;
        Eccentricity = ReturnFileValue(LineReadIndex+2);
        LongitudeOfAsc = ReturnFileValue(LineReadIndex+3)/(360/ Math.TAU);
        Inclination = ReturnFileValue(LineReadIndex+4)/(360/ Math.TAU);
        PeriArgument = ReturnFileValue(LineReadIndex+5)/(360/ Math.TAU);
        //calculate constants with retrieved info
        CalculateSemiConstants();


        OrbitRenderer = GetComponent<LineRenderer>();
        
        float R =  ReturnFileValue(LineReadIndex+6);
        float G =  ReturnFileValue(LineReadIndex+7);
        float B =  ReturnFileValue(LineReadIndex+8);
        //give the render line its properties
        ColorS = new Color(R/255f, G/255f, B/255f, 0);
        ColorE = new Color(R/255f, G/255f, B/255f, 1);
        //apply initial value
        OrbitRenderer.endColor = ColorE;
        
    }
    float ReturnFileValue(int index)
    {
        int FilePoint = SystemDataArray[index].IndexOf("=");
        //read one place from the equals value and forward
        string RetrievedValue = SystemDataArray[index].Substring(FilePoint+2);
        //convert the value string to a usable float
        return(float.Parse(RetrievedValue));
    }
    public float F(float E, float e, float M)  //Function f(x) = 0
    {
        return (M - E + e * Mathf.Sin(E));
    }
    public float DF(float E, float e)      //Derivative of the function
    {
        return (-1f) + e * Mathf.Cos(E);
    }
    public void CalculateSemiConstants()    //Numbers that only need to be calculated once if the orbit doesn't change.
    {
        mu = Constants.G * Parent.gameObject.GetComponent<Rigidbody>().mass;
        n = Mathf.Sqrt(mu / Mathf.Pow(SemiMajorAxis, 3));
        trueAnomalyConstant = Mathf.Sqrt((1 + Eccentricity) / (1 - Eccentricity));
        cosLOAN = Mathf.Cos(LongitudeOfAsc);
        sinLOAN = Mathf.Sin(LongitudeOfAsc);
        cosI = Mathf.Cos(Inclination);
        sinI = Mathf.Sin(Inclination);
    }

    float EccentricAnomalyTrail;
    void Update()
    {
        CalculateSemiConstants();

        float CurrentTime = transform.root.GetComponent<Timekeep>().TimeInSeconds;

        MeanAnomaly = (float)(n * (CurrentTime - MeanLongitude));

        float E1 = MeanAnomaly;   //initial guess
        float difference = 1f;
        for (int i = 0; difference > AccuracyTolerance && i < MaxIterations; i++)
        {
            float E0 = E1;
            E1 = E0 - F(E0, Eccentricity, MeanAnomaly) / DF(E0, Eccentricity);
            difference = Mathf.Abs(E1 - E0);
        }
        float EccentricAnomaly = E1;
        EccentricAnomalyTrail = E1;

        float trueAnomaly = 2 * Mathf.Atan(trueAnomalyConstant * Mathf.Tan(EccentricAnomaly / 2));
        float distance = SemiMajorAxis * (1 - Eccentricity * Mathf.Cos(EccentricAnomaly));

        float cosAOPPlusTA = Mathf.Cos(PeriArgument + trueAnomaly);
        float sinAOPPlusTA = Mathf.Sin(PeriArgument + trueAnomaly);

        float x = distance * ((cosLOAN * cosAOPPlusTA) - (sinLOAN * sinAOPPlusTA * cosI));
        float z = distance * ((sinLOAN * cosAOPPlusTA) + (cosLOAN * sinAOPPlusTA * cosI));      //Switching z and y to be aligned with xz not xy
        float y = distance * (sinI * sinAOPPlusTA);

        //Debug.Log(x + "," + y + "," + z);
        transform.position = new Vector3(x, y, z)/149.597870691f + Parent.position;

        //OrbitRenderer.positionCount = 0;
    }



    private void LateUpdate()
    {
        OrbitDraw();
    }
   

    private void OrbitDraw()
    {
        //declare orbital points array
        OrbitalPoints = new Vector3[OrbitResolution];
        //declare orbital focus position
        Vector3 pos = Parent.transform.position;
        float orbitFraction = 1f / OrbitResolution;

        for (int i = 0; i < OrbitResolution; i++)
        {
            float EccentricAnomaly = EccentricAnomalyTrail + i * orbitFraction * Math.TAU;

            float trueAnomaly = 2 * Mathf.Atan(trueAnomalyConstant * Mathf.Tan(EccentricAnomaly / 2));
            float distance = SemiMajorAxis * (1 - Eccentricity * Mathf.Cos(EccentricAnomaly));

            float cosAOPPlusTA = Mathf.Cos(PeriArgument + trueAnomaly);
            float sinAOPPlusTA = Mathf.Sin(PeriArgument + trueAnomaly);

            float x = distance * ((cosLOAN * cosAOPPlusTA) - (sinLOAN * sinAOPPlusTA * cosI));
            float z = distance * ((sinLOAN * cosAOPPlusTA) + (cosLOAN * sinAOPPlusTA * cosI));
            float y = distance * (sinI * sinAOPPlusTA);

            float meanAnomaly = EccentricAnomaly - Eccentricity * Mathf.Sin(EccentricAnomaly);
            
            OrbitalPoints[i] = pos + new Vector3(x, y, z)/149.597870691f;
        }
        
        OrbitRenderer.positionCount = OrbitResolution;
        OrbitRenderer.SetPositions(OrbitalPoints);
        
        float LineWidth = Vector3.Distance(GameObject.Find("MainCam").transform.position, GameObject.Find("Camera_Focus").transform.position)/1000;
            
        
        ColorS.a = 0.1f; 
        //Apply properties to the orbit line display, end colour is already transparent
        OrbitRenderer.startColor = ColorS;
        OrbitRenderer.SetWidth(LineWidth, LineWidth);
    }

   
}
