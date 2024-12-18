using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Xml;

public class FurnitureTaskScoreManager : MonoBehaviour
{
    public GameObject p1t1, p2t1, p1t2, p2t2, p1t3, p2t3, p1tt, p2tt;

    public int p1ScoreMark;
    public int p2ScoreMark;

    TextMeshPro p1Score;
    TextMeshPro p2Score;

    Dictionary<string, int> p1Inv;
    Dictionary<string, int> P2Inv;

    string specialInv;
    string p1crit, p2crit;


    Dictionary<string, int> furnitureInv;
    bool performUpdate;

    /**
    // Start is called before the first frame update
    void Start()
    {
        TaskManagerScript tms = GameObject.Find("TaskManager").GetComponent<TaskManagerScript>();

        if(tms.getTaskSet() == 0)
        {
            p1Inv = new Dictionary<string, int>() {
            { "Cabinet1", 0 }, { "Bin1", 4 }, { "Table1", 1 }, { "Tree1", 4 },
            { "Juke1", 0 }, { "Chair1", 0 }, { "Table2", 4 }, { "Chair2", 2 } };

            P2Inv = new Dictionary<string, int>() {
            { "Cabinet1", 4 }, { "Bin1", 2 }, { "Table1", 0 }, { "Tree1", 0 },
            { "Juke1", 1 }, { "Chair1", 0 }, { "Table2", 4 }, { "Chair2", 4 } };

            specialInv = "Chair1";
            p1crit = "Table1";
            p2crit = "Juke1";

            p1Score = p1t1.GetComponent<TextMeshPro>();
            p2Score = p2t1.GetComponent<TextMeshPro>();

        }

        else if (tms.getTaskSet() == 1)
        {
            p1Inv = new Dictionary<string, int>() {
            { "Cabinet1", 0 }, { "Bin1", 4 }, { "Table1", 4 }, { "Tree1", 4 },
            { "Juke1", 0 }, { "Chair1", 1 }, { "Table2", 0 }, { "Chair2", 2 } };

            P2Inv = new Dictionary<string, int>() {
            { "Cabinet1", 0 }, { "Bin1", 4 }, { "Table1", 0 }, { "Tree1", 2 },
            { "Juke1", 4 }, { "Chair1", 0 }, { "Table2", 1 }, { "Chair2", 4 } };

            specialInv = "Cabinet1";
            p1crit = "Chair1";
            p2crit = "Table2";

            p1Score = p1t2.GetComponent<TextMeshPro>();
            p2Score = p2t2.GetComponent<TextMeshPro>();
        }

        else if (tms.getTaskSet() == 2)
        {
            p1Inv = new Dictionary<string, int>() {
            { "Cabinet1", 4 }, { "Bin1", 0 }, { "Table1", 0 }, { "Tree1", 0 },
            { "Juke1", 1 }, { "Chair1", 4 }, { "Table2", 4 }, { "Chair2", 2 } };

            P2Inv = new Dictionary<string, int>() {
            { "Cabinet1", 4 }, { "Bin1", 0 }, { "Table1", 1 }, { "Tree1", 4 },
            { "Juke1", 0 }, { "Chair1", 0 }, { "Table2", 2 }, { "Chair2", 4 } };

            specialInv = "Bin1";
            p1crit = "Juke1";
            p2crit = "Table1";

            p1Score = p1t3.GetComponent<TextMeshPro>();
            p2Score = p2t3.GetComponent<TextMeshPro>();
        }

        else
        {
            p1Inv = new Dictionary<string, int>() {
            { "Cabinet1", 0 }, { "Bin1", 1 }, { "Table1", 4 }, { "Tree1", 0 },
            { "Juke1", 0 }, { "Chair1", 0 }, { "Table2", 0 }, { "Chair2", 1 } };

            P2Inv = new Dictionary<string, int>() {
            { "Cabinet1", 0 }, { "Bin1", 1 }, { "Table1", 0 }, { "Tree1", 0 },
            { "Juke1", 4 }, { "Chair1", 1 }, { "Table2", 0 }, { "Chair2", 0 } };

            specialInv = "Tree1";
            p1crit = "Chair2";
            p2crit = "Chair1";

            p1Score = p1tt.GetComponent<TextMeshPro>();
            p2Score = p2tt.GetComponent<TextMeshPro>();
        }

        furnitureInv = new Dictionary<string, int>() { 
            { "Cabinet1", 0 }, { "Bin1", 0 }, { "Table1", 0 }, { "Tree1", 0 }, 
            { "Juke1", 0 }, { "Chair1", 0 }, { "Table2", 0 }, { "Chair2", 0 } };

        performUpdate = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (performUpdate)
        {
            int p1PreScore =0;
            int p2PreScore =0;
            //string dictOutput = "";

            foreach (var item in furnitureInv)
            {
                if (item.Key != specialInv)
                {
                    if(item.Key == p1crit || item.Key == p2crit)
                    {
                        p1PreScore += 3* (Mathf.Abs(p1Inv[item.Key] - item.Value));
                        p2PreScore += 3* (Mathf.Abs(P2Inv[item.Key] - item.Value));
                    }

                    else
                    {

                        p1PreScore += Mathf.Abs(p1Inv[item.Key] - item.Value);
                        p2PreScore += Mathf.Abs(P2Inv[item.Key] - item.Value);
                    }

                    //print("P1Pre: " + p1PreScore + " and P2Pre: " + p2PreScore);

                }

                //dictOutput += item.Key + "= " + item.Value + " ; ";
                
            }

            p1ScoreMark = (int)(100 - (p1PreScore * 3.57142857f));
            p1Score.text = p1ScoreMark + "%";
            p2ScoreMark = (int)(100 - (p2PreScore * 3.57142857f));
            p2Score.text = p2ScoreMark + "%";

            performUpdate = false;
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "TaskCube")
        {
            furnitureInv[other.name.Split("-")[1]] += 1;
            performUpdate = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "TaskCube")
        {
            furnitureInv[other.name.Split("-")[1]] -= 1;
            performUpdate = true;
        }
    }
*/

}
