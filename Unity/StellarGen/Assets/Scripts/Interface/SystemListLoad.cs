using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SystemListLoad : MonoBehaviour
{
    public GameObject LoadCellPrefab;
    // Start is called before the first frame update
    void OnEnable()
    {
        //when the systems list is opened
        ResetList();
    }

    void OnDisable()
    {
        //when the systems list is hidden
        ClearList();
    }

    public void ResetList()
    {
        // When a new system is saved or created
        if(gameObject.activeSelf == true) {
            ClearList();
            BuildList();
        }
    }

    void BuildList()
    {
        string systemsPath = $"{Application.streamingAssetsPath}/Star_Systems/";
        DirectoryInfo d = new DirectoryInfo(systemsPath);

        // List area
        RectTransform rt = GetComponent<RectTransform>();
        Vector2 size = rt.sizeDelta;
        size.y = 0f; // set height to 220

        foreach (var File in d.GetFiles("*.json"))
        {
            // Convert file address to string
            string FileAddress = File.ToString();
            // Find position of systems folder and remove text before it
            int FilePoint = FileAddress.IndexOf("Star_Systems");
            string FileName = FileAddress.Substring(FilePoint+13);
            // Spawn the system save container
            GameObject obj = Instantiate(LoadCellPrefab);
            obj.transform.SetParent(this.gameObject.transform, false);
            // Set the loadcellmanager input filename
            obj.GetComponent<LoadCellManager>().Setup(FileName);

            // Extend the list area for each system
            size.y = size.y + 55f; // set height to 220

            // Compensate position based on pivot
            Vector2 pos = rt.anchoredPosition;
            pos.y -= 55f * (0.5f - rt.pivot.y);  // Adjust depending on pivot
            rt.anchoredPosition = pos;
        }

        Logger.Log("System Loader", $"Built Systems List");

        size.y = Math.Max(355f, size.y); // set height to minimum to fill UI
        rt.sizeDelta = size;
    }

    void ClearList()
    {
        // Delete all the load cell containers
        foreach (Transform child in transform) {
            GameObject.Destroy(child.gameObject);
        }
    }

}
