using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

/// <summary>
/// Records face express 
/// </summary>
public class FaceExpressionRecorder : MonoBehaviour
{

    [Header("Settings")]

    [SerializeField]
    private OVRFaceExpressions ovrFaceExpressions;

    [SerializeField, Range(1, 60)]
    private float recordFPS = 60f;

    //[SerializeField]
   // private StudyDataLogger studyDataLogger;

    [SerializeField]
    private int recordingSeconds = 60;

   //[SerializeField]
    //private int audioClipFrequency = 44100;

    [Header("UI")]

    [SerializeField]
    private TMPro.TMP_InputField recordingNameText;

    [SerializeField]
    private Button startRecordingButton;

    [SerializeField]
    private Button stopRecordingButton;

    [SerializeField]
    private TMPro.TMP_Dropdown microphoneDropdown;

    [SerializeField]
    private TMPro.TextMeshProUGUI timerText;

    private AudioClip audioClip;
    private string audioFilePath;
    private string microphoneDevice;

    private bool isRecording = false;

    private string faceFilePath;

    public bool startRecording, stopRecording;


    void Awake()
    {
        //Check microphone
        Assert.IsTrue(Microphone.devices.Length > 0, "No microphone found");

        //add microphone devices to dropdown
        microphoneDropdown.ClearOptions();
        microphoneDropdown.AddOptions(new List<string>(Microphone.devices));

        //add listener to dropdown
        microphoneDropdown.onValueChanged.AddListener((value) =>
        {
            microphoneDevice = Microphone.devices[value];
            Debug.Log($"Microphone device is {microphoneDevice}");
        });
    }

    void Start()
    {
        //Set inial device
        microphoneDevice = Microphone.devices[0];

        recordingNameText.onValueChanged.AddListener((value) =>
        {
            startRecordingButton.interactable = !string.IsNullOrEmpty(value);
            stopRecordingButton.interactable = !string.IsNullOrEmpty(value);
        });

        //Add listeners
        startRecordingButton.onClick.AddListener(() =>
        {
            StartRecording();
        });

        stopRecordingButton.onClick.AddListener(() =>
        {
            StopRecording();
        });

    }

    private void Update()
    {
        if (startRecording)
        {
            startRecording = false;
            StartRecording();
        }

        if (stopRecording)
        {
            stopRecording = false;
            StopRecording();
        }
    }


    public void StartRecording()
    {
        /**
        if (microphoneDevice != null)
        {
            Debug.Log($"Starting recording with device {microphoneDevice}");
            audioClip = Microphone.Start(
                microphoneDevice,
                false,
                recordingSeconds,
                audioClipFrequency);
        }
        */

        isRecording = true;
        StartCoroutine(RecordRoutine());
    }

    public void StopRecording()
    {
        isRecording = false;
    }

    private void CreateFiles()
    {
        string folderPath = System.IO.Path.Join(Application.dataPath, "FaceRecordings");

        if (!System.IO.Directory.Exists(folderPath))
            System.IO.Directory.CreateDirectory(folderPath);

        //Try create the file
        faceFilePath = System.IO.Path.Join(folderPath, $"{recordingNameText.text}_face.txt");
        if (!System.IO.File.Exists(faceFilePath))
        {
            System.IO.File.Create(faceFilePath).Close();
        }

        /**
        //Try create audio file
        audioFilePath = System.IO.Path.Join(folderPath, $"{recordingNameText.text}_audio.wav");
        if (!System.IO.File.Exists(audioFilePath))
        {
            System.IO.File.Create(audioFilePath).Close();
        }
        System.IO.File.WriteAllText(audioFilePath, "");
        */

        //CLear files just for testing
        System.IO.File.WriteAllText(faceFilePath, "");
    }

    private IEnumerator RecordRoutine()
    {
        Debug.Log("Starting face recording");

        CreateFiles();

        float time = 0;
        float timeTotal = 0;
        float fraction = 1000 / recordFPS;
        while (isRecording && timeTotal < recordingSeconds)
        {
            time += Time.deltaTime * 1000;
            timeTotal += Time.deltaTime;
            if (time >= fraction)
            {
                WriteFaceData();
                time = 0;
            }

            //timer text in seconds format
            timerText.text = $"{timeTotal:0.00}s";
            yield return new WaitForEndOfFrame();
        }

        /**
        //stop record audio
        if (microphoneDevice != null)
        {
            Debug.Log($"Stopping recording with device {microphoneDevice}");
            Microphone.End(microphoneDevice);
            SavWav.Save(audioFilePath, audioClip);
        }
        */
        Debug.Log($"Recoded {timeTotal} seconds of face");
    }

    /// <summary>
    /// Write the face data to a file
    /// </summary>
    /// <param name="timeStamp"></param>
    private void WriteFaceData()
    {

        string str = "";
        for (var expressionIndex = 0;
                expressionIndex < (int)OVRFaceExpressions.FaceExpression.Max; //63 = max
                ++expressionIndex)
        {
            //Try get the weight
            float weight;
            if (ovrFaceExpressions.TryGetFaceExpressionWeight((OVRFaceExpressions.FaceExpression)expressionIndex, out weight))
                str += $"{weight};";
            else
                str += "0;";
        }
        str += "\n";
        System.IO.File.AppendAllText(faceFilePath, str);
    }

}