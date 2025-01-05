using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Metaface.Utilities
{
    public class MidEyeGazeOn : MonoBehaviour
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

        // Blink control variables
        private bool isEyeGazeActive = true;
        private float eyeCloseTimer = 0f;
        private float eyeCloseThreshold = 1.5f; // Time threshold for eye close detection
        private bool isEyeClosed = false;

        
        private Vector3 originalScale;

        // MidEyeHelper integration variables
        private MidEyeGazeHelper midEyeHelper;
        public string focusOBJ;

        void Start()
        {
            midRay = midRayOB.GetComponent<LineRenderer>();
            gazeIndicator = transform.GetChild(1).gameObject;

            
            if (gazeIndicator.GetComponent<Renderer>() == null)
            {
                gazeIndicator.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
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

            
            focusOBJ = midEyeHelper.focusOBJ;
        }

        // --- Blink detection logic ---
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

                    
                    gazeIndicator.GetComponent<Renderer>().material.color = Color.yellow;
                    gazeIndicator.transform.localScale = originalScale;
                }
            }
            else
            {
              
                isGazing = false;
                gazeTimer = 0f;

               
                gazeIndicator.GetComponent<Renderer>().material.color = Color.yellow;
                gazeIndicator.transform.localScale = originalScale;
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
    }
}
