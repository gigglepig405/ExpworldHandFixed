using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TaskButtonHandler : MonoBehaviour
{
    public GameObject[] taskButtonList;
    public GameObject taskObject;

    float timerCountDown = 5;
    bool doingQuest = false;

    public TaskManagerScript tm;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(timerCountDown > 0)
        {
            timerCountDown -= Time.deltaTime;
        }

        if(timerCountDown < 0 && !doingQuest)
        {
            doingQuest = true;

            int index = Random.Range(0, taskButtonList.Length);
            taskButtonList[index].SetActive(true);
            taskObject.SetActive(false);
        }

    }

    public void ButtonTaskComplete()
    {
        taskObject.SetActive(true);
        timerCountDown = Random.Range(15, 20);
        doingQuest = false;

        //update any emotion in case was updated when button tasking
        //tm.applyBufferedEmotion();

    }
}
