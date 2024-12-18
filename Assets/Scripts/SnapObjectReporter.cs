using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapObjectReporter : MonoBehaviour
{

    public enum Goal
    {
        Brewery,
        Church,
        CityHall,
        FerrisWheel,
        GreenField,
        House,
        Pizzeria,
        PostOffice,
        TennisCourt
    }

    public Goal targetObject;

    //if correct object, 1; else 0.
    public int score = 0;
    int prevScore = 0; //trigger for calling ANNOUNCER to UPDATE TEXT SCORE for efficiency


    public bool canSnap = true;

    private void Update()
    {
        if(prevScore != score)
        {
            transform.parent.GetComponent<SnapObjectAnnouncer>().UpdateScore();
            prevScore = score;
        }
    }


    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "TaskCube")
        {
            canSnap = false;

            if(other.gameObject.name == targetObject.ToString())
            {
                score = 1;
            }
            else
            {
                score = 0;
            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "TaskCube")
        {
            canSnap = true;
            score = 0;
        }
    }
}
