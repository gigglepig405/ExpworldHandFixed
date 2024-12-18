using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SnapObjectAnnouncer : MonoBehaviour
{
    public TextMeshPro tmp;

    public void UpdateScore()
    {
        float total = 0;
        foreach (Transform child in transform)
        {
            total += child.GetComponent<SnapObjectReporter>().score;
        }
        total = Mathf.Ceil((total / 9) * 100);

        if(total < 25)
        {
            tmp.text = "<25%";
        }
        else if(total < 50)
        {
            tmp.text = "<50%";
        }
        else if(total < 75)
        {
            tmp.text = "<75%";
        }
        else if(total <100)
        {
            tmp.text = "ALMOST";
        }
        else
        {
            tmp.text = "TASK COMPLETED";
        }

        //tmp.text = total.ToString("F0") + "%";
    }
}
