using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Photon.Realtime;

/// <summary>
/// Records face express 
/// </summary>
public class NetworkExpressionDatabase : MonoBehaviourPunCallbacks
{

    [Header("Settings")]

    [SerializeField]
    private OVRFaceExpressions ovrFaceExpressions;

    [SerializeField]
    private FaceExpressionReceiver fep;

    [SerializeField, Range(1, 60)]
    private float recordFPS = 30f;

    //public bool facialTracker = false;

    public float[] facialParameters;

    bool isRecording = false;

    bool bootFaceSync = false;

    //QUEST USER MUST be MASTER and first user to join the game
    private void Start()
    {
        if (photonView.IsMine)
        {
            // Initialize and modify the array only on the local client
            facialParameters = new float[63];
            bootFaceSync = true;

            //fep.startPlayback = true; //enable facial expression immediately for Quest user
        }

    }

    private void Update()
    {
        if (photonView.IsMine && bootFaceSync)
        {
            isRecording = true;
            bootFaceSync = false;
            StartCoroutine(RecordRoutine());
        }
    }


    private IEnumerator RecordRoutine()
    {
        float time = 0;
        float fraction = 1000 / recordFPS;
        while (isRecording)
        {
            time += Time.deltaTime * 1000;
            if (time >= fraction)
            {
                UpdateFaceData();
                time = 0;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    private void UpdateFaceData()
    {

        for (var expressionIndex = 0; expressionIndex < (int)OVRFaceExpressions.FaceExpression.Max; //63 = max
                ++expressionIndex)
        {
            float weight;
            if (ovrFaceExpressions.TryGetFaceExpressionWeight((OVRFaceExpressions.FaceExpression)expressionIndex, out weight))
                facialParameters[expressionIndex] = weight;
            else
                facialParameters[expressionIndex] = 0;
        }

        photonView.RPC("UpdateFacialParameters", RpcTarget.Others, facialParameters);
    }

    [PunRPC]
    public void UpdateFacialParameters(float[] updatedParameters)
    {
        // Receive and apply the updated facialParameters from the network
        this.facialParameters = updatedParameters;
    }

    // This method is automatically called when a network synchronization update occurs
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && photonView.IsMine)
        {
            // If this is the local client, send the floatArray to other players
            stream.SendNext(facialParameters);
        }
        else if (!stream.IsWriting && !photonView.IsMine)
        {
            // If this is a non-master client, receive the floatArray from the network
            facialParameters = (float[])stream.ReceiveNext();
        }
    }
}