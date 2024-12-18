using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkPickUp : MonoBehaviour
{
    PhotonView pv;

    // Start is called before the first frame update
    void Start()
    {
        pv = GetComponent<PhotonView>();


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void userPick()
    {
        pv.RequestOwnership();

    }
}
