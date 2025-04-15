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
        [SerializeField] private OVREyeGaze leftEye;
        [SerializeField] private OVREyeGaze rightEye;
        [SerializeField] private GameObject midRayOB;
        [SerializeField] private bool showRays = false;
        [SerializeField] private float maxGazeDistance = 1000f;
        [SerializeField] private bool enableLogging = false;

        private LineRenderer midRay;
        private GameObject gazeIndicator;

        // 偏移设置（单位为角度），不直接修改眼睛 transform，而是在计算时叠加偏移
        public float eyeXOffset, eyeYOffset;

        // -------------------------------
        // 眼动采样数据相关变量
        // -------------------------------
        private float gazeTimer = 0f;
        private bool isGazing = false;
        private float gazeThreshold = 5f; // 持续凝视 5 秒触发蓝色状态
        private float totalGazeTime = 0f;
        private Vector3 gazeDirection = Vector3.zero; // 当前平滑后的视线方向

        // -------------------------------
        // 任务交互数据相关变量
        // -------------------------------
        private float blueDuration = 4f;  // 蓝色状态持续 4 秒
        private bool isBlueActive = false;
        private float blueTimer = 0f;
        private int blueTriggerCount = 0; // 记录触发蓝色状态次数

        // -------------------------------
        // 眨眼控制（用于开启/关闭 GazeIndicator）
        // -------------------------------
        private float eyeCloseTimer = 0f;
        private float eyeCloseThreshold = 1.5f; // 闭眼超过 1.5 秒视为眨眼
        private bool isEyeClosed = false;
        private bool isEyeGazeActive = true; // 控制是否显示 GazeIndicator
        private int ballToggleCount = 0;      // 记录眨眼切换次数

        // -------------------------------
        // 辅助/显示数据变量
        // -------------------------------
        private Color currentColor = Color.yellow;
        private Vector3 originalScale;
        private Vector3 currentScale;
        private string focusPosition = "None";

        // -------------------------------
        // 文件保存及日志数据变量
        // -------------------------------
        private string path;
        private string fileName;
        private float logInterval = 1f; // 每 1 秒写入一次日志
        private float logTimer = 0f;

        // -------------------------------
        // 平滑位置更新：采用 SmoothDamp
        // -------------------------------
        private Vector3 _smoothedPosition;
        private Vector3 _velocity = Vector3.zero; // SmoothDamp 用速度变量
        [SerializeField] private float smoothingTime = 0.05f;  // 数值越小响应越快，但平滑度降低

        void Start()
        {
            // 初始化 LineRenderer 和 GazeIndicator
            midRay = midRayOB.GetComponent<LineRenderer>();
            // 这里假设 gazeIndicator 是该物体第 2 个子物体（下标 1）
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

            // 设置数据存储目录（在 Application.dataPath 下创建文件夹）
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

            // 写入 CSV 文件表头：包含时间戳、眼动数据、任务交互数据、辅助数据
            File.AppendAllText(path, "Timestamp,TotalGazeTime,IsGazing,EyeClosed,FocusPosition,GazeDirection,LeftEyePosition,RightEyePosition,BallColor,BallScale,BlueTriggerCount,BallToggleCount\n");
        }

        void Update()
        {
            // 处理眨眼检测：判断闭眼情况来切换 GazeIndicator 的显示
            DetectEyeBlink();

            // 若眼动追踪处于关闭状态则直接返回
            if (!isEyeGazeActive)
                return;

            // 执行射线检测，计算视线方向，并更新 gazeIndicator
            RaycastHit hitMid;
            bool didHit = RaycastMidEye(out hitMid, midRay, maxGazeDistance);
            UpdateMidEye(didHit, hitMid);

            // 定时记录日志
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
            // 通过 Dot 判断眼睛是否接近向下（简单检测闭眼状态）
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
                // 切换眼动追踪状态：开启或关闭 gazeIndicator
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
        // 计算左右眼的平均 forward，并使用 Quaternion.Euler 叠加偏移
        private bool RaycastMidEye(out RaycastHit hit, LineRenderer visualRay, float distance = 1000f)
        {
            hit = default;
            if (leftEye == null || rightEye == null)
            {
                return false;
            }

            // 读取左右眼原始 forward，不修改眼球的 transform
            Vector3 rawForwardL = leftEye.transform.forward;
            Vector3 rawForwardR = rightEye.transform.forward;
            Vector3 avgForward = ((rawForwardL + rawForwardR) / 2f).normalized;

            // 生成偏移旋转，叠加 eyeXOffset、eyeYOffset（单位：角度）
            Quaternion offsetRot = Quaternion.Euler(eyeXOffset, eyeYOffset, 0f);
            Vector3 targetDirection = offsetRot * avgForward;

            // 对前一帧的 gazeDirection 使用 Slerp 平滑过渡到新方向
            gazeDirection = Vector3.Slerp(gazeDirection, targetDirection, Time.deltaTime * 8f);

            // 计算两个眼睛的位置平均值作为 Raycast 起点
            Vector3 midPosition = (leftEye.transform.position + rightEye.transform.position) / 2f;

            if (showRays && visualRay != null)
            {
                visualRay.SetPositions(new Vector3[] { midPosition, midPosition + gazeDirection * distance });
            }

            bool didHit = Physics.Raycast(midPosition, gazeDirection, out hit, distance);
            Log($"Raycast hit: {didHit}, Position: {(didHit ? hit.point.ToString("F2") : "N/A")}");
            return didHit;
        }
        #endregion

        #region Update Gaze & Task Interaction
        private void UpdateMidEye(bool didHit, RaycastHit hit)
        {
            if (didHit && isEyeGazeActive)
            {
                // 用 SmoothDamp 平滑更新 gazeIndicator 位置
                _smoothedPosition = Vector3.SmoothDamp(_smoothedPosition, hit.point, ref _velocity, smoothingTime);
                gazeIndicator.transform.position = _smoothedPosition;
                focusPosition = _smoothedPosition.ToString("F2");

                if (!isGazing)
                {
                    isGazing = true;
                    Log("Gazing started.");
                }

                gazeTimer += Time.deltaTime;
                totalGazeTime += Time.deltaTime;
                Log($"Gaze Timer: {gazeTimer}, Total Gaze Time: {totalGazeTime}");

                // 当持续凝视时间超过阈值时，激活蓝色状态
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
                // 蓝色状态时将 scale 放大（例如放大 2.5 倍）
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
                string eyeClosedStatus = (!isEyeGazeActive).ToString();
                string leftEyePos = $"\"{FormatVector3(leftEye.transform.position)}\"";
                string rightEyePos = $"\"{FormatVector3(rightEye.transform.position)}\"";
                string gazeDirectionStr = $"\"{FormatVector3(gazeDirection)}\"";
                string focusPosQuoted = $"\"{focusPosition}\"";
                string formattedScaleQuoted = $"\"{FormatVector3(currentScale)}\"";

                string line = $"{timestamp},{totalGazeTime:F2},{isGazing},{eyeClosedStatus},{focusPosQuoted},{gazeDirectionStr},{leftEyePos},{rightEyePos},{ballColorString},{formattedScaleQuoted},{blueTriggerCount},{ballToggleCount}\n";
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



