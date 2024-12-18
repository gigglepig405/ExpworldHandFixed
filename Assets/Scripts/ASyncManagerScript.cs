using Metaface.Debug;
using ShimmeringUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Playables;
using System.IO;
using Photon.Pun;

public class ASyncManagerScript : MonoBehaviour
{
    public GameObject playbackTarget;
    public AsyncPlayerBodyBehaviour apbb;
    public GameObject rewindInt;

    public bool startRecording;
    bool recording;
    public bool stopRecording;

    public bool startRewind;
    public bool globalRewind;

    private string filePath_face;
    private string filePath_body;


    [SerializeField]
    private OVRFaceExpressions ovrFaceExpressions;

    [SerializeField]
    private NetworkPlayerBodySync bodySync;

    [Header("PlaybackSingleFile")]
    [SerializeField]
    private string playbackFaceRecording;
    private string playbackBodyRecording;

    [Header("References")]
    [SerializeField]
    private FacePlaybackSystem facePlaybackSystem;

    private float recordFPS = 60f;

    private bool isPlayback = false;

    public bool IsPlayback => isPlayback;

    // Update is called once per frame
    void Update()
    {
        if (startRecording)
        {
            startRecording = false;
            recording = true;
            ovrFaceExpressions = GameObject.Find("SyncAvatarLocal").GetComponent<OVRFaceExpressions>();
            bodySync = GameObject.Find("SyncAvatarLocal").transform.parent.parent.GetComponent<NetworkPlayerBodySync>();
            CreateAsyncFileAndRecord();
        }

        if (stopRecording)
        {
            stopRecording = false;
            recording = false;
        }

        if (startRewind)
        {
            startRewind = false;
            StartPlayback();
        }
    }
    void CreateAsyncFileAndRecord()
    {
        var folderPath = System.IO.Path.Join(Application.dataPath, "DataLog", "AsyncReplay");

        if (!System.IO.Directory.Exists(folderPath))
            System.IO.Directory.CreateDirectory(folderPath);


        filePath_face = System.IO.Path.Join(folderPath, "session_face.txt");
        filePath_body = System.IO.Path.Join(folderPath,"session_body.txt");


        //CLear files just for testing
        System.IO.File.WriteAllText(filePath_face, "");
        System.IO.File.WriteAllText(filePath_body, "");

        StartCoroutine(RecordRoutine());
    }


    public void StartPlayback()
    {

        if (IsPlayback) return;

        playbackFaceRecording = File.ReadAllText(filePath_face);
        playbackBodyRecording = File.ReadAllText(filePath_body);


        if (playbackFaceRecording != null)
        {
            isPlayback = true;
            StartCoroutine(PlaybackRoutine());
        }
    }

    private IEnumerator PlaybackRoutine()
    {
        Debug.Log("PLAYBACK ... BEGIN");
        playbackTarget.SetActive(true);
        //playbackTarget.transform.LookAt(GameObject.Find("HeadRefPoint_OffsetNormal").transform);
        rewindInt.SetActive(false);

        var lines_face = playbackFaceRecording.Split("\n");
        var lines_body = playbackBodyRecording.Split("\n");


        float time = 0;
        float timeTotal = 0;
        float fraction = 1000 / recordFPS;

        int lineIdx = 0;
        while (isPlayback && lineIdx < lines_face.Length) //I suppose they should be of same length since record at same time?
        {
            time += Time.deltaTime * 1000;
            timeTotal += Time.deltaTime;
            if (time >= fraction)
            {
                PlaybackFaceData(lines_face[lineIdx]);
                PlaybackBodyData(lines_body[lineIdx]);
                time = 0;
                lineIdx++;
            }
            yield return new WaitForEndOfFrame();
        }

        Debug.Log("PLAYBACK ... DONE");

        isPlayback = false;
        playbackTarget.SetActive(false);
        rewindInt.SetActive(true);
    }

    private void PlaybackFaceData(string faceDataLine)
    {
        var weights = new float[(int)OVRFaceExpressions.FaceExpression.Max];
        if (!string.IsNullOrWhiteSpace(faceDataLine))
        {
            var split = faceDataLine.Split(";");
            for (var expressionIndex = 0;
                expressionIndex < (int)OVRFaceExpressions.FaceExpression.Max;
                ++expressionIndex)
            {
                if (!float.TryParse(split[expressionIndex], out weights[expressionIndex]))
                {
                    weights[expressionIndex] = 0;
                }
            }
        }
        facePlaybackSystem?.ApplyFaceWeight(weights);
    }


    // Method to apply positions and rotations to all child objects
    private void PlaybackBodyData(string bodyDataLine)
    {

        var split = bodyDataLine.Split(";");
        Vector3 pos;
        Quaternion rot;

        int childCount = apbb.outputChildTransforms.Length;

        try
        {

            for (int i = 0; i < childCount; i++)
            {
                var p_t = split[i].Split("|")[0].Split(",");
                var p_r = split[i].Split("|")[1].Split(",");

                pos = new Vector3(float.Parse(p_t[0]), float.Parse(p_t[1]), float.Parse(p_t[2]));
                rot = new Quaternion(float.Parse(p_r[0]), float.Parse(p_r[1]), float.Parse(p_r[2]), float.Parse(p_r[3]));

                apbb.outputChildTransforms[i].position = pos;
                apbb.outputChildTransforms[i].rotation = rot;
            }

            if (!globalRewind)
            {
                //move them to a designated spot
                apbb.outputChildTransforms[0].localPosition = Vector3.zero;
                apbb.outputChildTransforms[0].localEulerAngles = Vector3.zero;
            }
        }
        catch(IndexOutOfRangeException e)
        {
            return;
        }
    }




    private IEnumerator RecordRoutine()
    {
        Debug.Log("RECORDING ... BEGIN");

        float time = 0;
        float fraction = 1000 / recordFPS;
        while (recording)
        {
            time += Time.deltaTime * 1000;
            if (time >= fraction)
            {
                WriteFaceData();
                WriteBodyData();
                time = 0;
            }
            yield return new WaitForEndOfFrame();
        }

        recording = false;

        Debug.Log("RECORDING ... STOP");
    }

    /// <summary>
    /// Write the face data to a file
    /// </summary>
    /// <param name="timeStamp"></param>
    private void WriteFaceData()
    {
        string str = "";
        for (var expressionIndex = 0;
                expressionIndex < (int)OVRFaceExpressions.FaceExpression.Max;
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
        System.IO.File.AppendAllText(filePath_face, str);
    }


    // Method to retrieve the positions and rotations of all child objects
    private void WriteBodyData()
    {
        string str = "";

        for (int i = 0; i < bodySync.inputChildTransforms.Length; i++) //Should we use INPUT or OUTPUT? Check latency later.
        {
            str += bodySync.inputChildTransforms[i].position.x + "," + 
                bodySync.inputChildTransforms[i].position.y + "," +
                bodySync.inputChildTransforms[i].position.z;
            str += "|";
            str += bodySync.inputChildTransforms[i].rotation.x + "," +
                bodySync.inputChildTransforms[i].rotation.y + "," +
                bodySync.inputChildTransforms[i].rotation.z + "," +
                bodySync.inputChildTransforms[i].rotation.w + ";";
        }
        str += "\n";
        System.IO.File.AppendAllText(filePath_body, str);
    }


}
