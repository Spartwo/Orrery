using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;



public class FloatingPointManager : MonoBehaviour
{
    [SerializeField] [Range(100f, 3000f)] private float Threshold; //0.5AU
    void LateUpdate()
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
            Debug.Log("recentering origin" );
        }

    }
}