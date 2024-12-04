using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarycentreMass : MonoBehaviour
{
    
    [SerializeField] GameObject Object1;
    [SerializeField] GameObject Object2;
    // Update is called once per frame
    
    void LateUpdate()
    {
        //get masses of two bodies and apply the combined mass
        transform.GetComponent<Rigidbody>().mass = Object1.GetComponent<Rigidbody>().mass + Object2.GetComponent<Rigidbody>().mass;
        
    }
}
