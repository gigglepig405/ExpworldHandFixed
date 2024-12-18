using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FaceExpressionPlayback : MonoBehaviour
{
    [Header("Playback")]
    [SerializeField]
    private TextAsset playbackRecording;
    [SerializeField]
    private AudioClip playbackAudio;

    [Header("References")]

    [SerializeField]
    private FacePlaybackSystem facePlaybackSystem;

    [SerializeField]
    private EmotionHelper emotionHelper;

    [SerializeField, Range(1, 60)]
    private float recordFPS = 60f;

    [SerializeField]
    private AudioSource audioSource;

    private bool isPlayback = false;

    [Header("UI")]

    [SerializeField]
    private Button startPlaybackButton;

    [SerializeField]
    private Button stopPlaybackButton;


    public bool startPlayback;

    void Awake()
    {
        startPlaybackButton.onClick.AddListener(StartPlayback);
        stopPlaybackButton.onClick.AddListener(StopPlayback);
    }

    private void Update()
    {
        if (startPlayback)
        {
            startPlayback = false;
            isPlayback = true;

            StartCoroutine(PlaybackRoutine());
        }
    }

    private void StartPlayback()
    {
        if (isPlayback)
            return;

        if (playbackRecording != null)
        {
            isPlayback = true;
            StartCoroutine(PlaybackRoutine());

        }
        /**
        if (playbackAudio != null)
        {
            //play the audio 
            audioSource.clip = playbackAudio;
            audioSource.Play();
        }
        */
    }
    
    private void StopPlayback()
    {
        if (!isPlayback)
            return;

        isPlayback = false;


    }


    private IEnumerator PlaybackRoutine()
    {
        Debug.Log("Starting playback");

        var lines = playbackRecording.text.Split("\n");

        float time = 0;
        float timeTotal = 0;
        float fraction = 1000 / recordFPS;

        int lineIdx = 0;
        while (isPlayback && lineIdx < lines.Length)
        {
            time += Time.deltaTime * 1000;
            timeTotal += Time.deltaTime;
            if (time >= fraction)
            {
                PlaybackFaceData(lines[lineIdx]);
                time = 0;
                lineIdx++;
            }
            yield return new WaitForEndOfFrame();
        }

        Debug.Log($"Playback complete in {timeTotal} seconds");

        isPlayback = false;
        /**
        if (audioSource && audioSource.isPlaying)
            audioSource.Stop();
        */
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
        emotionHelper?.OverrideWeightsWithValues(weights);
    }

}
