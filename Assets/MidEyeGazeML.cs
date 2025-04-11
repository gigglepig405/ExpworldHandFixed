using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Metaface.Utilities
{
    public class MidEyeGazeML : MonoBehaviour
    {
        [SerializeField] private OVREyeGaze leftEye;
        [SerializeField] private OVREyeGaze rightEye;
        [SerializeField] private GameObject midRayOB;
        [SerializeField] private bool showRays = false;
        [SerializeField] private float maxGazeDistance = 1000f;
        [SerializeField] private bool enableLogging = false;

        private LineRenderer midRay;
        private GameObject gazeIndicator;
        public float eyeXOffset, eyeYOffset;
        private Transform adjustedEyeL, adjustedEyeR;

        // -------------------------------
        // 眼动采样数据及任务交互数据
        // -------------------------------
        private float gazeTimer = 0f;
        private bool isGazing = false;
        private float gazeThreshold = 5f; // 持续凝视 5 秒触发目标指示（蓝色状态）
        private float totalGazeTime = 0f;
        private Vector3 gazeDirection; // 当前采样的视线方向

        // -------------------------------
        // Blink 控制变量
        // -------------------------------
        private bool isEyeGazeActive = true;  // 控制是否显示 GazeIndicator
        private float eyeCloseTimer = 0f;
        private float eyeCloseThreshold = 1.5f; // 闭眼超过 1.5 秒视为眨眼
        private bool isEyeClosed = false;

        // -------------------------------
        // 辅助显示及闪烁效果
        // -------------------------------
        private Color currentColor = Color.yellow; // 默认黄色
        private Vector3 originalScale;
        private Vector3 currentScale;

        // -------------------------------
        // 数据统计变量
        // -------------------------------
        private string path;
        private int triggerCount = 0;       // 初次或非凝视->凝视 切换计数
        private int ballToggleCount = 0;    // 眨眼切换计数

        // -------------------------------
        // MidEyeHelper 集成变量
        // -------------------------------
        private MidEyeGazeHelper midEyeHelper;
        public string focusOBJ;

        // -------------------------------
        // 闪蓝控制
        // -------------------------------
        private Coroutine flashCoroutine;
        [SerializeField] private float flashDuration = 5f;
        [SerializeField] private float flashSpeed = 2f;

        private float autoFlashTimer = 0f;
        private float autoFlashInterval = 30f;

        // -------------------------------
        // 平滑移动（SmoothDamp）
        // -------------------------------
        private Vector3 _smoothedPosition;
        private Vector3 _velocity = Vector3.zero;
        [SerializeField] private float smoothingTime = 0.05f; // 数值越小，移动越快（但平滑度降低）

        // -------------------------------
        // 数据记录
        // -------------------------------
        private float logInterval = 1f; // 每 1 秒写入一次数据
        private float logTimer = 0f;

        void Start()
        {
            // 初始化组件
            midRay = midRayOB.GetComponent<LineRenderer>();
            gazeIndicator = transform.GetChild(1).gameObject;

            if (gazeIndicator.GetComponent<Renderer>() == null)
            {
                var renderer = gazeIndicator.AddComponent<MeshRenderer>();
                renderer.material = new Material(Shader.Find("Standard"));
            }

            // 设置初始材质
            Material mat = gazeIndicator.GetComponent<Renderer>().material;
            SetMaterialToFadeMode(mat);
            mat.color = new Color(1f, 1f, 0f, 0.3f); // 半透明黄色

            originalScale = gazeIndicator.transform.localScale;
            currentScale = originalScale;

            // 平滑移动初始化
            _smoothedPosition = gazeIndicator.transform.position;

            midEyeHelper = GetComponent<MidEyeGazeHelper>();

            // 设置数据存储目录并创建 CSV 文件
            string directory = Application.dataPath + "/MidEyeGazeML";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            string timestampFile = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            path = Path.Combine(directory, $"GazeMLSummary_{timestampFile}.csv");

            // CSV 表头
            File.AppendAllText(path, "Timestamp,TotalGazeTime,IsGazing,EyeClosed,FocusPosition,GazeDirection,LeftEyePosition,RightEyePosition,BallColor,BallScale,TriggerCount,BallToggleCount\n");
        }

        void Update()
        {
            // 检测眨眼切换
            DetectEyeBlink();

            // 若处于关闭状态，直接返回
            if (!isEyeGazeActive)
                return;

            // 执行射线检测，更新眼动采样数据和指示器位置
            RaycastHit hitMid;
            bool didHit = RaycastMidEye(leftEye, rightEye, out hitMid, midRay, maxGazeDistance);
            UpdateMidEye(didHit, hitMid);

            // 每隔 autoFlashInterval 秒自动闪蓝
            autoFlashTimer += Time.deltaTime;
            if (autoFlashTimer >= autoFlashInterval)
            {
                autoFlashTimer = 0f;
                SetGazeState(true);
            }

            // 定时写入日志
            logTimer += Time.deltaTime;
            if (logTimer >= logInterval)
            {
                LogData();
                logTimer = 0f;
            }

            // 如果有 MidEyeHelper，则获取 focusOBJ
            if (midEyeHelper != null)
                focusOBJ = midEyeHelper.focusOBJ;
        }

        #region Blink Detection & Gaze Toggle
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
                ballToggleCount++;
            }
            else if (!eyesClosed)
            {
                isEyeClosed = false;
            }
        }
        #endregion

        #region Raycasting & Gaze Sampling
        private bool RaycastMidEye(OVREyeGaze gazeL, OVREyeGaze gazeR, out RaycastHit hit, LineRenderer visualRay, float distance = 1000f)
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

            Vector3 midPosition = (adjustedEyeL.position + adjustedEyeR.position) / 2;
            Vector3 midDirection = ((adjustedEyeL.forward + adjustedEyeR.forward) / 2).normalized;
            gazeDirection = midDirection;

            if (showRays)
            {
                visualRay.SetPositions(new Vector3[]
                {
                    midPosition,
                    midPosition + midDirection * distance
                });
            }

            return Physics.Raycast(midPosition, midDirection, out hit, distance);
        }
        #endregion

        #region Update Gaze & Smooth Movement
        private void UpdateMidEye(bool didHit, RaycastHit hit)
        {
            if (didHit && isEyeGazeActive)
            {
                // 用 SmoothDamp 平滑更新位置
                _smoothedPosition = Vector3.SmoothDamp(_smoothedPosition, hit.point, ref _velocity, smoothingTime);
                gazeIndicator.transform.position = _smoothedPosition;

                if (!isGazing)
                {
                    isGazing = true;
                    triggerCount++;
                }
                gazeTimer += Time.deltaTime;
                totalGazeTime += Time.deltaTime;
            }
            else
            {
                isGazing = false;
                gazeTimer = 0f;
            }
        }
        #endregion

        #region SetGazeState & Flashing Blue
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
                // alpha 从 0.2 到 0.6 之间变化
                float t = Mathf.PingPong(Time.time * flashSpeed, 1f);
                float alpha = Mathf.Lerp(0.2f, 0.6f, t);
                mat.color = new Color(0f, 0f, 1f, alpha);

                // 固定蓝色时尺寸为 (0.13, 0.13, 0.13)
                gazeIndicator.transform.localScale = new Vector3(0.13f, 0.13f, 0.13f);

                elapsed += Time.deltaTime;
                yield return null;
            }

            // 恢复黄色和固定默认大小（仍为 0.13）
            mat.color = new Color(1f, 1f, 0f, 0.3f);
            gazeIndicator.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);

            flashCoroutine = null;
        }

        #endregion

        #region Data Logging
        private void LogData()
        {
            try
            {
                // 时间戳
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // FocusPosition 可能带逗号，需要用引号包裹
                // 若处于凝视状态，记录位置，否则 “None”
                string focusPos = isGazing ? gazeIndicator.transform.position.ToString("F2") : "None";
                string focusPosQuoted = $"\"{focusPos}\"";

                // GazeDirection、LeftEyePosition、RightEyePosition、BallScale 同样需要引号
                string gazeDirStr = $"\"{FormatVector3(gazeDirection)}\"";
                string leftEyePos = $"\"{FormatVector3(leftEye.transform.position)}\"";
                string rightEyePos = $"\"{FormatVector3(rightEye.transform.position)}\"";

                // 判断当前颜色
                // 如果材质颜色中 r/g 都近似 1，且 b=0，则视为 Yellow；否则视为 Blue（简单判断）
                // 你也可以通过维护 currentColor 来判断
                var matColor = gazeIndicator.GetComponent<Renderer>().material.color;
                string ballColorStr = (Mathf.Abs(matColor.r - 1f) < 0.01f && Mathf.Abs(matColor.g - 1f) < 0.01f && matColor.b < 0.1f)
                    ? "Yellow"
                    : "Blue";

                string ballScaleQuoted = $"\"{FormatVector3(gazeIndicator.transform.localScale)}\"";

                // EyeClosed：取 isEyeGazeActive 的反值
                string eyeClosedStr = (!isEyeGazeActive).ToString();

                string line = $"{timestamp}," +
                              $"{totalGazeTime:F2}," +
                              $"{isGazing}," +
                              $"{eyeClosedStr}," +
                              $"{focusPosQuoted}," +
                              $"{gazeDirStr}," +
                              $"{leftEyePos}," +
                              $"{rightEyePos}," +
                              $"{ballColorStr}," +
                              $"{ballScaleQuoted}," +
                              $"{triggerCount}," +
                              $"{ballToggleCount}\n";

                File.AppendAllText(path, line);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to log data: {ex.Message}");
            }
        }

        private string FormatVector3(Vector3 vector)
        {
            return $"({vector.x:F2}, {vector.y:F2}, {vector.z:F2})";
        }
        #endregion

        #region Helper Methods
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
        #endregion
    }
}




