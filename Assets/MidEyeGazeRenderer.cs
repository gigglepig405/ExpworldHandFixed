using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Metaface.Utilities
{
    public class MidEyeGazeRenderer : MonoBehaviour
    {
        [SerializeField] private OVREyeGaze leftEye;
        [SerializeField] private OVREyeGaze rightEye;
        [SerializeField] private GameObject midRayOB;
        [SerializeField] private bool showRays = false;
        [SerializeField] private float maxGazeDistance = 1000f;

        private LineRenderer midRay;
        private GameObject gazeIndicator;

        public float eyeXOffset, eyeYOffset;

        private Transform adjustedEyeL, adjustedEyeR;

        private float gazeTimer = 0f;
        private bool isGazing = false;
        private float gazeThreshold = 5f;
        private float blueDuration = 3f;
        private bool isBlueActive = false;
        private float blueTimer = 0f;
        private float flashSpeed = 10f;
        private bool isFlashing = false;
        private Vector3 originalScale;

        private bool isEyeGazeActive = true;
        private float eyeCloseTimer = 0f;
        private float eyeCloseThreshold = 1.5f;
        private bool isEyeClosed = false;

        private MidEyeGazeHelper midEyeHelper;
        public string focusOBJ;

        private float totalGazeTime = 0f;
        private int blueTriggerCount = 0;

        void Start()
        {
            midRay = midRayOB.GetComponent<LineRenderer>();
            gazeIndicator = transform.GetChild(1).gameObject;

            if (gazeIndicator.GetComponent<Renderer>() == null)
            {
                var renderer = gazeIndicator.AddComponent<MeshRenderer>();
                renderer.material = new Material(Shader.Find("Standard"));
            }

            gazeIndicator.GetComponent<Renderer>().material.color = Color.yellow;
            originalScale = gazeIndicator.transform.localScale;

            midEyeHelper = GetComponent<MidEyeGazeHelper>();
        }

        void Update()
        {
            DetectEyeBlink();
            if (!isEyeGazeActive) return;

            RaycastHit hitMid;
            bool didHit = RaycastMidEye(leftEye, rightEye, out hitMid, midRay, maxGazeDistance);
            UpdateMidEye(didHit, hitMid);

            if (midEyeHelper != null)
            {
                focusOBJ = midEyeHelper.focusOBJ;
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
                isEyeGazeActive = !isEyeGazeActive;
                gazeIndicator.SetActive(isEyeGazeActive);
                isEyeClosed = true;
            }
            else if (!eyesClosed)
            {
                isEyeClosed = false;
            }
        }

        private void UpdateMidEye(bool didHit, RaycastHit hit)
        {
            if (didHit)
            {
                transform.GetChild(1).transform.position = hit.point;
                gazeIndicator.transform.position = hit.point;

                if (hit.collider != null)
                {
                    isGazing = true;
                    gazeTimer += Time.deltaTime;

                    totalGazeTime += Time.deltaTime;

                    if (isBlueActive)
                    {
                        blueTimer += Time.deltaTime;
                        if (blueTimer >= blueDuration)
                        {
                            gazeIndicator.GetComponent<Renderer>().material.color = Color.yellow;
                            gazeIndicator.transform.localScale = originalScale;
                            isBlueActive = false;
                            blueTimer = 0f;
                            gazeTimer = 0f;
                            isFlashing = false;
                        }
                        else if (isFlashing)
                        {
                            float emission = Mathf.PingPong(Time.time * flashSpeed, 1.0f);
                            Color finalColor = Color.blue * Mathf.LinearToGammaSpace(emission);
                            gazeIndicator.GetComponent<Renderer>().material.color = finalColor;

                            float scaleMultiplier = 1.5f + Mathf.PingPong(Time.time * 0.5f, 0.5f);
                            gazeIndicator.transform.localScale = originalScale * scaleMultiplier;
                        }
                    }
                    else
                    {
                        if (gazeTimer >= gazeThreshold)
                        {
                            isBlueActive = true;
                            blueTimer = 0f;
                            isFlashing = true;
                            blueTriggerCount++;
                        }
                        else
                        {
                            gazeIndicator.GetComponent<Renderer>().material.color = Color.yellow;
                            gazeIndicator.transform.localScale = originalScale;
                        }
                    }
                }
            }
            else
            {
                isGazing = false;
                gazeTimer = 0f;
                if (!isBlueActive)
                {
                    gazeIndicator.GetComponent<Renderer>().material.color = Color.yellow;
                    gazeIndicator.transform.localScale = originalScale;
                }
            }
        }

        private bool RaycastMidEye(OVREyeGaze gazeL, OVREyeGaze gazeR, out RaycastHit hit,
                                   LineRenderer visualRay, float distance = 1000f)
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
                    (adjustedEyeL.transform.forward * distance + adjustedEyeR.transform.forward * distance) / 2
                });
            }

            return Physics.Raycast(
                (adjustedEyeL.transform.position + adjustedEyeR.transform.position) / 2,
                ((adjustedEyeL.transform.forward * distance + adjustedEyeR.transform.forward * distance) / 2).normalized,
                out hit,
                distance
            );
        }

        void OnApplicationQuit()
        {
            SaveDataToCsv();
        }

        private void SaveDataToCsv()
        {
            // This path points to "Assets/MidEyeGazeRenderer/gaze_data.csv"
            // Make sure "MidEyeGazeRenderer" folder exists under Assets
            string path = Application.dataPath + "/MidEyeGazeRenderer/gaze_data.csv";

            // Append one line: "totalGazeTime,blueTriggerCount"
            // e.g. "12.34,3"
            string line = totalGazeTime + "," + blueTriggerCount + "\n";
            File.AppendAllText(path, line);

            UnityEngine.Debug.Log("[MidEyeGazeRenderer] Data appended to CSV at: " + path);
        }
    }
}










