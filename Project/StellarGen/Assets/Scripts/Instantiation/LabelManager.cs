using StellarGenHelpers;
using System;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UI;

namespace SystemGen
{
	[RequireComponent(typeof(Renderer))]
	public class LabelManager : MonoBehaviour
    {
        [SerializeField] private StarManager starManager; // assign if this is a star
        [SerializeField] private BodyManager bodyManager; // assign if this is a body

        public float baseLabelScale = 0.1f; // base scale for label
        [SerializeField] private Camera mainCamera;

        private GameObject labelObject;
        private TextMesh labelTextMesh;

		void Start()
		{
            // Find main camera if not assigned
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            SetManagers();

            // Create label GameObject
            labelObject = new GameObject("Label");
            labelObject.transform.SetParent(transform, false);

            labelTextMesh = labelObject.AddComponent<TextMesh>();
            labelTextMesh.anchor = TextAnchor.MiddleLeft;
            labelTextMesh.alignment = TextAlignment.Left;
            labelTextMesh.fontSize = 50;
            labelTextMesh.characterSize = 0.025f;
            labelTextMesh.color = Color.white; 
            // Using default font (no explicit font assignment)
        }

        void Update()
        {
            // Check to see if this body is the camera target or a child of the camera target
            GameObject cameraTarget = mainCamera.transform.parent.parent.gameObject;
            bool isTargetOrChild = false;

            if (gameObject == cameraTarget)
                isTargetOrChild = true;
            else if (transform.parent != null && transform.parent.gameObject == cameraTarget)
                isTargetOrChild = true;

            // Hide label if not target or direct child
            if (!isTargetOrChild)
            {
                labelTextMesh.text = ""; 
                return;
            }

            // Update label text dynamically (in case name changes)
            labelTextMesh.text = "\t" + GetLabelText();

            // Update label color to match the line
            labelTextMesh.color = GetLabelColor();

            // Position offset scaled by distance to camera (to keep it offset proportionally)
            float distanceToCamera = Vector3.Distance(mainCamera.transform.position, transform.position);
            labelObject.transform.localPosition = Vector3.zero;

            // Make label face camera
            labelObject.transform.rotation = Quaternion.LookRotation(labelObject.transform.position - mainCamera.transform.position);

            // Scale label so it appears constant size on screen
            float scale = distanceToCamera * baseLabelScale;
            labelObject.transform.localScale = Vector3.one * scale;
        }
        private string GetLabelText()
        {
            try
            {
                if (starManager != null && starManager.star != null)
                {
                    return starManager.star.Name;
                }
                if (bodyManager != null && bodyManager.body != null)
                {
                    return bodyManager.body.Name;
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Orbital Labels", $"Error getting label text: {e.Message}");
            }
            return "Unnamed";
        }

        private Color GetLabelColor()
        {
            if (starManager != null && starManager.star != null)
            {
                // Assuming star.Colour or similar exists as Color or float array
                return starManager.star.OrbitLine != null ? ColourUtils.ArrayToColor(starManager.star.OrbitLine) : Color.white;
            }
            else if (bodyManager != null && bodyManager.body != null)
            {
                return bodyManager.body.OrbitLine != null ? ColourUtils.ArrayToColor(bodyManager.body.OrbitLine) : Color.white;
            }
            return Color.white;
        }

        private void SetManagers()
        {
            // Try to find StarManager or BodyManager in parent hierarchy
            starManager = GetComponent<StarManager>();
            bodyManager = GetComponent<BodyManager>();
            

            if (starManager == null && bodyManager == null)
            {
                Debug.LogWarning("No StarManager or BodyManager found in parent hierarchy.");
            }
        }
    }
}