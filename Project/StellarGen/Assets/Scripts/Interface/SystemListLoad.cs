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
        //when a new system is saved or created
        if(gameObject.activeSelf == true) {
            ClearList();
            BuildList();
        }
       
    }

    void BuildList()
    {
        string SystemsPath = Application.streamingAssetsPath + "/Star_Systems/";
        DirectoryInfo d = new DirectoryInfo(SystemsPath);
        foreach (var File in d.GetFiles("*.system"))
        {
            //convert file address to string
            string FileAddress = File.ToString();
            //find position of systems folder and remove text before it
            int FilePoint = FileAddress.IndexOf("Star_Systems");
            string FileName = FileAddress.Substring(FilePoint+13);
            //spawn the system save container
            GameObject obj = Instantiate(LoadCellPrefab);
            obj.transform.SetParent(this.gameObject.transform, false);
            //set the loadcellmanager input filename
            obj.GetComponent<LoadCellManager>().SystemName = FileName;
        }
    }

    void ClearList()
    {
        //delete all the load cell containers
        foreach (Transform child in transform) {
            GameObject.Destroy(child.gameObject);
        }
    }

}
