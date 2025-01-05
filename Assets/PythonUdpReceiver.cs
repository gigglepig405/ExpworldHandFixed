using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Metaface.Utilities;  
using UnityEngine;

public class PythonUdpReceiver : MonoBehaviour
{
    private UdpClient udpClient;

    [Header("Listening Port")]
    public int listenPort = 12345;

    [Header("Reference to MidEyeGazeML script")]
    public MidEyeGazeML gazeML;
   

    void Start()
    {
       
        udpClient = new UdpClient(listenPort);
        udpClient.BeginReceive(ReceiveCallback, null);

        Debug.Log("[PythonUdpReceiver] Started listening on port: " + listenPort);
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
        byte[] data = udpClient.EndReceive(ar, ref remoteEP);
        string message = Encoding.UTF8.GetString(data);

        Debug.Log("[PythonUdpReceiver] Received: " + message);

        
        if (gazeML != null)
        {
            if (message.Contains("GAZE_ON"))
            {
                
                gazeML.SetGazeState(true);
            }
            else if (message.Contains("NON_GAZE"))
            {
                
                gazeML.SetGazeState(false);
            }
        }

        
        udpClient.BeginReceive(ReceiveCallback, null);
    }

    void OnDestroy()
    {
        
        if (udpClient != null)
        {
            udpClient.Close();
        }
    }
}


