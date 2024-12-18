using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskLevelSpawner : MonoBehaviour
{
    public GameObject[] instList;
    public GameObject[] snapperList;

    public void initTaskEnv(int taskN)
    {
        //ACTIVATE task by different task requirements
        instList[taskN].SetActive(true);
        snapperList[taskN].SetActive(true);
    }

}
