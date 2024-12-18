using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ObjectProjectionManager : MonoBehaviour
{
    /**
    //public CubeSurfaceProjManager cm;
    

    PhotonView pv;

    TaskManagerScript tm;

    GameObject cubeSnap;


    // Start is called before the first frame update
    void Start()
    {
        pv = GetComponent<PhotonView>();

        tm = GameObject.Find("TaskManager").GetComponent<TaskManagerScript>();

        //if(tm.TaskCondition != 2)
        //{
        //    cm.gameObject.SetActive(false);
        //}
    }

    private void OnTriggerEnter(Collider other)
    {

        if(other.tag == "CubeSnapper")
        {
            if(!other.GetComponent<CollidCheck>().isComplete) //only apply snap if there isnt any cube on it
                cubeSnap = other.gameObject;
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject == cubeSnap)
        {
            cubeSnap = null;
        }
    }

    public void ApplySnapIfExist()
    {
        if (cubeSnap)
        {
            this.transform.position = cubeSnap.transform.position;

            //add IF statement for player A or B to decide cube rotation side for 2D 3D face direction

            //TO VERIFY //maybe we wont need this anymore?
            this.transform.eulerAngles = new Vector3(0, 0f, 0);


            //if(PhotonNetwork.LocalPlayer.ActorNumber == 1)
            //{
                //    this.transform.eulerAngles = new Vector3(0, 270f, 0);

                // }
                // else
                // {
                //     this.transform.eulerAngles = new Vector3(0, 90f, 0);
                // }

        }
    }

    public void ObjProjectionON()
    {
        pv.RequestOwnership();


        if (tm.cond == TaskManagerScript.Conditions.FaceObject)
        {
            //will set to headpos automatically
            GameObject.Find("3DHead").transform.GetComponent<HeadProjectorManager>().setCube(this.gameObject);

        }

    }

    public void ObjProjectionOFF()
    {

        if (tm.cond == TaskManagerScript.Conditions.FaceObject)
            GameObject.Find("3DHead").transform.GetComponent<HeadProjectorManager>().unsetCube(this.gameObject);

    }
    */
}
