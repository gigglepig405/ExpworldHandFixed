using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollidCheck : MonoBehaviour
{
    public bool isComplete;


    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.tag == "TaskCube")
        {
            isComplete = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "TaskCube")
        {
            isComplete = false;
        }
    }
}
