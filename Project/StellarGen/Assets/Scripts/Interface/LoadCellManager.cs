using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Linq;

public class LoadCellManager : MonoBehaviour
{   
    public string SystemName;
    protected string SystemFileLocation;

    // Start is called before the first frame update
    void Start()
    {
        //define the file location with the system name
        SystemFileLocation = Application.streamingAssetsPath + "/Star_Systems/" + SystemName;
        //Get and set readable system name from file definition
        SystemName = SystemName.Substring(0, SystemName.Length - 7);
        transform.GetChild(0).GetComponent<TMP_Text>().text = SystemName;

        //get file star count
        string StarsValue = File.ReadLines(SystemFileLocation).Skip(1).Take(1).First();
        StarsValue = StarsValue.Substring(8);
        //add to a string and cast to box
        string Stars = "Stars | " + StarsValue;
        transform.GetChild(1).GetComponent<TMP_Text>().text = Stars;

        //get file planet count 
        string PlanetsValue = File.ReadLines(SystemFileLocation).Skip(2).Take(1).First();
        PlanetsValue = PlanetsValue.Substring(9);
        //add to a string and cast to box
        string Planets = "Bodies | " + PlanetsValue;
        transform.GetChild(2).GetComponent<TMP_Text>().text = Planets;
    }

    public void LoadName()
    {
        //apply to name
        GameObject.Find("Seed_Input").GetComponent<TMP_InputField>().text = SystemName;
    }

    public void DeleteSystem()
    {
        //delete the system files
        File.Delete (SystemFileLocation);
        File.Delete (SystemFileLocation + ".meta");
        //remove the entry from the list
        Destroy(gameObject);
    }
}
