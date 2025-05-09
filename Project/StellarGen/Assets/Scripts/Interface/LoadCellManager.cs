using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using SystemGen;
using StellarGenHelpers;

public class LoadCellManager : MonoBehaviour
{   
    public string SystemName;
    protected string SystemFileLocation;

    // Start is called before the first frame update
    void Start()
    {
        // Define the file location with the system name
        SystemFileLocation = $"{Application.streamingAssetsPath}/Star_Systems/{SystemName}.json";

        // Get and set readable system name from file definition
        string systemName = JsonUtils.ReadJsonValueAtLine(SystemFileLocation, 3);
        transform.GetChild(0).GetComponent<TextMesh>().text = SystemName;

        string quantities =
            $"{JsonUtils.ReadJsonValueAtLine(SystemFileLocation, 4)} {Settings.LocalisationProvider.GetLocalisedString("#loc_Star_Quantity")}\t|\t " +
            $"{JsonUtils.ReadJsonValueAtLine(SystemFileLocation, 5)} {Settings.LocalisationProvider.GetLocalisedString("#loc_Body_Quantity")}\t|\t " +
            $"{JsonUtils.ReadJsonValueAtLine(SystemFileLocation, 6)} {Settings.LocalisationProvider.GetLocalisedString("#loc_Belt_Quantity")}";
        transform.GetChild(1).GetComponent<TextMesh>().text = quantities;
    }

    public void LoadName()
    {
        // Apply to name
        GameObject.Find("Seed_Input").GetComponent<TextMesh>().text = SystemName;
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
