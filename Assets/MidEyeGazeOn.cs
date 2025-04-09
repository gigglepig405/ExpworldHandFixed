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
        [SerializeField] private bool enableLogging = false;

        private LineRenderer midRay;
        private GameObject gazeIndicator;
        public float eyeXOffset, eyeYOffset;
        private Transform adjustedEyeL, adjustedEyeR;

        // -------------------------------
        // 时间采样与眼动状态记录
        // -------------------------------
        private float gazeTimer = 0f;
        private bool isGazing = false;
        private float gazeThreshold = 5f; // 持续凝视 5 秒触发一次目标指示事件
        private float totalGazeTime = 0f;
        private Vector3 gazeDirection; // 当前采样得到的视线方向

        // -------------------------------
        // Blink 控制变量
        // -------------------------------
        private bool isEyeGazeActive = true; // 控制是否允许眼动追踪（显示 GazeIndicator）
        private float eyeCloseTimer = 0f;
        private float eyeCloseThreshold = 1.5f; // 闭眼超过 1.5 秒认为眨眼
        private bool isEyeClosed = false;

        // -------------------------------
        // 辅助显示变量
        // -------------------------------
        private Vector3 originalScale;
        private Vector3 currentScale;
        // 本版本 GazeIndicator 始终保持黄色
        private Color currentColor = Color.yellow;

        // -------------------------------
        // 数据统计变量
        // -------------------------------
        private string path;
        private int triggerCount = 0;      // 记录至少一次凝视（每次从非凝视状态开始）
        private int ballToggleCount = 0;   // 记录闭眼切换次数

        // -------------------------------
        // MidEyeHelper 相关（如果有整合其它逻辑）
        // -------------------------------
        private MidEyeGazeHelper midEyeHelper;
        public string focusOBJ;

        // -------------------------------
        // 平滑位置更新（使用 SmoothDamp）
        // -------------------------------
        private Vector3 _smoothedPosition;
        private Vector3 _velocity = Vector3.zero;
        [SerializeField] private float smoothingTime = 0.05f; // 平滑时间，值越小响应越快

        void Start()
        {
            // 初始化组件
            midRay = midRayOB.GetComponent<LineRenderer>();
            gazeIndicator = transform.GetChild(1).gameObject;
            if (gazeIndicator.GetComponent<Renderer>() == null)
            {
                gazeIndicator.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
            }
            // 初始化指示器颜色（黄色）及数据
            gazeIndicator.GetComponent<Renderer>().material.color = Color.yellow;
            originalScale = gazeIndicator.transform.localScale;
            currentScale = originalScale;

            // 初始化平滑更新的位置为当前小球位置
            _smoothedPosition = gazeIndicator.transform.position;

            midEyeHelper = GetComponent<MidEyeGazeHelper>();

            // 设置数据存储目录
            string directory = Application.dataPath + "/MidEyeGazeOn";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            path = directory + $"/GazeOnSummary_{timestamp}.csv";

            // 写入 CSV 表头（包含眼动采样数据、任务交互数据、同步时间戳数据及辅助数据）
            File.AppendAllText(path, "Timestamp,TotalGazeTime,IsGazing,EyeClosed,FocusPosition,GazeDirection,LeftEyePosition,RightEyePosition,BallColor,BallScale,TriggerCount,BallToggleCount\n");
        }

        void Update()
        {
            // 先执行射线检测，并更新眼动采样数据
            RaycastHit hitMid;
            bool didHit = RaycastMidEye(leftEye, rightEye, out hitMid, midRay, maxGazeDistance);
            UpdateMidEye(didHit, hitMid);

            // 获取 MidEyeHelper 的 focusOBJ （如有整合其他逻辑）
            if (midEyeHelper != null)
                focusOBJ = midEyeHelper.focusOBJ;

            // 记录数据（包含同步时间戳、采样数据、辅助数据）
            LogData();

            // 检测眨眼切换（用于开启/关闭 GazeIndicator）
            DetectEyeBlink();
        }

        #region Eye Blink Detection
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
                // 切换眼动激活状态
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
            adjustedEyeL = gazeL.transform;
            adjustedEyeR = gazeR.transform;
            adjustedEyeL.eulerAngles = gazeL.transform.eulerAngles + new Vector3(eyeXOffset, eyeYOffset, 0f);
            adjustedEyeR.eulerAngles = gazeR.transform.eulerAngles + new Vector3(eyeXOffset, eyeYOffset, 0f);

            // 计算左右眼的中间位置与中间视线方向
            Vector3 midPosition = (adjustedEyeL.position + adjustedEyeR.position) / 2;
            Vector3 midDirection = ((adjustedEyeL.forward + adjustedEyeR.forward) / 2).normalized;
            // 保存采样的视线方向数据
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

        #region Update Gaze & Smooth Indicator Movement
        private void UpdateMidEye(bool didHit, RaycastHit hit)
        {
            if (didHit && isEyeGazeActive)
            {
                // 使用 SmoothDamp 平滑更新位置
                _smoothedPosition = Vector3.SmoothDamp(_smoothedPosition, hit.point, ref _velocity, smoothingTime);
                gazeIndicator.transform.position = _smoothedPosition;
                // 更新记录的 FocusPosition 为平滑后的位置字符串
                string posString = _smoothedPosition.ToString("F2");

                // 如果当前从未进入凝视状态，则记录一次触发（任务交互数据）
                if (!isGazing)
                {
                    isGazing = true;
                    triggerCount++;
                }
                // 累计凝视时间
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

        #region Data Logging
        private void LogData()
        {
            try
            {
                // 时间戳
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // FocusPosition：如果正在凝视，则记录指示器位置，否则记录 "None"
                // 此处 focusPos 依然可能包含逗号，比如 "(3.14, 1.59, -2.65)"
                string focusPos = isGazing ? gazeIndicator.transform.position.ToString("F2") : "None";
                // 将其用引号包裹，确保 CSV 不会因逗号产生跨列
                string focusPosQuoted = $"\"{focusPos}\"";

                // GazeDirection：同理做双引号包裹
                string gazeDirStr = $"\"{FormatVector3(gazeDirection)}\"";

                // 左右眼位置
                string leftEyePos = $"\"{FormatVector3(leftEye.transform.position)}\"";
                string rightEyePos = $"\"{FormatVector3(rightEye.transform.position)}\"";

                // BallColor 在本例中固定为 Yellow，这里无需引号（若你想保持风格一致，也可加上）
                string ballColorStr = "Yellow";

                // BallScale 若也包含逗号，需要同样包裹
                string ballScaleStr = $"\"{FormatVector3(currentScale)}\"";

                // EyeClosed 状态：以 isEyeGazeActive 的反值表示（若 isEyeGazeActive = false 表示眼睛闭合）
                string eyeClosedStr = (!isEyeGazeActive).ToString();

                // 组装 CSV 行
                string line = $"{timestamp}," +
                              $"{totalGazeTime:F2}," +
                              $"{isGazing}," +
                              $"{eyeClosedStr}," +
                              $"{focusPosQuoted}," +        // 用了 focusPosQuoted
                              $"{gazeDirStr}," +            // 同样用引号包裹
                              $"{leftEyePos}," +
                              $"{rightEyePos}," +
                              $"{ballColorStr}," +
                              $"{ballScaleStr}," +          // 同样用引号包裹
                              $"{triggerCount}," +
                              $"{ballToggleCount}\n";

                // 写入 CSV
                File.AppendAllText(path, line);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to log data: {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods
        private string FormatVector3(Vector3 vector)
        {
            return $"({vector.x:F2}, {vector.y:F2}, {vector.z:F2})";
        }

        #endregion
    }
}

