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

        // -------------------------------
        // 眼动采样数据及交互数据
        // -------------------------------
        private float gazeTimer = 0f;
        private bool isGazing = false;
        private float gazeThreshold = 5f; // 持续凝视 5 秒触发目标（蓝色状态）
        private float totalGazeTime = 0f;
        private Vector3 gazeDirection; // 当前采样的视线方向（平滑处理后的方向）
        private Vector3 targetGazeDirection; // 计算得到的目标方向（未经过平滑）

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
        [SerializeField] private float smoothingTime = 0.05f; // 数值越小，移动越快但平滑度降低

        // -------------------------------
        // 数据记录
        // -------------------------------
        private float logInterval = 1f; // 每 1 秒写入一次日志
        private float logTimer = 0f;

        void Start()
        {
            // 初始化组件
            midRay = midRayOB.GetComponent<LineRenderer>();
            gazeIndicator = transform.GetChild(1).gameObject;

            // 添加 MeshRenderer 如果不存在
            if (gazeIndicator.GetComponent<Renderer>() == null)
            {
                var renderer = gazeIndicator.AddComponent<MeshRenderer>();
                renderer.material = new Material(Shader.Find("Standard"));
            }

            // 设置初始材质为半透明黄色
            Material mat = gazeIndicator.GetComponent<Renderer>().material;
            SetMaterialToFadeMode(mat);
            mat.color = new Color(1f, 1f, 0f, 0.3f);

            originalScale = gazeIndicator.transform.localScale;
            currentScale = originalScale;
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

            // 若处于关闭状态则不进行眼动追踪更新
            if (!isEyeGazeActive)
                return;

            // 执行射线检测，更新眼动采样数据和指示器位置
            RaycastHit hitMid;
            bool didHit = RaycastMidEye(out hitMid, midRay, maxGazeDistance);
            UpdateMidEye(didHit, hitMid);

            // 每隔 autoFlashInterval 秒自动触发蓝色闪烁
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

            // 获取 midEyeHelper 的 focusOBJ（如果有）
            if (midEyeHelper != null)
                focusOBJ = midEyeHelper.focusOBJ;
        }

        #region Blink Detection & Gaze Toggle
        private void DetectEyeBlink()
        {
            if (leftEye == null || rightEye == null) return;

            // 这里使用 Dot 产品简单判断眼睛是否闭合：当前策略是检测眼睛是否大部分向下
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
                // 反转眼动状态
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
        // 不再直接修改 OVREyeGaze 的 transform，采用本地变量计算方向
        private bool RaycastMidEye(out RaycastHit hit, LineRenderer visualRay, float distance = 1000f)
        {
            hit = default;
            if (leftEye == null || rightEye == null)
            {
                return false;
            }

            // 获取左右眼原始 forward，不修改原始 transform
            Vector3 rawForwardL = leftEye.transform.forward;
            Vector3 rawForwardR = rightEye.transform.forward;

            // 计算平均 forward，并加入偏移，注意不要修改眼睛组件的 transform！
            Vector3 avgForward = ((rawForwardL + rawForwardR) / 2f).normalized;
            Quaternion offsetRot = Quaternion.Euler(eyeXOffset, eyeYOffset, 0f);
            targetGazeDirection = offsetRot * avgForward;

            // 可选：对 gazeDirection 使用 Slerp 平滑（平滑因子可调节）
            gazeDirection = Vector3.Slerp(gazeDirection, targetGazeDirection, Time.deltaTime * 8f);

            // 起点采用两眼位置平均计算
            Vector3 midPosition = (leftEye.transform.position + rightEye.transform.position) / 2f;

            if (showRays && visualRay != null)
            {
                visualRay.SetPositions(new Vector3[]
                {
                    midPosition,
                    midPosition + gazeDirection * distance
                });
            }

            return Physics.Raycast(midPosition, gazeDirection, out hit, distance);
        }
        #endregion

        #region Update Gaze & Smooth Movement
        private void UpdateMidEye(bool didHit, RaycastHit hit)
        {
            if (didHit && isEyeGazeActive)
            {
                // 平滑更新 gazeIndicator 位置
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
                // 通过 PingPong 函数让透明度在 0.2 到 0.6 之间变化
                float t = Mathf.PingPong(Time.time * flashSpeed, 1f);
                float alpha = Mathf.Lerp(0.2f, 0.6f, t);
                mat.color = new Color(0f, 0f, 1f, alpha);

                // 固定蓝色时尺寸为 (0.13, 0.13, 0.13)
                gazeIndicator.transform.localScale = new Vector3(0.13f, 0.13f, 0.13f);

                elapsed += Time.deltaTime;
                yield return null;
            }

            // 恢复黄色和默认大小（这里默认设定为 0.05）
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
                // 当前时间戳
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // FocusPosition（用引号包裹防止逗号问题）
                string focusPos = isGazing ? gazeIndicator.transform.position.ToString("F2") : "None";
                string focusPosQuoted = $"\"{focusPos}\"";

                // 格式化 gazeDirection、LeftEyePosition、RightEyePosition
                string gazeDirStr = $"\"{FormatVector3(gazeDirection)}\"";
                string leftEyePos = $"\"{FormatVector3(leftEye.transform.position)}\"";
                string rightEyePos = $"\"{FormatVector3(rightEye.transform.position)}\"";

                // 简单判断当前颜色：黄色或蓝色
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

