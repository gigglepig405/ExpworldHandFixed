using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Metaface.Utilities
{
    public class MidEyeGazeOn : MonoBehaviour
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

        // Blink control variables
        private bool isEyeGazeActive = true;
        private float eyeCloseTimer = 0f;
        private float eyeCloseThreshold = 1.5f; // Time threshold for eye close detection
        private bool isEyeClosed = false;

        private Vector3 originalScale;

        // Statistics variables
        private string path;
        private float totalGazeTime = 0f;
        private int triggerCount = 0;
        private int ballToggleCount = 0;
        private Color currentColor = Color.yellow;
        private Vector3 currentScale;

        // MidEyeHelper integration variables
        private MidEyeGazeHelper midEyeHelper;
        public string focusOBJ;

        void Start()
        {
            // Initialize components
            midRay = midRayOB.GetComponent<LineRenderer>();
            gazeIndicator = transform.GetChild(1).gameObject;

            if (gazeIndicator.GetComponent<Renderer>() == null)
            {
                gazeIndicator.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
            }

            // Set initial properties
            gazeIndicator.GetComponent<Renderer>().material.color = Color.yellow;
            originalScale = gazeIndicator.transform.localScale;
            currentScale = originalScale;
            midEyeHelper = GetComponent<MidEyeGazeHelper>();

            // Setup data storage
            string directory = Application.dataPath + "/MidEyeGazeOn";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            path = directory + $"/GazeOnSummary_{timestamp}.csv";

            File.AppendAllText(path, "Timestamp,TotalGazeTime,IsGazing,EyeClosed,FocusPosition,BallColor,BallScale,TriggerCount,BallToggleCount\n");
        }

        void Update()
        {
            // Eye gaze processing
            RaycastHit hitMid;
            bool didHit = RaycastMidEye(leftEye, rightEye, out hitMid, midRay, maxGazeDistance);
            UpdateMidEye(didHit, hitMid);
            focusOBJ = midEyeHelper.focusOBJ;

            // Log data continuously
            LogData();

            // Blink detection for toggling ball
            DetectEyeBlink();
        }

        private void UpdateMidEye(bool didHit, RaycastHit hit)
        {
            if (didHit)
            {
                // Update position
                transform.GetChild(1).transform.position = hit.point;
                gazeIndicator.transform.position = hit.point;

                // Update gaze status
                if (!isGazing)
                {
                    isGazing = true;
                    triggerCount++;
                }

                // Accumulate gaze time
                gazeTimer += Time.deltaTime;
                totalGazeTime += Time.deltaTime;
            }
            else
            {
                isGazing = false;
                gazeTimer = 0f;
            }

            // Maintain yellow color and original size
            gazeIndicator.GetComponent<Renderer>().material.color = Color.yellow;
            gazeIndicator.transform.localScale = originalScale;
            currentColor = Color.yellow;
            currentScale = originalScale;
        }

        private bool RaycastMidEye(OVREyeGaze gazeL, OVREyeGaze gazeR, out RaycastHit hit, LineRenderer visualRay, float distance = 1000f)
        {
            adjustedEyeL = gazeL.transform;
            adjustedEyeR = gazeR.transform;

            adjustedEyeL.eulerAngles = gazeL.transform.eulerAngles + new Vector3(eyeXOffset, eyeYOffset, 0f);
            adjustedEyeR.eulerAngles = gazeR.transform.eulerAngles + new Vector3(eyeXOffset, eyeYOffset, 0f);

            if (showRays)
            {
                visualRay.SetPositions(new Vector3[]
                {
                    (adjustedEyeL.transform.position + adjustedEyeR.transform.position) / 2,
                    (adjustedEyeL.forward * distance + adjustedEyeR.forward * distance) / 2
                });
            }

            return Physics.Raycast(
                (adjustedEyeL.transform.position + adjustedEyeR.transform.position) / 2,
                ((adjustedEyeL.forward * distance + adjustedEyeR.forward * distance) / 2).normalized,
                out hit,
                distance
            );
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
                isEyeGazeActive = !isEyeGazeActive;
                gazeIndicator.SetActive(isEyeGazeActive);
                isEyeClosed = true;
                ballToggleCount++;
            }
            else if (!eyesClosed)
            {
                isEyeClosed = false;
            }
        }

        private void LogData()
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string focusPosition = isGazing ? gazeIndicator.transform.position.ToString("F2") : "None";
            string color = currentColor == Color.yellow ? "Yellow" : "Blue";
            string scale = currentScale.ToString("F2");

            string line = $"{timestamp},{totalGazeTime:F2},{isGazing},{isEyeClosed},{focusPosition},{color},{scale},{triggerCount},{ballToggleCount}\n";
            File.AppendAllText(path, line);
        }
    }
}


