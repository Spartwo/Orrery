using Models;
using StellarGenHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemGen;
using UnityEngine;
using UnityEngine.UI;

namespace Universe
{
    public class OrbitGrid : MonoBehaviour
    {
        // Scaling
        [SerializeField] private SystemManager controller;
        private float orbitScale = 100f;
        private bool useLogScaling = false;
        private float lineScale = 1f;

        private float lastOrbitScale;
        private float lastLineScale;
        private bool lastLogState;
        //private UnityEngine.Vector3 lastPosition;

        // Line render values
        private readonly List<LineRenderer> ringLines = new();
        private readonly List<LineRenderer> radialLines = new();
        [SerializeField][Range(0, 360)] int orbitResolution;
        public Material lineMaterial;
        [SerializeField] Color lineColor;

        private float maxOrbitAU, minOrbitAU;
        private float farthestRingRadius;

        public void Start()
        {
            controller = GameObject.Find("Game_Controller").GetComponent<SystemManager>();
            maxOrbitAU = 1f;
            minOrbitAU = 1f;
        }
        private void Update()
        {
            UpdateScaling();

            // Regenerate the grid if the scaling has changed
            if (orbitScale != lastOrbitScale || lineScale != lastLineScale || useLogScaling != lastLogState)// || transform.position != lastPosition)
            {
                GenerateGrid();
            }
        }

        private void GenerateGrid()
        {
            Logger.Log("UI", $"Building Orbital Grid");
            ClearGrid();

            // Draw concentric rings
            float spacingFactor = 1.2f; // multiplicative AU spacing
            float currentAU = minOrbitAU;

            while (true)
            {
                DrawOrbitRing(currentAU);

                float nextAU = currentAU * spacingFactor;
                if (nextAU > maxOrbitAU)
                    break;

                currentAU = nextAU;
            }

            farthestRingRadius = currentAU;
            // Draw radial lines
            DrawCardinalLines();

            // Lay down distance labels

        }
        private void DrawOrbitRing(float au)
        {
            float radius = PhysicsUtils.GetWorldDistance(au, useLogScaling, orbitScale);

            GameObject ringObject = new GameObject($"OrbitRing_{au:0.00}AU");
            ringObject.transform.SetParent(transform, false);
            ringObject.transform.localPosition = Vector3.zero;
            ringObject.transform.localRotation = Quaternion.Euler(90, 0, 0); // Rotate XY to XZ

            LineRenderer line = ringObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.positionCount = orbitResolution + 1;
            line.alignment = LineAlignment.TransformZ;
            line.loop = true;
            line.material = lineMaterial; 
            line.widthMultiplier = 0.3f * (lineScale * radius)/orbitScale;
            line.startColor = lineColor;
            line.endColor = lineColor;

            for (int i = 0; i <= orbitResolution; i++)
            {
                float angle = (2 * Mathf.PI / orbitResolution) * i;
                float x = Mathf.Cos(angle) * radius;
                float y = Mathf.Sin(angle) * radius;
                line.SetPosition(i, new Vector3(x, y, 0));
            }

            DrawOrbitLabel(au);

            ringLines.Add(line);
        }
        private void DrawOrbitLabel(float au)
        {
            float radius = PhysicsUtils.GetWorldDistance(au, useLogScaling, orbitScale);

            // Scale the text
            float baseScale = 0.05f;
            float scaleFactor = (useLogScaling) ? (Mathf.Log10(au + 1f) / Mathf.Log10(2f)) : au;

            // Cardinal directions with corresponding positions (XZ plane)
            Vector3[] directions = new Vector3[]
            {
                new Vector3(1, 0, 0),  // +X (East)
                new Vector3(0, 0, 1),  // +Z (North)
                new Vector3(-1, 0, 0), // -X (West)
                new Vector3(0, 0, -1), // -Z (South)
            };
            
            // Predefined rotations: (X = 90 to lay flat), Y rotates text tangent to ring at that point
            Vector3[] rotations = new Vector3[]
            {
                new Vector3(90, 0, 0),    // +X: flat + no Y rotation
                new Vector3(90, 90, 0),   // +Z: flat + 90 deg Y rotation
                new Vector3(90, 180, 0),  // -X: flat + 180 deg Y rotation
                new Vector3(90, 270, 0),  // -Z: flat + 270 deg Y rotation
            };

            // Offsets perpendicular to the cardinal direction to move label beside the line
            Vector3[] offsets = new Vector3[]
            {
                new Vector3(0, 0, -(radius * 1.005f - radius) * 2f),   // East: offset in +Z
                new Vector3((radius * 1.005f - radius) * 2f, 0, 0),   // North: offset in +X
                new Vector3(0, 0, (radius * 1.005f - radius) * 2f),   // West: offset in +Z
                new Vector3(-(radius * 1.005f - radius) * 2f, 0, 0),   // South: offset in +X
            };

            for (int i = 0; i < directions.Length; i++)
            {
                Vector3 dir = directions[i];
                Vector3 offset = offsets[i];
                Vector3 rotEuler = rotations[i];

                GameObject labelObj = new GameObject($"Label_{au:0.00}AU_{i}");
                labelObj.transform.SetParent(transform, false);

                Vector3 basePos = dir * radius * 1.005f;
                Vector3 labelPos = basePos + offset;

                labelObj.transform.localPosition = labelPos;

                labelObj.transform.localRotation = Quaternion.Euler(rotEuler);

                TextMesh labelText = labelObj.AddComponent<TextMesh>();
                labelText.text = $"{au:0.00} AU";
                labelText.fontSize = 140;
                labelText.anchor = TextAnchor.UpperLeft;

                Color.RGBToHSV(lineColor, out float h, out float s, out float v);
                s = Mathf.Clamp01(s * 1.9f);
                Color adjustedColor = Color.HSVToRGB(h, s, v);
                adjustedColor.a = lineColor.a;
                labelText.color = adjustedColor;

                labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

                labelObj.transform.localScale = Vector3.one * baseScale * scaleFactor;
            }
        }

        private void DrawCardinalLines()
        {
            float[] angles = { 0f, 90f, 180f, 270f };

            foreach (float angle in angles)
            {
                float rad = Mathf.Deg2Rad * angle;
                float maxRadius = PhysicsUtils.GetWorldDistance(farthestRingRadius, useLogScaling, orbitScale);

                Vector3 direction = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0);
                Vector3 end = direction * maxRadius;

                GameObject lineObject = new GameObject($"RadialLine_{angle}deg");
                lineObject.transform.SetParent(transform, false);
                lineObject.transform.localPosition = Vector3.zero;
                lineObject.transform.localRotation = Quaternion.Euler(90, 0, 0); // same as orbit rings

                LineRenderer line = lineObject.AddComponent<LineRenderer>();
                line.useWorldSpace = false;
                line.positionCount = 2;
                line.alignment = LineAlignment.TransformZ;
                line.material = lineMaterial;
                line.startColor = lineColor;
                line.endColor = lineColor;

                // 🔧 Match orbit ring line width:
                float endWidth = 0.3f * (lineScale * maxRadius) / orbitScale;
                line.widthCurve = new AnimationCurve(
                    new Keyframe(0, 0f),
                    new Keyframe(1, endWidth)
                );

                line.widthMultiplier = 1f;

                line.SetPosition(0, Vector3.zero);
                line.SetPosition(1, end);

                radialLines.Add(line);
            }
        }
        private void ClearGrid()
        {
            lastOrbitScale = orbitScale;
            lastLineScale = lineScale;
            lastLogState = useLogScaling;
            //lastPosition = transform.position;

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            ringLines.Clear();
            radialLines.Clear();
        }

        private void FindOrbitBounds()
        {
            float maxDistance = 1f; // Always include 1 AU as a scale reference
            float minDistance = 1f;

            Debug.Log("au" + minDistance);

            // Check stellar bodies
            foreach (StarProperties star in controller.systemProperties.stellarBodies)
            {
                if (star.Orbit != null && controller.systemProperties.stellarBodies.Count >= 2)
                {
                    decimal semiMajor = star.Orbit.SemiMajorAxis;
                    decimal ecc = (decimal)Mathf.Abs(star.Orbit.Eccentricity);

                    float periapsis = PhysicsUtils.ConvertToAU(semiMajor * (1 - ecc));
                    float apoapsis = PhysicsUtils.ConvertToAU(semiMajor * (1 + ecc));

                    if (apoapsis > maxDistance)
                        maxDistance = apoapsis;
                    if (periapsis < minDistance)
                        minDistance = periapsis;
                }
            }

            Debug.Log("au" + minDistance);
            // Check solid bodies
            foreach (BodyProperties body in controller.systemProperties.solidBodies)
            {
                if (body.Orbit != null && controller.systemProperties.stellarBodies.Any(star => star.SeedValue == body.Parent))
                {
                    decimal semiMajor = body.Orbit.SemiMajorAxis;
                    decimal ecc = (decimal)Mathf.Abs(body.Orbit.Eccentricity);

                    float periapsis = PhysicsUtils.ConvertToAU(semiMajor * (1 - ecc));
                    float apoapsis = PhysicsUtils.ConvertToAU(semiMajor * (1 + ecc));

                    if (apoapsis > maxDistance)
                        maxDistance = apoapsis;
                    if (periapsis < minDistance)
                        minDistance = periapsis;
                }
            }

            Debug.Log("au" + minDistance);
            // Check belts
            foreach (BeltProperties belt in controller.systemProperties.belts)
            {
                if (controller.systemProperties.stellarBodies.Any(star => star.SeedValue == belt.Parent))
                {
                    float upperEdge = PhysicsUtils.ConvertToAU(belt.UpperEdge * (decimal)(1 + Mathf.Abs(belt.Orbit.Eccentricity)));
                    float lowerEdge = PhysicsUtils.ConvertToAU(belt.LowerEdge * (decimal)(1 + Mathf.Abs(belt.Orbit.Eccentricity)));
                    if (upperEdge > maxDistance)
                        maxDistance = upperEdge;
                    if (lowerEdge < minDistance)
                        minDistance = lowerEdge;
                }
            }

            maxOrbitAU = maxDistance;
            minOrbitAU = minDistance;
            Logger.Log("UI", $"Orbit bounds found: Max {maxOrbitAU:0.00} AU, Min {minOrbitAU:0.00} AU");
        }
        public void UpdateMaximum()
        {
            FindOrbitBounds();
            UpdateScaling();
            GenerateGrid();
        }

        public void UpdateScaling()
        {
            orbitScale = controller.orbitScale;
            useLogScaling = controller.useLogScaling;
            lineScale = controller.lineScale;
        }
    }
}
