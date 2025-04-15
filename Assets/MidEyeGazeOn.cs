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

        // 用于微调眼动数据的偏移（单位：角度），注意：不直接修改眼睛 transform
        public float eyeXOffset, eyeYOffset;

        // -------------------------------
        // 时间采样与眼动状态记录
        // -------------------------------
        private float gazeTimer = 0f;
        private bool isGazing = false;
        [SerializeField] private float gazeThreshold = 5f; // 持续凝视 5 秒触发一次目标指示事件
        private float totalGazeTime = 0f;
        private Vector3 gazeDirection = Vector3.zero; // 平滑后的视线方向

        // -------------------------------
        // Blink 控制变量（眨眼检测用于开关 gazeIndicator）
        // -------------------------------
        private bool isEyeGazeActive = true;
        private float eyeCloseTimer = 0f;
        [SerializeField] private float eyeCloseThreshold = 1.5f; // 闭眼超过 1.5 秒认为眨眼
        private bool isEyeClosed = false;

        // -------------------------------
        // 辅助显示变量（baseline 下保持固定为黄色，不进行颜色/尺寸变化）
        // -------------------------------
        private Vector3 originalScale;
        private Vector3 currentScale;
        private Color currentColor = Color.yellow; // 固定黄色

        // -------------------------------
        // 数据统计变量
        // -------------------------------
        private string path;
        private int triggerCount = 0;      // 记录至少一次凝视（每次从非凝视状态开始）
        private int ballToggleCount = 0;   // 记录眨眼切换次数

        // -------------------------------
        // MidEyeHelper 相关（如有其它逻辑整合）
        // -------------------------------
        private MidEyeGazeHelper midEyeHelper;
        public string focusOBJ;

        // -------------------------------
        // 平滑位置更新（使用 SmoothDamp）
        // -------------------------------
        private Vector3 _smoothedPosition;
        private Vector3 _velocity = Vector3.zero;
        [SerializeField] private float smoothingTime = 0.05f; // 值越小响应越快，但平滑性降低

        void Start()
        {
            // 初始化组件
            midRay = midRayOB.GetComponent<LineRenderer>();
            // 假设 gazeIndicator 为当前对象下的第二个子物体
            gazeIndicator = transform.GetChild(1).gameObject;
            if (gazeIndicator.GetComponent<Renderer>() == null)
            {
                Renderer rend = gazeIndicator.AddComponent<MeshRenderer>();
                rend.material = new Material(Shader.Find("Standard"));
            }
            // 固定设置为黄色（baseline condition，不做颜色切换）
            Material mat = gazeIndicator.GetComponent<Renderer>().material;
            mat.color = Color.yellow;
            originalScale = gazeIndicator.transform.localScale;
            currentScale = originalScale;
            _smoothedPosition = gazeIndicator.transform.position;

            midEyeHelper = GetComponent<MidEyeGazeHelper>();

            // 设置数据存储目录和日志文件
            string directory = Application.dataPath + "/MidEyeGazeOn";
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            path = System.IO.Path.Combine(directory, $"GazeOnSummary_{timestamp}.csv");

            // 使用 File.WriteAllText 写入表头（避免重复追加），表头包含 BallScale 列
            string header = "Timestamp,TotalGazeTime,IsGazing,EyeClosed,FocusPosition," +
                            "GazeDirection,LeftEyePosition,RightEyePosition,BallColor,BallScale,TriggerCount,BallToggleCount\n";
            File.WriteAllText(path, header);
        }

        void Update()
        {
            // 首先执行射线检测与 gaze 数据采样
            RaycastHit hitMid;
            bool didHit = RaycastMidEye(out hitMid, midRay, maxGazeDistance);
            UpdateMidEye(didHit, hitMid);

            // 如果有 midEyeHelper 整合其它逻辑，则更新 focusOBJ
            if (midEyeHelper != null)
                focusOBJ = midEyeHelper.focusOBJ;

            // 记录数据日志（每帧记录或按时间间隔记录，根据需求）
            LogData();

            // 执行眨眼检测，实现 gazeIndicator 的开关
            DetectEyeBlink();
        }

        #region 眨眼检测
        private void DetectEyeBlink()
        {
            // 通过左右眼 forward 与 Vector3.down 的夹角判断是否闭眼（简单检测）
            bool isLeftEyeClosed = Vector3.Dot(leftEye.transform.forward, Vector3.down) > 0.85f;
            bool isRightEyeClosed = Vector3.Dot(rightEye.transform.forward, Vector3.down) > 0.85f;
            bool eyesClosed = isLeftEyeClosed && isRightEyeClosed;

            if (eyesClosed)
                eyeCloseTimer += Time.deltaTime;
            else
                eyeCloseTimer = 0f;

            if (eyeCloseTimer >= eyeCloseThreshold && !isEyeClosed)
            {
                // 切换眼动激活状态
                isEyeGazeActive = !isEyeGazeActive;
                gazeIndicator.SetActive(isEyeGazeActive);
                isEyeClosed = true;
                ballToggleCount++;
                Log($"Blink detected. isEyeGazeActive: {isEyeGazeActive}, ToggleCount: {ballToggleCount}");
            }
            else if (!eyesClosed)
            {
                isEyeClosed = false;
            }
        }
        #endregion

        #region 射线检测与眼动数据采样
        private bool RaycastMidEye(out RaycastHit hit, LineRenderer visualRay, float distance = 1000f)
        {
            hit = default;
            if (leftEye == null || rightEye == null)
                return false;

            // 取左右眼的原始 forward 向量，不修改 transform
            Vector3 rawForwardL = leftEye.transform.forward;
            Vector3 rawForwardR = rightEye.transform.forward;
            // 计算平均方向
            Vector3 avgForward = (rawForwardL + rawForwardR).normalized;
            // 计算偏移旋转（不直接修改眼睛的 transform）
            Quaternion offsetRot = Quaternion.Euler(eyeXOffset, eyeYOffset, 0f);
            // 得到目标视线方向，并通过 Slerp 平滑过渡
            Vector3 targetDirection = offsetRot * avgForward;
            gazeDirection = Vector3.Slerp(gazeDirection, targetDirection, Time.deltaTime * 8f);

            // 取两眼位置的平均值作为射线起点
            Vector3 midPosition = (leftEye.transform.position + rightEye.transform.position) / 2f;

            if (showRays && visualRay != null)
            {
                visualRay.SetPositions(new Vector3[]
                {
                    midPosition,
                    midPosition + gazeDirection * distance
                });
            }

            bool didHit = Physics.Raycast(midPosition, gazeDirection, out hit, distance);
            Log($"Raycast hit: {didHit}, HitPoint: {(didHit ? hit.point.ToString("F2") : "N/A")}");
            return didHit;
        }
        #endregion

        #region 更新眼动状态与平滑移动
        private void UpdateMidEye(bool didHit, RaycastHit hit)
        {
            if (didHit && isEyeGazeActive)
            {
                // 使用 SmoothDamp 平滑更新 gazeIndicator 位置
                _smoothedPosition = Vector3.SmoothDamp(_smoothedPosition, hit.point, ref _velocity, smoothingTime);
                gazeIndicator.transform.position = _smoothedPosition;

                // 如果从未进入凝视状态，则记录一次触发事件
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

        #region 数据日志记录
        private void LogData()
        {
            try
            {
                // 时间戳
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // FocusPosition：若正在凝视则记录 gazeIndicator 的位置，否则记录 "None"
                string focusPos = isGazing ? gazeIndicator.transform.position.ToString("F2") : "None";
                string focusPosQuoted = $"\"{focusPos}\"";
                // GazeDirection（使用自定义格式，双引号包裹）
                string gazeDirStr = $"\"{FormatVector3(gazeDirection)}\"";
                // 左右眼位置
                string leftEyePos = $"\"{FormatVector3(leftEye.transform.position)}\"";
                string rightEyePos = $"\"{FormatVector3(rightEye.transform.position)}\"";
                // BallColor 固定为 Yellow（baseline condition）
                string ballColorStr = "Yellow";
                // BallScale：记录 gazeIndicator 的缩放（格式化后，双引号包裹）
                string ballScaleStr = $"\"{FormatVector3(gazeIndicator.transform.localScale)}\"";

                string line = $"{timestamp}," +
                              $"{totalGazeTime:F2}," +
                              $"{isGazing}," +
                              $"{(!isEyeGazeActive).ToString()}," +
                              $"{focusPosQuoted}," +
                              $"{gazeDirStr}," +
                              $"{leftEyePos}," +
                              $"{rightEyePos}," +
                              $"{ballColorStr}," +
                              $"{ballScaleStr}," +
                              $"{triggerCount}," +
                              $"{ballToggleCount}\n";

                File.AppendAllText(path, line);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to log data: {ex.Message}");
            }
        }
        #endregion

        #region 辅助方法
        private string FormatVector3(Vector3 vector)
        {
            return $"({vector.x:F2}, {vector.y:F2}, {vector.z:F2})";
        }

        private void Log(string message)
        {
            if (enableLogging)
                UnityEngine.Debug.Log(message);
        }
        #endregion
    }
}

