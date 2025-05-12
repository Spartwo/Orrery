using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using Models;
using Universe;
using StellarGenHelpers;

namespace SystemGen
{
    public class BeltManager : MonoBehaviour
    {
        // Public knowledge variables that will be accessed by UI
        [SerializeField] public BeltProperties belt;
        public GameObject parentObject;
        public string bodyName;

        // Start is called before the first frame update
        void Start()
        {
            ApplyData();
        }

        public void FindParent()
        {
            // Find the parent object of the body
            if (belt.Parent != 0)
            {
                int parentID = belt.Parent;
                // Find the parent object by its ID
                GameObject parentObject = GameObject.Find(parentID.ToString());

                if (parentObject != null)
                {
                    transform.SetParent(parentObject.transform, false);
                    transform.GetComponent<OrbitManager>().LoadOrbit(belt.Orbit, transform.parent, belt.OrbitLine);
                }
            }
        }

        // ApplyData is called by UI 
        public void ApplyData()
        {
            // Pame the root object after the stars unique idenfitier
            gameObject.name = belt.SeedValue.ToString();

            float beltLine = PhysicsUtils.ConvertToAU(belt.Orbit.SemiMajorAxis) * 250;

            // Set radiation zone bounds
            transform.GetChild(1).GetChild(0).localScale = new Vector3(beltLine, beltLine, beltLine);

            float massInEarth = PhysicsUtils.RawToEarthMass(belt.Mass);
            // Get rigidbody and apply the mass
            transform.GetComponent<Rigidbody>().mass = massInEarth / 10000;
        }
    }
}