using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Linq;

public class BodyManager : MonoBehaviour
{
    //public knowledge variables that will be accessed by UI
    [HideInInspector] public string BodySearchTerm;
    [SerializeField] float BodyMass;
    [SerializeField] float AxialTilt;
    public string ParentObject;
    public string BodyName;
    [SerializeField] [Range(0f, 70f)]   float RotationRate;
    [HideInInspector]public string SystemFileName;
    //system loaddata
    protected List<string> SystemDataArray;
    // Start is called before the first frame update
    void Start()
    {
        //name the root object after the stars unique idenfitier
        gameObject.name = BodySearchTerm;
        
        //recombine the system file directory address
        string SystemFileAddress = Application.streamingAssetsPath 
            + "/Star_Systems/" 
            + SystemFileName
            + ".system";

        //parse all lines of the system file to a list for reading
        SystemDataArray = File.ReadAllLines(SystemFileAddress).ToList();

        //search each line until unique identifier is found
        for (int i = 0; i < SystemDataArray.Count ; i++) 
        {
            if(SystemDataArray[i] == BodySearchTerm + " {"){

                //get saved body data from system file
                BodyName = ReturnFileValue(i+1);
                BodyMass = float.Parse(ReturnFileValue(i+2));
                ParentObject = ReturnFileValue(i+3);
                //set the parent gameobject
                GameObject ParentBody = GameObject.Find(ParentObject);
                transform.SetParent(ParentBody.transform, true);

                RotationRate = float.Parse(ReturnFileValue(i+4));
                AxialTilt = float.Parse(ReturnFileValue(i+5));
                //pass lineread address and file address to orbiter script
                transform.gameObject.GetComponent<Orbiter>().LoadOrbit(i+6, SystemFileAddress, ParentBody.transform);
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
        
        //Volume Displayed in Cubic km
        float Diameter = 1*BodyMass;


        //set size of the star itself relative to earth=1
        transform.GetChild(0).localScale = new Vector3(Diameter/100, Diameter/100, Diameter/100);
        //set size of double click collider
        transform.GetComponent<SphereCollider>().radius = Diameter;
        //All bodies are weighed where 1 = Earth
        float MassInEarth = BodyMass;
        //get rigidbody and apply the mass
        transform.GetComponent<Rigidbody>().mass = MassInEarth/10000;
    }
}
