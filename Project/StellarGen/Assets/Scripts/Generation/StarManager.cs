using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Linq;

public class StarManager : MonoBehaviour
{
    //public knowledge variables that will be accessed by UI
    [SerializeField] [Range(0.05f, 5f)] float StarMass;
    [SerializeField] [Range(0f, 180f)]  float AxialTilt;

    [SerializeField] Transform Parent;
    public string StarName;
    private string StarSearchTerm;
    [SerializeField] float RotationRate;
    //the stellar colour gradient
    [SerializeField] Gradient TemperatureGradient;
    //system loaddata
    protected List<string> SystemDataArray;

    // Start is called before the first frame update
    void Start()
    {
        //clear any previous data
        SystemDataArray = new List<string>();

        //name the root object after the stars unique idenfitier
        StarSearchTerm = gameObject.name;
        
        //recombine the system file directory address
        string SystemFileAddress = Application.streamingAssetsPath 
            + "/Star_Systems/" 
            + (GameObject.Find("Seed_Input").GetComponent<TMP_InputField>().text) 
            + ".system";

        //parse all lines of the system file to a list for reading
        SystemDataArray = File.ReadAllLines(SystemFileAddress).ToList();

        //search each line until unique identifier is found
        for (int i = 0; i < SystemDataArray.Count ; i++) 
        {
            if(SystemDataArray[i] == StarSearchTerm + " {"){

                //get saved body data from system file
                StarName = ReturnFileValue(i+1);
                StarMass = float.Parse(ReturnFileValue(i+2));
                RotationRate = float.Parse(ReturnFileValue(i+3));
                AxialTilt = float.Parse(ReturnFileValue(i+4));
                try {
                    //pass lineread address and file address to orbiter script
                    gameObject.GetComponent<Orbiter>().LoadOrbit(i+6, SystemFileAddress, Parent);
                } catch { }
            }
        }

        ApplyData();   
    }

    string ReturnFileValue(int index)
    {
        int FilePoint = SystemDataArray[index].IndexOf("=");
        return(SystemDataArray[index].Substring(FilePoint+2));
    }

    // Update is called once per frame
    void Update()
    {
        ApplyData();  
        RotateBody(); 
    }

    void RotateBody()
    {
        /*
        //rotation of body
        rotationrate
        float Rate = GameObject.Find("Barycenter").GetComponent<Timekeep>().GameSpeed;
        transform.Rotate(0, (Rate / 300) / (600 * Time.deltaTime), 0);
        */
    }


    // ApplyData is called by UI 
    public void ApplyData()
    {
         //Diameter at formation
        float BaseDiameter = Mathf.Pow(StarMass, 0.7f)*0.8f;
        //Temperature is consistent during the main sequence
        float Temperature = Mathf.Pow(StarMass, 0.5f) * 5780;

        //get general luminosity from size and mass based temperature
        float BaseLuminosity = Mathf.Pow(BaseDiameter, 2) * Mathf.Pow((Temperature/5780), 4);
        //estimate stellar lifespan from diameter divided by luminosity
        float Lifespan = (BaseDiameter / BaseLuminosity) * 9;
        //grab systemage from barycenter
        float SystemAge = GameObject.Find("Game_Controller").GetComponent<SystemGen>().SystemAge;
        //Age Ajustment is % though lifespan
        float AgeAjustment = SystemAge / Lifespan;

        //Overwrite Luminosity and Diameter now  knowing the age
        float Diameter = BaseDiameter + ((BaseDiameter / 2) * AgeAjustment);
        float Luminosity = Mathf.Pow(Diameter, 2) * Mathf.Pow((Temperature/5780), 4);
        
        //set boundaries of various temperature zones
        float CenterLine = Mathf.Sqrt(Luminosity);
        float AridLine = CenterLine * 0.95f;
        float OuterHabitableLine = CenterLine * 1.35f;
        float FrostLine = CenterLine * 4.8f;
        float Heliopause = CenterLine * 75f;

        //produce magnetic values
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
        

        //apply colour
        float ColourPosition = Temperature/12000f;
        Color StellarSurface = TemperatureGradient.Evaluate(ColourPosition);

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
}
