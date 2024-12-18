using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModeSwitcherBehaviour : MonoBehaviour
{
    public bool isEmpMirror;
    // Start is called before the first frame update
    void Start()
    {
        if (GameObject.Find("TaskManager").GetComponent<TaskManagerScript>().useEmpMirror)
        {
            if (!isEmpMirror)
            {
                this.gameObject.SetActive(false);
            }
        }
        else
        {
            if (isEmpMirror)
            {
                this.gameObject.SetActive(false);
            }
        }
    }
}
