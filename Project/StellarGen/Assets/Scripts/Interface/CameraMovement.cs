using Models;
using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SystemGen;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Universe
{ 
    public class CameraMovement : MonoBehaviour
    {
        // CamRotate variables
        private float panSpeed;
        private float smoothSpeed = 0.05f;
        private float yaw = 0.0f;
        private float pitch = 0.0f;
        // CamTrack variables
        private float lastClickTime; 
        private float doubleClickTime = 0.3f;
        [SerializeField] public Transform parent;
        [SerializeField] private int parentIndex;
        [SerializeField] private List <Transform> targetableBodies;
        [SerializeField] private Timekeep timeKeeper;
        // CamZoom variables
        [SerializeField] private float targetSize;
        private Vector3 positionGoal;
        private float zoomGoal;
        private float zoomCurrent;
        [SerializeField] private GameObject CamToMove;

        private bool activeUI = true;

        void Update()
        {
            CamTrack();
            CamInputs();
        }

        public void Start()
        {
            timeKeeper = GameObject.Find("Game_Controller").GetComponent<Timekeep>();
            CamToMove = transform.GetChild(0).gameObject;
            UpdateBodyList();
        }
        private void LateUpdate()
        {
            //smooths the movement of the camera
            zoomCurrent = Mathf.Lerp(zoomCurrent, zoomGoal, smoothSpeed);
            CamToMove.transform.localPosition = new Vector3(0, 0, zoomCurrent);
        }

        public void UpdateSelectedBody(Transform target)
        {
            // Set the parent object to the selected body
            parent = target.transform;
            // Get info to send
            string sentInfo = "NaN";
            if (target.GetComponent<StarManager>() != null)
            {
                parent = target.transform;
                sentInfo = target.GetComponent<StarManager>().star.GetInfo();
            }
            else if (target.GetComponent<BodyManager>() != null)
            {
                parent = target.transform; 
                sentInfo = target.GetComponent<BodyManager>().body.GetInfo();
            }
            else if (target.GetComponent<BeltManager>() != null)
            {
                parent = target.transform;
                sentInfo = target.GetComponent<BeltManager>().belt.GetInfo();
            }
            else
            {
                parent = target.transform;
            }

            transform.SetParent(parent, false);
            GameObject.Find("Game_Controller").GetComponent<SystemManager>().SetInfoBox(sentInfo);

            // Try to zoom in towards the body
            zoomGoal = Math.Max(zoomCurrent, 0.05f * targetSize);
        }

        public void UpdateBodyList()
        {
            // Store a list all bodies with the orbit handlet
            targetableBodies = Object.FindObjectsByType<OrbitManager>(FindObjectsSortMode.None)
                         .Select(o => o.transform)
                         .ToList(); 

            Logger.Log("Camera Controller", $"Found {targetableBodies.Count} targetable bodies");
            //targetableBodies = GameObject.FindGameObjectsWithTag("Focus_Object");
            //set the parent object to first index in targettable objects
            //parent = targetableBodies[0].transform;
            parentIndex = 0;
        }

        void CamTrack()
        {
            // Reset the list of cycle objects if voided
            if(!parent) {
                Logger.Log(GetType().Name, "No Camera Parent Found");
                UpdateBodyList();
            } else {
               transform.SetParent(parent, false);
            }

            // Prevents the camera from drifting
            positionGoal = parent.position;
            float offset = Vector3.Distance(positionGoal, transform.position);
            // Move to intended angle
            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
            // Move back towards root object if drifting beyond range
            if (offset >= 0.01)
            {
                positionGoal = Vector3.Lerp(transform.position, positionGoal, smoothSpeed);
                transform.position = positionGoal;
            }

            targetSize = parent.GetChild(0).transform.lossyScale.z;
            // Moves the main cam towards or away from the core object
            zoomGoal += (CamToMove.transform.localPosition.z)/50 * panSpeed * -Input.GetAxis("Mouse ScrollWheel");
        
            // Clamp the max zoom to within an expected surface based on mass
            zoomGoal = Mathf.Clamp(zoomGoal, 0.05f*targetSize , targetSize*50000);
        }

        void CamInputs()
        {
            // Pause input
            if (Input.GetKeyDown(KeyCode.Space))
            {
                timeKeeper.PauseUnpause();
            }
            // Hide UI input
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                activeUI = !activeUI;
                // Toggle the worldspace UI
                int uiLayer = 1 << LayerMask.NameToLayer("UI");
                Camera cam = transform.GetChild(0).GetComponent<Camera>();
                if (activeUI)
                {
                    cam.cullingMask |= uiLayer;  // Enable UI layer
                }
                else
                {
                    cam.cullingMask &= ~uiLayer; // Disable UI layer
                }
                // Toggle the screenspace UI
                GameObject.Find("UI_Canvas").GetComponent<Canvas>().enabled = activeUI;

                Logger.Log("Camera Controller", $"Toggled UI to {activeUI.ToString()}");
            }
            // Quicker control
            if (Input.GetKey(KeyCode.LeftShift))
            {
                panSpeed = 50f;
            } else {
                panSpeed = 15f;
            }
            // Zoom input keys
            if (Input.GetKey(KeyCode.Minus))
            {
                Logger.Log(GetType().Name, "Zoom out");
                zoomGoal += panSpeed / 75000 * 1 / Time.unscaledDeltaTime;
            }
            if (Input.GetKey(KeyCode.Equals))
            {
                Logger.Log(GetType().Name, "Zoom in");
                zoomGoal -= panSpeed / 75000 * 1 / Time.unscaledDeltaTime;
            }
            // traditional controls
            if (Input.GetKey("d") || Input.GetKey(KeyCode.RightArrow))
            {
                yaw -= panSpeed / 7500 * 1 / Time.unscaledDeltaTime;
            }
            if (Input.GetKey("a") || Input.GetKey(KeyCode.LeftArrow))
            {
                yaw += panSpeed / 7500 * 1 / Time.unscaledDeltaTime;
            }
            if (Input.GetKey("w") || Input.GetKey(KeyCode.UpArrow))
            {
                pitch -= panSpeed / 7500 * 1 / Time.unscaledDeltaTime;
            }
            if (Input.GetKey("s") || Input.GetKey(KeyCode.DownArrow))
            {
                pitch += panSpeed / 7500 * 1 / Time.unscaledDeltaTime; ;
            }
            // Click and drag movement
            if (Input.GetMouseButton(2))
            {
                yaw += panSpeed * Input.GetAxis("Mouse X");
                pitch += panSpeed * Input.GetAxis("Mouse Y");
            }
            pitch = Mathf.Clamp(pitch, -90f, 90f);
        
            // Tabbing Between objects
            if (Input.GetKeyDown(KeyCode.RightBracket))
            {
                //check if next number is larger than the childcount and loop back if so
                if (parentIndex == (targetableBodies.Count)) {
                    parentIndex = 0;
                    //reset to start of array
                    UpdateSelectedBody(targetableBodies[parentIndex]);
                } else {
                    parentIndex += 1;
                    UpdateSelectedBody(targetableBodies[parentIndex]);
                }
            }
            if (Input.GetKeyDown(KeyCode.LeftBracket))
            {
                //check if next number is less than 0 and loop back if so
                if (parentIndex == 0) {
                    //get array length ajusted for 0 start
                    parentIndex = (targetableBodies.Count)-1;
                    UpdateSelectedBody(targetableBodies[parentIndex]);
                } else {
                    parentIndex -= 1;
                    UpdateSelectedBody(targetableBodies[parentIndex]);
                }
                Logger.Log("Camera Controller", $"Tab Reparented to {parentIndex}");
            }

            if (Input.GetMouseButtonDown(0))
            {
                // If it's been less than 0.3 seconds between the clicks
                if (Time.realtimeSinceStartup - lastClickTime <= doubleClickTime)
                {
                    // Run the click based reparenting script
                    DoubleClickReparent();
                } 
                // Reset the click timer for the 0.3
                lastClickTime = Time.realtimeSinceStartup;
            }
            // Resets camera Z rotation each frame
            Vector3 eulerRotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y, 0);
        }

        void DoubleClickReparent()
        {
            Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
            RaycastHit hit;
         
            if ( Physics.Raycast( ray, out hit ) )
            {
                // If the raycast intercepts the collider of a planet or star
                if (hit.collider.gameObject.tag == "Focus_Object")
                {
                    Logger.Log("Camera Controller", $"Double Click Reparented from {parent} to {hit.collider.gameObject.name}");
                    UpdateSelectedBody(hit.collider.transform);
                }
            }
        }
    }
}
