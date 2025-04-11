using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Metaface.Utilities
{
    public class MidEyeGazeRenderer : MonoBehaviour
    {
        [Header("Eye Gaze Settings")]
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

        // -------------------------------
        // 眼动采样数据相关变量
        // -------------------------------
        private float gazeTimer = 0f;
        private bool isGazing = false;
        private float gazeThreshold = 5f; // 持续凝视 5 秒触发蓝色状态
        private float totalGazeTime = 0f;
        private Vector3 gazeDirection; // 当前采样到的视线方向

        // -------------------------------
        // 任务交互数据相关变量
        // -------------------------------
        private float blueDuration = 4f;  // 蓝色状态维持 4 秒
        private bool isBlueActive = false;
        private float blueTimer = 0f;
        private int blueTriggerCount = 0; // 记录目标指示事件次数

        // -------------------------------
        // 眼闭合/眨眼控制（用于开启/关闭 GazeIndicator）
        // -------------------------------
        private float eyeCloseTimer = 0f;
        private float eyeCloseThreshold = 1.5f; // 闭眼超过 1.5 秒视为一次眨眼
        private bool isEyeClosed = false;
        private bool isEyeGazeActive = true; // 控制是否显示 GazeIndicator
        private int ballToggleCount = 0;      // 记录闭眼切换次数

        // -------------------------------
        // 辅助/显示数据相关变量
        // -------------------------------
        private Color currentColor = Color.yellow;
        private Vector3 originalScale;
        private Vector3 currentScale;
        private string focusPosition = "None";

        // -------------------------------
        // 文件保存及日志数据
        // -------------------------------
        private string path;
        private string fileName;
        private float logInterval = 1f; // 每 1 秒写入一次数据
        private float logTimer = 0f;

        // -------------------------------
        // 平滑位置更新：采用 SmoothDamp
        // -------------------------------
        private Vector3 _smoothedPosition;
        private Vector3 _velocity = Vector3.zero; // 用于 SmoothDamp 的速度变量
        [SerializeField] private float smoothingTime = 0.05f;  // 平滑时间，值越小响应越快

        void Start()
        {
            // 初始化 LineRenderer 和 GazeIndicator
            midRay = midRayOB.GetComponent<LineRenderer>();
            gazeIndicator = transform.GetChild(1).gameObject;
            if (gazeIndicator.GetComponent<Renderer>() == null)
            {
                gazeIndicator.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
            }
            Material mat = gazeIndicator.GetComponent<Renderer>().material;
            SetMaterialToFadeMode(mat);
            // 初始材质设置为半透明黄色
            mat.color = new Color(1f, 1f, 0f, 0.3f);

            originalScale = gazeIndicator.transform.localScale;
            currentScale = originalScale;

            // 初始化平滑位置为当前小球位置
            _smoothedPosition = gazeIndicator.transform.position;

            // 设置数据存储目录（在 Application.dataPath 下创建一个文件夹）
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

            // CSV 文件表头：包含时间戳、眼动采样数据（FocusPosition, GazeDirection, TotalGazeTime）、任务交互数据（BlueTriggerCount, BallToggleCount）
            // 以及辅助数据（左右眼位置、BallColor、BallScale）
            File.AppendAllText(path, "Timestamp,TotalGazeTime,IsGazing,EyeClosed,FocusPosition,GazeDirection,LeftEyePosition,RightEyePosition,BallColor,BallScale,BlueTriggerCount,BallToggleCount\n");
        }

        void Update()
        {
            // 处理眨眼检测：闭眼/睁眼切换 GazeIndicator 开关
            DetectEyeBlink();

            // 执行射线检测，获取左右眼视线信息
            RaycastHit hitMid;
            bool didHit = RaycastMidEye(leftEye, rightEye, out hitMid, midRay, maxGazeDistance);
            UpdateMidEye(didHit, hitMid);

            // 每 logInterval 秒记录一次数据
            logTimer += Time.deltaTime;
            if (logTimer >= logInterval)
            {
                LogData();
                logTimer = 0f;
            }
        }

        #region Eye Blink & Gaze Toggle
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
                // 切换是否允许眼动追踪（开启或关闭 GazeIndicator）
                isEyeGazeActive = !isEyeGazeActive;
                gazeIndicator.SetActive(isEyeGazeActive);

                Log($"Blink detected. isEyeGazeActive: {isEyeGazeActive}, BallToggleCount: {ballToggleCount}");
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
            adjustedEyeL = gazeL.transform;
            adjustedEyeR = gazeR.transform;
            adjustedEyeL.eulerAngles = gazeL.transform.eulerAngles + new Vector3(eyeXOffset, eyeYOffset, 0f);
            adjustedEyeR.eulerAngles = gazeR.transform.eulerAngles + new Vector3(eyeXOffset, eyeYOffset, 0f);

            Vector3 midPosition = (adjustedEyeL.position + adjustedEyeR.position) / 2;
            Vector3 midDirection = ((adjustedEyeL.forward + adjustedEyeR.forward) / 2).normalized;
            // 保存采样到的视线方向（眼动采样数据）
            gazeDirection = midDirection;

            if (showRays)
            {
                visualRay.SetPositions(new Vector3[] { midPosition, midPosition + midDirection * distance });
            }

            bool didHit = Physics.Raycast(midPosition, midDirection, out hit, distance);
            Log($"Raycast hit: {didHit}, Position: {(didHit ? hit.point.ToString("F2") : "N/A")}");
            return didHit;
        }
        #endregion

        #region Update Gaze & Task Interaction
        private void UpdateMidEye(bool didHit, RaycastHit hit)
        {
            if (didHit && isEyeGazeActive)
            {
                // 使用 SmoothDamp 平滑更新 _smoothedPosition，使 gazeIndicator 跟踪射线击中点
                _smoothedPosition = Vector3.SmoothDamp(_smoothedPosition, hit.point, ref _velocity, smoothingTime);
                gazeIndicator.transform.position = _smoothedPosition;
                // 更新记录的 focusPosition
                focusPosition = _smoothedPosition.ToString("F2");

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
                        Log("Blue state deactivated.");
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
        #endregion

        #region Update & Reset Indicator Appearance
        private void UpdateIndicator()
        {
            Material mat = gazeIndicator.GetComponent<Renderer>().material;
            if (isBlueActive)
            {
                currentColor = Color.blue;
                currentScale = originalScale * 2.5f;
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
            Material mat = gazeIndicator.GetComponent<Renderer>().material;
            mat.color = currentColor;
            gazeIndicator.transform.localScale = currentScale;
        }
        #endregion

        #region Data Logging
        private void LogData()
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string ballColorString = GetBallColorString();
                string eyeClosedStatus = !isEyeGazeActive ? "True" : "False";
                string leftEyePos = $"\"{FormatVector3(leftEye.transform.position)}\"";
                string rightEyePos = $"\"{FormatVector3(rightEye.transform.position)}\"";
                string gazeDirectionStr = $"\"{FormatVector3(gazeDirection)}\"";
                string focusPositionQuoted = $"\"{focusPosition}\"";
                string formattedScaleQuoted = $"\"{FormatVector3(currentScale)}\"";

                string line = $"{timestamp},{totalGazeTime:F2},{isGazing},{eyeClosedStatus},{focusPositionQuoted},{gazeDirectionStr},{leftEyePos},{rightEyePos},{ballColorString},{formattedScaleQuoted},{blueTriggerCount},{ballToggleCount}\n";
                File.AppendAllText(path, line);
            }
            catch (Exception ex)
            {
                LogError($"Failed to log data: {ex.Message}");
            }
        }
        #endregion

        #region Helper Methods
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
        #endregion
    }
}
























