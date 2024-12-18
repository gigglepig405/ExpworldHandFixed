using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GazeTraceBehaviour : MonoBehaviour
{
    TaskManagerScript tms;
    public GameObject happMood, neuMood, angMood, sadMood;

    // Start is called before the first frame update
    void Start()
    {
        tms = GameObject.Find("TaskManager").GetComponent<TaskManagerScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (tms.self_emo == "Happy")
        {
            GameObject.Instantiate(happMood, this.transform.position, Quaternion.identity);
        }
        else if (tms.self_emo == "Angry")
        {
            GameObject.Instantiate(angMood, this.transform.position, Quaternion.identity);
        }
        else if (tms.self_emo == "Sad")
        {
            GameObject.Instantiate(sadMood, this.transform.position, Quaternion.identity);
        }
        else
        {
            GameObject.Instantiate(neuMood, this.transform.position, Quaternion.identity);

        }
    }
}
