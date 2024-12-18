using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardManager : MonoBehaviour
{
    private Camera mainCamera;

  //  public GameObject targetObj;

  //  bool startBillBoard = false;


    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        //if (startBillBoard)
       // {
            //transform.position = new Vector3(targetObj.transform.position.x, targetObj.transform.position.y+0.2f, targetObj.transform.position.z);
            transform.LookAt(mainCamera.transform.position);
            //transform.LookAt(mainCamera.transform.rotation * Vector3.forward,
   // mainCamera.transform.rotation * Vector3.up);



      //  }
    }
    /*
    public void InitBillboard(GameObject obj)
    {
        mainCamera = Camera.main;
        startBillBoard = true;

        targetObj = obj;
    }*/
}
