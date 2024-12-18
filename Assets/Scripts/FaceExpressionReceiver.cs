using System.Collections;
using UnityEngine;

public class FaceExpressionReceiver : MonoBehaviour
{

    [Header("References")]

    [SerializeField]
    private FacePlaybackSystem facePlaybackSystem;


    [SerializeField, Range(1, 60)]
    private float recordFPS = 30f;

    private bool isPlayback = false;

    public bool startPlayback;

    public NetworkExpressionDatabase fdb;


    private void Update()
    {
        if (startPlayback)
        {
            startPlayback = false;
            isPlayback = true;

            StartCoroutine(PlaybackRoutine());
        }
    }


    private void StopPlayback()
    {
        if (!isPlayback)
            return;

        isPlayback = false;
    }


    private IEnumerator PlaybackRoutine()
    {

        float time = 0;
        float fraction = 1000 / recordFPS;

        int lineIdx = 0;
        while (isPlayback)
        {
            time += Time.deltaTime * 1000;
            if (time >= fraction)
            {
                facePlaybackSystem?.ApplyFaceWeight(fdb.facialParameters);
                time = 0;
                lineIdx++;
            }
            yield return new WaitForEndOfFrame();
        }

    }

}
