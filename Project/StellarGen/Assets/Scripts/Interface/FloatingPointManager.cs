﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Settings;



public class FloatingPointManager : MonoBehaviour
{
    [SerializeField] [Range(100f, 3000f)] private float Threshold;

    // Start is called before the first frame update
    void Start()
    {
        //set the threshold to the current camera position
        Threshold = GlobalSettings.FloatingPointThreshold;
    }

    void Update()
    {
        Vector3 CameraPosition = this.transform.position;

        //triggers if the distance between the camera and 0,0,0 is exceeds the threshold
        if (CameraPosition.magnitude > Threshold)
        {

            for (int z = 0; z < SceneManager.sceneCount; z++)
            {
                //find every gameobject with no parent
                foreach (GameObject g in SceneManager.GetSceneAt(z).GetRootGameObjects())
                {
                    //move in accordance with the camera position
                    g.transform.position -= CameraPosition;
                }
            }
            //Logger.Log(GetType().Name, "Recentering Origin" );
        }

    }
}