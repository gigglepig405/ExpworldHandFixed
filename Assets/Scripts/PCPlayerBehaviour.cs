using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCPlayerBehaviour : MonoBehaviourPunCallbacks
{
    public GameObject mainPlayer, puppetPlayer;
    

    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine)
        {
            mainPlayer.SetActive(true);

        }
        else
        {
            puppetPlayer.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
