using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Metaface.Utilities
{
    public class MidEyeGazeRenderer : MonoBehaviour
    {
        [SerializeField] OVREyeGaze leftEye;
        [SerializeField] OVREyeGaze rightEye;
        [SerializeField] GameObject midRayOB;
        [SerializeField] private bool showRays = false;
        [SerializeField] private float maxGazeDistance = 1000f;

        [SerializeField] private bool enableLogging = false;

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

        // Blink variables
        private float eyeCloseTimer = 0f;
        private float eyeCloseThreshold = 1.5f; // Time to consider eyes closed
        private bool isEyeClosed = false;
        private bool isEyeGazeActive = true; // Controls whether gaze is active

        // Statistics variables
        private float totalGazeTime = 0f;
        private int blueTriggerCount = 0;
        private int ballToggleCount = 0;
        private string path;
        private string fileName;

        // Additional data collection variables
        private Color currentColor = Color.yellow;
        private Vector3 currentScale;
        private string focusPosition = "None";

        // File saving variables
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
            string directory = Application.dataPath + "/MidEyeGazeRenderer";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            int fileIndex = 1;
            do
            {
                fileName = $"GazeSummary_{fileIndex}.csv";
                path = Path.Combine(directory, fileName);
                fileIndex++;
            } while (File.Exists(path));

            // Updated CSV header with additional fields
            File.AppendAllText(path, "Timestamp,TotalGazeTime,IsGazing,EyeClosed,FocusPosition,BallColor,BallScale,TriggerCount,BallToggleCount\n");
        }

        void Update()
        {
            DetectEyeBlink();

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

        private void DetectEyeBlink()
        {
            bool isLeftEyeClosed = Vector3.Dot(leftEye.transform.forward, Vector3.down) > 0.85f;
            bool isRightEyeClosed = Vector3.Dot(rightEye.transform.forward, Vector3.down) > 0.85f;
            bool eyesClosed = isLeftEyeClosed && isRightEyeClosed;

            if (eyesClosed)
            {
                eyeCloseTimer += Time.deltaTime;
            }
            else
            {
                eyeCloseTimer = 0f;
            }

            if (eyeCloseTimer >= eyeCloseThreshold && !isEyeClosed)
            {
                isEyeClosed = true;
                ballToggleCount++;

                // Toggle gaze activity
                isEyeGazeActive = !isEyeGazeActive;
                gazeIndicator.SetActive(isEyeGazeActive);

                Log($"Blink detected. isEyeGazeActive: {isEyeGazeActive}, BallToggleCount: {ballToggleCount}");
            }
            else if (!eyesClosed)
            {
                isEyeClosed = false;
            }
        }

        private bool RaycastMidEye(OVREyeGaze gazeL, OVREyeGaze gazeR, out RaycastHit hit, LineRenderer visualRay, float distance = 1000f)
        {
            adjustedEyeL = gazeL.transform;
            adjustedEyeR = gazeR.transform;

            adjustedEyeL.eulerAngles = gazeL.transform.eulerAngles + new Vector3(eyeXOffset, eyeYOffset, 0f);
            adjustedEyeR.eulerAngles = gazeR.transform.eulerAngles + new Vector3(eyeXOffset, eyeYOffset, 0f);

            Vector3 midPosition = (adjustedEyeL.position + adjustedEyeR.position) / 2;
            Vector3 midDirection = ((adjustedEyeL.forward + adjustedEyeR.forward) / 2).normalized;

            if (showRays)
            {
                midRay.SetPositions(new Vector3[] { midPosition, midPosition + midDirection * distance });
            }

            bool didHit = Physics.Raycast(midPosition, midDirection, out hit, distance);

            Log($"Raycast hit: {didHit}, Position: {(didHit ? hit.point.ToString("F2") : "N/A")}");

            return didHit;
        }

        private void UpdateMidEye(bool didHit, RaycastHit hit)
        {
            if (didHit && isEyeGazeActive)
            {
                transform.GetChild(1).transform.position = hit.point;
                gazeIndicator.transform.position = hit.point;
                focusPosition = hit.point.ToString("F2");

                if (!isGazing)
                {
                    isGazing = true;
                    Log("Gazing started.");
                }

                gazeTimer += Time.deltaTime;
                totalGazeTime += Time.deltaTime;

                Log($"Gaze Timer: {gazeTimer}, Total Gaze Time: {totalGazeTime}");

                if (gazeTimer >= gazeThreshold && !isBlueActive)
                {
                    isBlueActive = true;
                    blueTimer = 0f;
                    blueTriggerCount++;

                    Log($"Blue state activated. blueTriggerCount: {blueTriggerCount}");
                }

                if (isBlueActive)
                {
                    blueTimer += Time.deltaTime;

                    if (blueTimer >= blueDuration)
                    {
                        isBlueActive = false;
                        gazeTimer = 0f;

                        Log($"Blue state deactivated.");
                    }
                }

                UpdateIndicator();
            }
            else
            {
                if (isGazing)
                {
                    isGazing = false;
                    Log("Gazing stopped.");
                }
                gazeTimer = 0f;
                ResetIndicator();
                focusPosition = "None";
            }
        }

        private void UpdateIndicator()
        {
            var mat = gazeIndicator.GetComponent<Renderer>().material;

            if (isBlueActive)
            {
                currentColor = Color.blue;
                currentScale = originalScale * 1.5f;
            }
            else
            {
                currentColor = Color.yellow;
                currentScale = originalScale;
            }

            mat.color = currentColor;
            gazeIndicator.transform.localScale = currentScale;
        }

        private void ResetIndicator()
        {
            currentColor = Color.yellow;
            currentScale = originalScale;

            var mat = gazeIndicator.GetComponent<Renderer>().material;
            mat.color = currentColor;
            gazeIndicator.transform.localScale = currentScale;
        }

        private void LogData()
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string ballColorString = GetBallColorString();
                string eyeClosedStatus = !isEyeGazeActive ? "True" : "False"; // Reflect gaze active state
                string formattedScale = FormatVector3(currentScale);

                string line = $"{timestamp},{totalGazeTime:F2},{isGazing},{eyeClosedStatus},{focusPosition},{ballColorString},{formattedScale},{blueTriggerCount},{ballToggleCount}\n";
                File.AppendAllText(path, line);
            }
            catch (Exception ex)
            {
                LogError($"Failed to log data: {ex.Message}");
            }
        }

        private string GetBallColorString()
        {
            if (currentColor == Color.blue)
                return "Blue";
            else if (currentColor == Color.yellow)
                return "Yellow";
            else
                return "Unknown";
        }

        private string FormatVector3(Vector3 vector)
        {
            return $"({vector.x:F2}, {vector.y:F2}, {vector.z:F2})";
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

        private void Log(string message)
        {
            if (enableLogging)
            {
                UnityEngine.Debug.Log(message);
            }
        }

        private void LogError(string message)
        {
            if (enableLogging)
            {
                UnityEngine.Debug.LogError(message);
            }
        }
    }
}























