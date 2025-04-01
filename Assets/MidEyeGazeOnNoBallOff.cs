using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Metaface.Utilities
{
    public class MidEyeGazeOnNoBallOff : MonoBehaviour
    {
        [SerializeField] OVREyeGaze leftEye;
        [SerializeField] OVREyeGaze rightEye;
        [SerializeField] GameObject midRayOB;
        [SerializeField] private bool showRays = false;
        [SerializeField] private float maxGazeDistance = 1000f;

        private LineRenderer midRay;
        private GameObject gazeIndicator;
        public float eyeXOffset, eyeYOffset;
        private Transform adjustedEyeL, adjustedEyeR;

        // Timer variables
        private float gazeTimer = 0f;
        private bool isGazing = false;

        // Statistics variables
        private string path;
        private float totalGazeTime = 0f;
        private int triggerCount = 0;
        private Color currentColor = Color.yellow;
        private Vector3 currentScale;

        // MidEyeHelper integration variables
        private MidEyeGazeHelper midEyeHelper;
        public string focusOBJ;

        // 新增：用于平滑小球位置的变量
        private Vector3 _smoothedPosition;
        [Range(0f, 1f)]
        public float smoothingFactor = 0.1f;  // 值越小移动越平滑（跟随越慢）

        void Start()
        {
            // 初始化组件
            midRay = midRayOB.GetComponent<LineRenderer>();
            gazeIndicator = transform.GetChild(1).gameObject;

            if (gazeIndicator.GetComponent<Renderer>() == null)
            {
                gazeIndicator.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
            }

            // 设置初始颜色和尺寸
            gazeIndicator.GetComponent<Renderer>().material.color = Color.yellow;
            currentScale = gazeIndicator.transform.localScale;
            midEyeHelper = GetComponent<MidEyeGazeHelper>();

            // 初始化平滑位置为当前小球位置
            _smoothedPosition = gazeIndicator.transform.position;

            // 设置数据存储
            string directory = Application.dataPath + "/MidEyeGazeOnNoBallOff";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            path = directory + $"/GazeNoBallSummary_{timestamp}.csv";

            File.AppendAllText(path, "Timestamp,TotalGazeTime,IsGazing,FocusPosition,BallColor,BallScale,TriggerCount\n");
        }

        void Update()
        {
            // 眼动追踪处理
            RaycastHit hitMid;
            bool didHit = RaycastMidEye(leftEye, rightEye, out hitMid, midRay, maxGazeDistance);
            UpdateMidEye(didHit, hitMid);
            focusOBJ = midEyeHelper.focusOBJ;

            // 持续记录数据
            LogData();
        }

        private void UpdateMidEye(bool didHit, RaycastHit hit)
        {
            if (didHit)
            {
                // 获取射线击中的目标位置
                Vector3 desiredPosition = hit.point;

                // 使用 Lerp 平滑插值更新 _smoothedPosition
                _smoothedPosition = Vector3.Lerp(_smoothedPosition, desiredPosition, smoothingFactor);

                // 更新小球位置为平滑后的结果
                transform.GetChild(1).transform.position = _smoothedPosition;
                gazeIndicator.transform.position = _smoothedPosition;

                // 更新眼动状态
                if (!isGazing)
                {
                    isGazing = true;
                    triggerCount++;
                }

                // 累计注视时间
                gazeTimer += Time.deltaTime;
                totalGazeTime += Time.deltaTime;
            }
            else
            {
                isGazing = false;
                gazeTimer = 0f;
            }

            // 保持小球颜色和尺寸
            gazeIndicator.GetComponent<Renderer>().material.color = Color.yellow;
            gazeIndicator.transform.localScale = currentScale;
            currentColor = Color.yellow;
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
                    (adjustedEyeL.forward * distance + adjustedEyeR.forward * distance) / 2
                });
            }

            return Physics.Raycast(
                (adjustedEyeL.transform.position + adjustedEyeR.transform.position) / 2,
                ((adjustedEyeL.forward * distance + adjustedEyeR.forward * distance) / 2).normalized,
                out hit,
                distance
            );
        }

        private void LogData()
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string focusPosition = isGazing ? gazeIndicator.transform.position.ToString("F2") : "None";
            string color = currentColor == Color.yellow ? "Yellow" : "Blue";
            string scale = currentScale.ToString("F2");

            string line = $"{timestamp},{totalGazeTime:F2},{isGazing},{focusPosition},{color},{scale},{triggerCount}\n";
            File.AppendAllText(path, line);
        }
    }
}
