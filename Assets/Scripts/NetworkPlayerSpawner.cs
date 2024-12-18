using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkPlayerSpawner : MonoBehaviourPunCallbacks
{
    GameObject mirrorPlayerPrefab;
    public bool debugMode;
    public bool pcMode; 

    Transform[] spawnPoints;

    private void Start()
    {
        spawnPoints = new Transform[] { this.transform.GetChild(0), this.transform.GetChild(1) };
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        print("Joined player id: " + PhotonNetwork.LocalPlayer.ActorNumber + " from " + Application.platform);

        if (debugMode)
        {
            mirrorPlayerPrefab = PhotonNetwork.Instantiate("DebugNetworkPlayer", spawnPoints[PhotonNetwork.LocalPlayer.ActorNumber - 1].position, spawnPoints[PhotonNetwork.LocalPlayer.ActorNumber - 1].rotation);
            mirrorPlayerPrefab.name = "DEBUG_PLAYER";
        }
        else if (pcMode)
        {
            mirrorPlayerPrefab = PhotonNetwork.Instantiate("PCNetworkPlayer", spawnPoints[PhotonNetwork.LocalPlayer.ActorNumber - 1].position, spawnPoints[PhotonNetwork.LocalPlayer.ActorNumber - 1].rotation);
            mirrorPlayerPrefab.name = "DEBUG_PC_PLAYER";
        }

        else
        {

            mirrorPlayerPrefab = PhotonNetwork.Instantiate("NetworkPlayer", spawnPoints[PhotonNetwork.LocalPlayer.ActorNumber-1].position, spawnPoints[PhotonNetwork.LocalPlayer.ActorNumber-1].rotation);
            mirrorPlayerPrefab.name = "P" + PhotonNetwork.CurrentRoom.PlayerCount + "_PLAYER";
            PhotonNetwork.LocalPlayer.NickName = mirrorPlayerPrefab.name;
        }
    }

}
