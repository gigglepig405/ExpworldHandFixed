using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Metaface.Utilities
{
    public class MidEyeGazeML : MonoBehaviour
    {
        [SerializeField]
        private OVREyeGaze leftEye;

        [SerializeField]
        private OVREyeGaze rightEye;

        [SerializeField]
        private GameObject midRayOB;

        [SerializeField]
        private bool showRays = false;

        [SerializeField]
        private float maxGazeDistance = 1000f;

        private LineRenderer midRay;
        private GameObject gazeIndicator;

        public float eyeXOffset, eyeYOffset;

        private Transform adjustedEyeL, adjustedEyeR;

        // Blink detection
        private bool isEyeGazeActive = true;
        private float eyeCloseTimer = 0f;
        private float eyeCloseThreshold = 1.5f;
        private bool isEyeClosed = false;

        // MidEyeHelper integration variables
        private MidEyeGazeHelper midEyeHelper;
        public string focusOBJ;

        private Coroutine flashCoroutine;
        [SerializeField] private float flashDuration = 5f;
        [SerializeField] private float flashSpeed = 2f;


        private float autoFlashTimer = 0f;
        private float autoFlashInterval = 30f;

        void Start()
        {
            midRay = midRayOB.GetComponent<LineRenderer>();
            gazeIndicator = transform.GetChild(1).gameObject;

            if (gazeIndicator.GetComponent<Renderer>() == null)
            {
                var renderer = gazeIndicator.AddComponent<MeshRenderer>();
                renderer.material = new Material(Shader.Find("Standard"));
            }


            var mat = gazeIndicator.GetComponent<Renderer>().material;
            SetMaterialToFadeMode(mat);


            mat.color = new Color(1f, 1f, 0f, 0.3f);

            midEyeHelper = GetComponent<MidEyeGazeHelper>();
        }

        void Update()
        {
            DetectEyeBlink();
            if (!isEyeGazeActive) return;

            RaycastHit hitMid;
            RaycastMidEye(leftEye, rightEye, out hitMid, midRay, maxGazeDistance);
            focusOBJ = midEyeHelper != null ? midEyeHelper.focusOBJ : "";


            autoFlashTimer += Time.deltaTime;
            if (autoFlashTimer >= autoFlashInterval)
            {
                autoFlashTimer = 0f;
                SetGazeState(true);
            }
        }

        private void DetectEyeBlink()
        {
            if (leftEye == null || rightEye == null) return;

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

        private bool RaycastMidEye(OVREyeGaze gazeL, OVREyeGaze gazeR, out RaycastHit hit,
                                   LineRenderer visualRay, float distance = 1000f)
        {
            if (gazeL == null || gazeR == null)
            {
                hit = default;
                return false;
            }

            adjustedEyeL = gazeL.transform;
            adjustedEyeR = gazeR.transform;

            adjustedEyeL.eulerAngles = gazeL.transform.eulerAngles + new Vector3(eyeXOffset, eyeYOffset, 0f);
            adjustedEyeR.eulerAngles = gazeR.transform.eulerAngles + new Vector3(eyeXOffset, eyeYOffset, 0f);

            if (showRays)
            {
                visualRay.SetPositions(new Vector3[]
                {
                    (adjustedEyeL.position + adjustedEyeR.position) / 2,
                    (adjustedEyeL.forward * distance + adjustedEyeR.forward * distance) / 2
                });
            }

            return Physics.Raycast(
                (adjustedEyeL.position + adjustedEyeR.position) / 2,
                ((adjustedEyeL.forward * distance + adjustedEyeR.forward * distance) / 2).normalized,
                out hit,
                distance
            );
        }

        public void SetGazeState(bool isGazeOn)
        {

            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
                flashCoroutine = null;
            }

            if (isGazeOn)
            {

                flashCoroutine = StartCoroutine(FlashBlueForSeconds(flashDuration));
            }
            else
            {

                var mat = gazeIndicator.GetComponent<Renderer>().material;
                mat.color = new Color(1f, 1f, 0f, 0.3f);
            }
        }

        private IEnumerator FlashBlueForSeconds(float duration)
        {
            float elapsed = 0f;
            var mat = gazeIndicator.GetComponent<Renderer>().material;

            while (elapsed < duration)
            {

                float t = Mathf.PingPong(Time.time * flashSpeed, 1f);
                float alpha = Mathf.Lerp(0.2f, 0.6f, t);


                mat.color = new Color(0f, 0f, 1f, alpha);

                elapsed += Time.deltaTime;
                yield return null;
            }


            mat.color = new Color(1f, 1f, 0f, 0.3f);
            flashCoroutine = null;
        }

        private void SetMaterialToFadeMode(Material mat)
        {
            if (mat == null) return;

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






