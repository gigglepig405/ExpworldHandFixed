using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GrabbableObject : MonoBehaviourPun, IPunObservable
{
    private bool isGrabbed = false;
    private Transform grabbedBy;

    void Update()
    {
        if (isGrabbed && grabbedBy != null)
        {
            transform.position = Vector3.Lerp(transform.position, grabbedBy.position, Time.deltaTime * 10f);
            transform.rotation = grabbedBy.rotation;
        }
    }

    public void Grab(Transform grabber)
    {
        isGrabbed = true;
        grabbedBy = grabber;
        photonView.RPC("SyncGrab", RpcTarget.Others, grabber.GetComponent<PhotonView>().ViewID);
    }

    public void Release()
    {
        isGrabbed = false;
        grabbedBy = null;
        photonView.RPC("SyncRelease", RpcTarget.Others);
    }

    [PunRPC]
    void SyncGrab(int grabberID)
    {
        PhotonView grabberView = PhotonView.Find(grabberID);
        grabbedBy = grabberView.transform;
        isGrabbed = true;
    }

    [PunRPC]
    void SyncRelease()
    {
        isGrabbed = false;
        grabbedBy = null;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
        }
    }
}