using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Metaface.Utilities
{
    public class MidEyeGazeRendererNoBallOff : MonoBehaviour
    {
        [SerializeField] OVREyeGaze leftEye;
        [SerializeField] OVREyeGaze rightEye;
        [SerializeField] GameObject midRayOB;
        [SerializeField] private bool showRays = false;
        [SerializeField] private float maxGazeDistance = 1000f;

        private LineRenderer midRay;
        private GameObject gazeIndicator;

        public float eyeXOffset, eyeYOffset;
        private Transform adjustedEyeL, adjustedEyeR;

        // Timer variables
        private float gazeTimer = 0f;
        private bool isGazing = false;
        private float gazeThreshold = 5f; // 5 seconds to trigger blue
        private float blueDuration = 4f;  // Blue lasts for 4 seconds
        private bool isBlueActive = false;
        private float blueTimer = 0f;
        private float flashSpeed = 10f;  // Flashing speed
        private Vector3 originalScale;

        // Statistics variables
        private float totalGazeTime = 0f;
        private int blueTriggerCount = 0;

        // Additional data collection variables
        private Color currentColor = Color.yellow;
        private Vector3 currentScale;

        // File saving variables
        private string path;
        private float logInterval = 1f; // Log data every 1 second
        private float logTimer = 0f;

        void Start()
        {
            // Initialize LineRenderer and indicator
            midRay = midRayOB.GetComponent<LineRenderer>();
            gazeIndicator = transform.GetChild(1).gameObject;

            if (gazeIndicator.GetComponent<Renderer>() == null)
            {
                gazeIndicator.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
            }

            // Default material settings
            var mat = gazeIndicator.GetComponent<Renderer>().material;
            SetMaterialToFadeMode(mat);
            mat.color = new Color(1f, 1f, 0f, 0.3f); // Yellow with transparency

            originalScale = gazeIndicator.transform.localScale;
            currentScale = originalScale;

            // Setup data storage
            string directory = Application.dataPath + "/MidEyeGazeRendererNoBallOff";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            int fileCount = Directory.GetFiles(directory, "GazeNoBallOffSummary_*.csv").Length + 1;
            path = directory + $"/GazeNoBallOffSummary_{fileCount}.csv";

            // Write headers
            File.AppendAllText(path, "Timestamp,TotalGazeTime,IsGazing,FocusPosition,BallColor,BallScale,TriggerCount\n");
        }

        void Update()
        {
            RaycastHit hitMid;
            bool didHit = RaycastMidEye(leftEye, rightEye, out hitMid, midRay, maxGazeDistance);
            UpdateMidEye(didHit, hitMid);

            // Log data every second
            logTimer += Time.deltaTime;
            if (logTimer >= logInterval)
            {
                LogData(); // Write data to CSV
                logTimer = 0f; // Reset timer
            }
        }

        private void UpdateMidEye(bool didHit, RaycastHit hit)
        {
            if (didHit)
            {
                transform.GetChild(1).transform.position = hit.point;
                gazeIndicator.transform.position = hit.point;

                if (!isGazing)
                {
                    isGazing = true;
                }

                // Accumulate gaze time
                gazeTimer += Time.deltaTime;
                totalGazeTime += Time.deltaTime;

                // Trigger blue if gaze exceeds threshold
                if (gazeTimer >= gazeThreshold && !isBlueActive)
                {
                    isBlueActive = true;
                    blueTimer = 0f;
                    blueTriggerCount++;
                }

                // Handle blue flashing state
                if (isBlueActive)
                {
                    blueTimer += Time.deltaTime;

                    if (blueTimer >= blueDuration)
                    {
                        isBlueActive = false;
                        gazeTimer = 0f; // Reset gaze timer
                    }
                }

                // Update indicator
                UpdateIndicator();
            }
            else
            {
                if (isGazing)
                {
                    isGazing = false;
                }
                gazeTimer = 0f;
                ResetIndicator();
            }
        }

        private void UpdateIndicator()
        {
            var mat = gazeIndicator.GetComponent<Renderer>().material;

            if (isBlueActive)
            {
                float emission = Mathf.PingPong(Time.time * flashSpeed, 1.0f);
                Color finalColor = new Color(0f, 0f, 1f, 0.6f * emission); // Blue flashing with transparency
                mat.color = finalColor;
                gazeIndicator.transform.localScale = originalScale * 1.5f; // Enlarged scale
                currentColor = Color.blue;
                currentScale = originalScale * 1.5f;
            }
            else
            {
                mat.color = new Color(1f, 1f, 0f, 0.3f); // Yellow with transparency
                gazeIndicator.transform.localScale = originalScale;
                currentColor = Color.yellow;
                currentScale = originalScale;
            }
        }

        private void ResetIndicator()
        {
            var mat = gazeIndicator.GetComponent<Renderer>().material;
            mat.color = new Color(1f, 1f, 0f, 0.3f); // Reset to yellow with transparency
            gazeIndicator.transform.localScale = originalScale; // Reset scale
        }

        private bool RaycastMidEye(OVREyeGaze gazeL, OVREyeGaze gazeR, out RaycastHit hit, LineRenderer visualRay, float distance = 1000f)
        {
            adjustedEyeL = gazeL.transform;
            adjustedEyeR = gazeR.transform;

            adjustedEyeL.eulerAngles = gazeL.transform.eulerAngles + new Vector3(eyeXOffset, eyeYOffset, 0f);
            adjustedEyeR.eulerAngles = gazeR.transform.eulerAngles + new Vector3(eyeXOffset, eyeYOffset, 0f);

            if (showRays)
            {
                visualRay.SetPositions(new Vector3[] {
                    (adjustedEyeL.transform.position + adjustedEyeR.transform.position) / 2,
                    (adjustedEyeL.forward * distance + adjustedEyeR.forward * distance) / 2
                });
            }

            return Physics.Raycast((adjustedEyeL.transform.position + adjustedEyeR.transform.position) / 2,
                ((adjustedEyeL.forward * distance + adjustedEyeR.forward * distance) / 2).normalized, out hit, distance);
        }

        private void LogData()
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string focusPosition = isGazing ? gazeIndicator.transform.position.ToString("F2") : "None";
            string color = currentColor == Color.yellow ? "Yellow" : "Blue";
            string scale = currentScale.ToString("F2");

            string line = $"{timestamp},{totalGazeTime:F2},{isGazing},{focusPosition},{color},{scale},{blueTriggerCount}\n";
            File.AppendAllText(path, line);
        }

        private void SetMaterialToFadeMode(Material mat)
        {
            mat.SetFloat("_Mode", 2);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
        }
    }
}


