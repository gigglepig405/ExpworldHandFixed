using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Metaface.Utilities
{
    public class MidEyeGazeRenderer : MonoBehaviour
    {
        [SerializeField]
        OVREyeGaze leftEye;

        [SerializeField]
        OVREyeGaze rightEye;

        [SerializeField]
        GameObject midRayOB;

        [SerializeField]
        private bool showRays = false;

        [SerializeField]
        private float maxGazeDistance = 1000f;

        private LineRenderer midRay;
        GameObject gazeIndicator;

        public float eyeXOffset, eyeYOffset;

        Transform adjustedEyeL, adjustedEyeR;

        // Timer variables
        private float gazeTimer = 0f;
        private bool isGazing = false;
        private float gazeThreshold = 5f; // 5 seconds to trigger blue
        private float blueDuration = 3f; // Blue lasts for 3 seconds
        private bool isBlueActive = false;
        private float blueTimer = 0f;
        private float flashSpeed = 10f; // Flashing speed
        private bool isFlashing = false;
        private Vector3 originalScale;

        void Start()
        {
            midRay = midRayOB.GetComponent<LineRenderer>();
            gazeIndicator = transform.GetChild(1).gameObject;

            // Ensure material is set
            if (gazeIndicator.GetComponent<Renderer>() == null)
            {
                gazeIndicator.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
            }

            // Default color is yellow for browsing
            gazeIndicator.GetComponent<Renderer>().material.color = Color.yellow;

            // Save original scale
            originalScale = gazeIndicator.transform.localScale;
        }

        void Update()
        {
            RaycastHit hitMid;
            UpdateMidEye(RaycastMidEye(leftEye, rightEye, out hitMid, midRay, maxGazeDistance), hitMid);
        }

        private void UpdateMidEye(bool didHit, RaycastHit hit)
        {
            if (didHit)
            {
                transform.GetChild(1).transform.position = hit.point;
                gazeIndicator.transform.position = hit.point;

                // Check if still gazing at the same object
                if (hit.collider != null)
                {
                    isGazing = true;
                    gazeTimer += Time.deltaTime;

                    // If blue is active, count down its timer
                    if (isBlueActive)
                    {
                        blueTimer += Time.deltaTime;

                        if (blueTimer >= blueDuration)
                        {
                            // Reset to yellow after blue duration
                            gazeIndicator.GetComponent<Renderer>().material.color = Color.yellow;
                            gazeIndicator.transform.localScale = originalScale;
                            isBlueActive = false;
                            blueTimer = 0f;
                            gazeTimer = 0f; // Reset gaze timer
                            isFlashing = false;
                        }
                        else if (isFlashing)
                        {
                            // Apply flashing and scaling effect
                            float emission = Mathf.PingPong(Time.time * flashSpeed, 1.0f);
                            Color finalColor = Color.blue * Mathf.LinearToGammaSpace(emission);
                            gazeIndicator.GetComponent<Renderer>().material.color = finalColor;

                            // Apply scaling effect
                            float scaleMultiplier = 1.5f + Mathf.PingPong(Time.time * 0.5f, 0.5f);
                            gazeIndicator.transform.localScale = originalScale * scaleMultiplier;
                        }
                    }
                    else
                    {
                        // Change color to blue and activate flashing if gaze exceeds threshold
                        if (gazeTimer >= gazeThreshold)
                        {
                            isBlueActive = true;
                            blueTimer = 0f; // Start blue timer
                            isFlashing = true; // Start flashing effect
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
                // Reset gaze status and color to browsing (yellow)
                isGazing = false;
                gazeTimer = 0f;
                if (!isBlueActive)
                {
                    gazeIndicator.GetComponent<Renderer>().material.color = Color.yellow;
                    gazeIndicator.transform.localScale = originalScale;
                }
            }
        }

        private bool RaycastMidEye(OVREyeGaze gazeL, OVREyeGaze gazeR, out RaycastHit hit, LineRenderer visualRay, float distance = 1000f)
        {
            adjustedEyeL = gazeL.transform;
            adjustedEyeR = gazeR.transform;

            adjustedEyeL.eulerAngles = gazeL.transform.eulerAngles + new Vector3(eyeXOffset, eyeYOffset, 0f);
            adjustedEyeR.eulerAngles = gazeR.transform.eulerAngles + new Vector3(eyeXOffset, eyeYOffset, 0f);

            if (showRays)
            {
                midRay.SetPositions(new Vector3[] { (adjustedEyeL.transform.position + adjustedEyeR.transform.position) / 2,
                                                   (adjustedEyeL.transform.forward * distance + adjustedEyeR.transform.forward * distance) / 2 });
            }

            return Physics.Raycast((adjustedEyeL.transform.position + adjustedEyeR.transform.position) / 2,
                ((adjustedEyeL.transform.forward * distance + adjustedEyeR.transform.forward * distance) / 2).normalized, out hit, distance);
        }
    }
}
