using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskCubeManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitAllBB()
    {
        foreach (Transform child in transform)
        {
            child.GetChild(1).GetComponent<ObjBillboardManager>().InitObjBill();
        }
    }
}
