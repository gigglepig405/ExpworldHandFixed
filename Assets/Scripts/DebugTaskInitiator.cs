using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTaskInitiator : MonoBehaviour
{



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartTask()
    {
        GameObject.Find("TaskManager").GetComponent<TaskManagerScript>().SetUpTask = true;
        this.gameObject.SetActive(false);
    }
    
}
