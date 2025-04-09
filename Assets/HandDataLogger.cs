using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;  // 用于 DateTime

public class HandDataLogger : MonoBehaviour
{
    [Header("Log Settings")]
    [Tooltip("CSV 文件名称前缀")]
    public string fileNamePrefix = "HandDataLogger";
    [Tooltip("是否将采集数据写入文件中")]
    public bool logToFile = true;
    [Tooltip("是否采集并写入关节数据")]
    public bool logJoints = true;
    [Tooltip("是否采集速度数据")]
    public bool logVelocity = true;
    [Tooltip("是否采集加速度数据")]
    public bool logAcceleration = true;
    [Tooltip("是否记录抓取事件（如捏取开始/结束）")]
    public bool logGrabEvents = true;

    [Header("Hand Settings")]
    [Tooltip("手部名称：Left / Right")]
    public string handName = "Left";
    [Tooltip("用于判断抓取的捏取阈值")]
    public float pinchThreshold = 0.9f;

    // 内部日志写入
    private StreamWriter writer;
    private string filePath;
    private bool isLogging = false;

    // 用于计算运动动态的数据
    private Vector3 previousPosition;
    private Vector3 previousVelocity;
    private bool previousPinchState = false;

    // 引用 OVRHand 和 OVRSkeleton 组件（需要在手部物体上挂载 Oculus 手部相关组件）
    private OVRHand ovrHand;
    private OVRSkeleton ovrSkeleton;

    void Start()
    {
        // 尝试获取 OVRHand 和 OVRSkeleton 组件
        ovrHand = GetComponent<OVRHand>();
        ovrSkeleton = GetComponent<OVRSkeleton>();

        // 改为保存到 Assets/HandGesture 目录下
        if (logToFile)
        {
            string folderPath = Path.Combine(Application.dataPath, "HandGesture");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            filePath = Path.Combine(folderPath, fileNamePrefix + "_" + handName + "_" + timestamp + ".csv");

            writer = new StreamWriter(filePath);
            writer.WriteLine(GetCSVHeader());
            isLogging = true;
            Debug.Log("文件保存路径：" + filePath);
        }

        // 初始化前一帧数据
        previousPosition = transform.position;
        previousVelocity = Vector3.zero;
    }

    /// <summary>
    /// 构造 CSV 文件的表头，包含各字段名称
    /// </summary>
    string GetCSVHeader()
    {
        string header = "Timestamp,Hand,PosX,PosY,PosZ,RotX,RotY,RotZ,RotW";
        if (logVelocity)
        {
            header += ",VelX,VelY,VelZ";
        }
        if (logAcceleration)
        {
            header += ",AccX,AccY,AccZ";
        }
        if (logJoints && ovrSkeleton != null)
        {
            header += ",JointData";
        }
        if (logGrabEvents)
        {
            header += ",GrabEvent";
        }
        return header;
    }

    void Update()
    {
        // 时间戳（单位：秒）
        float timeStamp = Time.time;
        Vector3 currentPosition = transform.position;
        Quaternion currentRotation = transform.rotation;

        // 计算速度
        Vector3 velocity = Vector3.zero;
        if (logVelocity)
        {
            velocity = (currentPosition - previousPosition) / Time.deltaTime;
        }

        // 计算加速度
        Vector3 acceleration = Vector3.zero;
        if (logAcceleration)
        {
            acceleration = (velocity - previousVelocity) / Time.deltaTime;
        }

        // 采集所有关节数据（如果开启并且 OVRSkeleton 存在）
        string jointData = "";
        if (logJoints && ovrSkeleton != null && ovrSkeleton.Bones != null)
        {
            foreach (var bone in ovrSkeleton.Bones)
            {
                Vector3 jointPos = bone.Transform.position;
                Quaternion jointRot = bone.Transform.rotation;
                jointData += bone.Id + ":"
                           + jointPos.x.ToString("F3") + "|" + jointPos.y.ToString("F3") + "|" + jointPos.z.ToString("F3")
                           + "_"
                           + jointRot.x.ToString("F3") + "|" + jointRot.y.ToString("F3") + "|" + jointRot.z.ToString("F3") + "|" + jointRot.w.ToString("F3") + ";";
            }
        }

        // 利用 OVRHand 检测抓取（捏取）状态
        bool currentPinch = false;
        string grabEvent = "";
        if (logGrabEvents && ovrHand != null)
        {
            float pinchStrength = ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
            currentPinch = (pinchStrength >= pinchThreshold);
            if (currentPinch != previousPinchState)
            {
                grabEvent = currentPinch ? "GrabStart" : "GrabEnd";
            }
            previousPinchState = currentPinch;
        }

        // 拼装 CSV 行数据
        string line = "";
        line += timeStamp.ToString("F3") + "," +
                handName + "," +
                currentPosition.x.ToString("F3") + "," +
                currentPosition.y.ToString("F3") + "," +
                currentPosition.z.ToString("F3") + "," +
                currentRotation.x.ToString("F3") + "," +
                currentRotation.y.ToString("F3") + "," +
                currentRotation.z.ToString("F3") + "," +
                currentRotation.w.ToString("F3");

        if (logVelocity)
        {
            line += "," + velocity.x.ToString("F3") + "," + velocity.y.ToString("F3") + "," + velocity.z.ToString("F3");
        }
        if (logAcceleration)
        {
            line += "," + acceleration.x.ToString("F3") + "," + acceleration.y.ToString("F3") + "," + acceleration.z.ToString("F3");
        }
        if (logJoints)
        {
            line += ",\"" + jointData + "\"";
        }
        if (logGrabEvents)
        {
            line += "," + grabEvent;
        }

        if (isLogging)
        {
            writer.WriteLine(line);
        }

        previousPosition = currentPosition;
        previousVelocity = velocity;
    }

    void OnDestroy()
    {
        if (writer != null)
        {
            writer.Close();
        }
    }
}
