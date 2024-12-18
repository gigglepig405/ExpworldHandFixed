using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class AvatarEmotionSyncDB : MonoBehaviourPunCallbacks
{
    [SerializeField] 
    private float[] floatArray; // The array of floats to synchronize

    private bool isMasterClient;

    private void Start()
    {
        isMasterClient = PhotonNetwork.IsMasterClient;

        if (isMasterClient)
        {
            // Initialize and modify the floatArray on the master client
            floatArray = new float[63]; 
        }
    }

    private void Update()
    {
        // Check if this is the sub-client (non-master client)
        if (isMasterClient)
        {
            Debug.Log("On Master...");
        }
        else
        {
            Debug.Log("Update list...");

        }
    }

    // This method is automatically called when a network synchronization update occurs
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && isMasterClient)
        {
            // If this is the master client, send the floatArray to other players
            stream.SendNext(floatArray);
        }
        else if (!stream.IsWriting)
        {
            // If this is a non-master client, receive the floatArray from the network
            floatArray = (float[])stream.ReceiveNext();
        }
    }
}