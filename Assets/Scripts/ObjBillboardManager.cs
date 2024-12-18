using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjBillboardManager : MonoBehaviour
{

    private Camera mainCamera;

    bool startBillBoard = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        if (startBillBoard)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
    mainCamera.transform.rotation * Vector3.up);
        }
    }

    public void InitObjBill()
    {
        mainCamera = Camera.main;
        startBillBoard = true;
    }
}
