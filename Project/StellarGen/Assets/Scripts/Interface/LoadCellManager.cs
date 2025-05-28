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
    private string systemName;
    protected string systemFileName;
    private readonly string assetsFolder = Application.streamingAssetsPath;
    private string quantities;

    [SerializeField] private Text nameUI;
    [SerializeField] private Text dataUI;

    public void Setup (string systemFileName)
    {
        // Define the file location with the system name
        this.systemFileName = systemFileName;
        string systemFileLocation = $"{assetsFolder}/Star_Systems/{systemFileName}";

        // Get and set readable system name from file definition
        systemName = JsonUtils.ReadJsonValueAtLine(systemFileLocation, 3);
        nameUI.text = systemName;

        this.quantities =
            $"{JsonUtils.ReadJsonValueAtLine(systemFileLocation, 4)} {Settings.LocalisationProvider.GetLocalisedString("#loc_Star_Quantity")}\t|\t " +
            $"{JsonUtils.ReadJsonValueAtLine(systemFileLocation, 5)} {Settings.LocalisationProvider.GetLocalisedString("#loc_Body_Quantity")}\t|\t " +
            $"{JsonUtils.ReadJsonValueAtLine(systemFileLocation, 6)} {Settings.LocalisationProvider.GetLocalisedString("#loc_Belt_Quantity")}";
        dataUI.text = quantities;

        Logger.Log("LoadCellManager", $"Loaded system {systemName} with {quantities}");
    }

    public void LoadName()
    {
        GameObject.Find("Game_Controller").GetComponent<SystemManager>().RecieveSystem(systemFileName, systemName);
    }

    public void DeleteSystem()
    {
        //delete the system files
        File.Delete($"{assetsFolder}/Star_Systems/{systemFileName}");
        //remove the entry from the list
        Destroy(gameObject);
    }
}
