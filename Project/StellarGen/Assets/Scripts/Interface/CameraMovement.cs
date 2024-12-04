using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraMovement : MonoBehaviour
{
    //camrotate variables
    private float PanSpeed;
    private float SmoothSpeed = 0.05f;
    private float yaw = 0.0f;
    private float pitch = 0.0f;
    //camtrack variables
    private float LastClickTime; 
    private float DoubleClickTime = 0.3f; 
    public Transform Parent;
    private int ParentIndex;
    private GameObject[] TargettableBodies;
    [SerializeField] private GameObject TimeKeeper;
    // zoom variables
    [SerializeField] private float TargetSize;
    private float RotationGoal;
    private Vector3 PositionGoal;
    private float ZoomGoal;
    private float Zoom;
    [SerializeField] private GameObject CamToMove;

    void Update()
    {
        CamTrack();
        CamInputs();
    }

    public void Start()
    {
        CamToMove = transform.GetChild(0).gameObject;
        UpdateBodyList();
    }
    private void LateUpdate()
    {
        //smooths the movement of the camera
        Zoom = Mathf.Lerp(Zoom, ZoomGoal, SmoothSpeed);
        CamToMove.transform.localPosition = new Vector3(0, 0, Zoom);
    }

    public void UpdateBodyList()
    {      
        //list all bodies with the focusable tag
        TargettableBodies = GameObject.FindGameObjectsWithTag("Focus_Object");
        //set the parent object to first index in targettable objects
        Parent = TargettableBodies[0].transform;
        ParentIndex = 0;
    }

    void CamTrack()
    {
        //Reset the list of cycle objects if voided
        if(!Parent) {
            Debug.Log("Nullskull");
            UpdateBodyList();
        } else {
           transform.SetParent(Parent, false);
        }

        //Prevents the camera from drifting
        PositionGoal = Parent.position;
        float Offset = Vector3.Distance(PositionGoal, transform.position);
        //move to intended angle
        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        //move back towards root object if drifting beyond range
        if (Offset >= 0.01)
        {
            PositionGoal = Vector3.Lerp(transform.position, PositionGoal, SmoothSpeed);
            transform.position = PositionGoal;
        }

        TargetSize = Parent.GetChild(0).transform.lossyScale.z;
        //moves the main cam towards or away from the core object
        ZoomGoal += (CamToMove.transform.localPosition.z)/50 * PanSpeed * -Input.GetAxis("Mouse ScrollWheel");
        
        //clamp the max zoom to within an expected surface based on mass
        ZoomGoal = Mathf.Clamp(ZoomGoal, 1.1f*TargetSize , TargetSize*100000000);
    }

    void CamInputs()
    {
        //pause input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TimeKeeper.GetComponent<Timekeep>().PauseUnpause();
        }
        //hide UI input
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            transform.GetChild(0).GetComponent<Camera>().cullingMask ^= 1 << LayerMask.NameToLayer("UI");
        }
        //quicker control
        if (Input.GetKey(KeyCode.LeftShift))
        {
            PanSpeed = 50f;
        } else {
            PanSpeed = 15f;
        }
        //zoom input keys
        if (Input.GetKey(KeyCode.Minus))
        {
            Debug.Log("Zoom out");
            ZoomGoal += PanSpeed / 75000 * 1 / Time.unscaledDeltaTime;
        }
        if (Input.GetKey(KeyCode.Equals))
        {
            Debug.Log("Zoom in");
            ZoomGoal -= PanSpeed / 75000 * 1 / Time.unscaledDeltaTime;
        }
        //traditional controls
        if (Input.GetKey("d") || Input.GetKey(KeyCode.RightArrow))
        {
            yaw -= PanSpeed / 7500 * 1 / Time.unscaledDeltaTime;
        }
        if (Input.GetKey("a") || Input.GetKey(KeyCode.LeftArrow))
        {
            yaw += PanSpeed / 7500 * 1 / Time.unscaledDeltaTime;
        }
        if (Input.GetKey("w") || Input.GetKey(KeyCode.UpArrow))
        {
            pitch -= PanSpeed / 7500 * 1 / Time.unscaledDeltaTime;
        }
        if (Input.GetKey("s") || Input.GetKey(KeyCode.DownArrow))
        {
            pitch += PanSpeed / 7500 * 1 / Time.unscaledDeltaTime; ;
        }
        //click and drag movement
        if (Input.GetMouseButton(2))
        {
            yaw += PanSpeed * Input.GetAxis("Mouse X");
            pitch += PanSpeed * Input.GetAxis("Mouse Y");
        }
        pitch = Mathf.Clamp(pitch, -90f, 90f);
        
        //Tabbing Between objects
        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            //check if next number is larger than the childcount and loop back if so
            if (ParentIndex == (TargettableBodies.Length-1)) {
                ParentIndex = 0; 
                //reset to start of array
                Parent = TargettableBodies[ParentIndex].GetComponent<Transform>();
            } else {
                ParentIndex += 1;
                Parent = TargettableBodies[ParentIndex].GetComponent<Transform>();
            }
            //try to zoom in towards the body
            ZoomGoal = 1.1f*TargetSize;
        }
        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            //check if next number is less than 0 and loop back if so
            if (ParentIndex == 0) {
                //get array length ajusted for 0 start
                ParentIndex = (TargettableBodies.Length)-1;
                Parent = TargettableBodies[ParentIndex].GetComponent<Transform>();
            } else {
                ParentIndex -= 1;
                Parent = TargettableBodies[ParentIndex].GetComponent<Transform>();
            }
            //try to zoom in towards the body
            ZoomGoal = 1.1f*TargetSize;
        }

        if (Input.GetMouseButtonDown(0))
        {
            //if it's been less than 0.3 seconds between the clicks
            if (Time.realtimeSinceStartup - LastClickTime <= DoubleClickTime)
            {
                //run the click based reparenting script
                DoubleClickReparent();
            } 
            //reset the click timer for the 0.3
            LastClickTime = Time.realtimeSinceStartup;
        }
        //Resets camera Z rotation each frame
        Vector3 eulerRotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y, 0);
    }

    void DoubleClickReparent()
    {
        Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
        RaycastHit hit;
         
        if( Physics.Raycast( ray, out hit ) )
        {
            //if the raycast intercepts the collider of a planet or star
            if (hit.collider.gameObject.tag == "Focus_Object")
            {
                Parent = hit.transform;
                //try to zoom in towards the body
                ZoomGoal = 1.1f*TargetSize;
            }
        }
    }
}

