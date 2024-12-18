using Metaface.Debug;
using Metaface.Utilities;
using Photon.Pun;
using ShimmeringUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using TMPro;

public class TurnInteractorManager : MonoBehaviourPunCallbacks
{
    /**
    int goNext; //only 2 lead to next process
    public GameObject questionnairePanel;

    public int q1Ans = -1; //ref by studydatalogger
    public int q2Ans = -1;

    public GameObject q1UI;
    public GameObject q2UI;

    public int currentTurn;

    public GameObject buttonp1;
    public GameObject buttonp2;

    public float timeRemaining = 30f;
    bool startcd = false;
    public TextMeshPro timerTxt;

    public GameObject taskCompleteUI;


    public TextMeshPro taskStartTxt;

    float taskStartTime = 10f;
    public bool startTask5sec;


    private void Update()
    {
        if (startTask5sec) //for first 5 sec wait before trial start
        {
            if(taskStartTime > 0)
            {
                taskStartTime -= Time.deltaTime;
                int roundTime = (int)taskStartTime;
                taskStartTxt.text = "Task will begin in " + roundTime.ToString();
            }
            else
            {
                startTask5sec = false;
                taskStartTxt.text = "";
                currentTurn = 0;
                UpdateUI();
            }
        }


        if (startcd) //for countdown in each trial
        {
            if(timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                int roundTime = (int)timeRemaining;
                timerTxt.text = roundTime.ToString();
            }
            else
            {
                nextButtonPressed();
            }
        }
    }

    public void startTask()
    {
        startTask5sec = true;
    }


    [PunRPC]
    public void showQuestionnaire()
    {
        if (startcd)
        {
            startcd = false;
            timerTxt.text = "";
        }

        buttonp1.SetActive(false);
        buttonp2.SetActive(false);


        q1Ans = -1;
        q2Ans = -1;

        questionnairePanel.SetActive(true);

    }


    [PunRPC]
    public void UpdateTurnUI()
    {
        goNext++;
        if (goNext == 2)
        {
            currentTurn++;
            goNext = 0;
            UpdateUI();
        }
        
    }


    void UpdateUI()
    {
        if (currentTurn % 2 == 1) //here for DESKTOP user
        {
            buttonp1.SetActive(false);
            buttonp2.SetActive(true);
        }
        else //here for LAPTOP user
        {
            buttonp1.SetActive(true);
            buttonp2.SetActive(false);

            //move this down for one user.
            timeRemaining = 30;
            startcd = true;
        }


    }

    public void nextButtonPressed()
    {
        photonView.RPC("showQuestionnaire", RpcTarget.All);
    }


    public void submitPressed()
    {
        if(q1Ans != -1 && q2Ans != -1)
        {
            if(currentTurn >= 21)
            {
                taskCompleteUI.SetActive(true);
                
                GameObject.Find("DataLogger").GetComponent<StudyDataLogger>().isLogging = false;
                print("All done! Stopped recording");
                return;
            }


            //submit here
            photonView.RPC("UpdateTurnUI", RpcTarget.All); //need to move this to til questionnaire submitted

            q1UI.SetActive(true);
            q2UI.SetActive(true);

            questionnairePanel.SetActive(false);
        }
    }

    /**
 //Q1
    public void q1a1()
    {
        q1Ans = 1;  q1UI.SetActive(false); submitPressed();
    }
    public void q1a2()
    {
        q1Ans = 2;  q1UI.SetActive(false); submitPressed();
    }
    public void q1a3()
    {
        q1Ans = 3;  q1UI.SetActive(false); submitPressed();
    }
    public void q1a4()
    {
        q1Ans = 4;  q1UI.SetActive(false); submitPressed();
    }
    public void q1a5()
    {
        q1Ans = 5;  q1UI.SetActive(false); submitPressed();
    }

//Q2
    public void q2a1()
    {
        q2Ans = 1;  q2UI.SetActive(false); submitPressed();
    }
    public void q2a2()
    {
        q2Ans = 2;  q2UI.SetActive(false); submitPressed();
    }
    public void q2a3()
    {
        q2Ans = 3;  q2UI.SetActive(false); submitPressed();
    }
    public void q2a4()
    {
        q2Ans = 4;  q2UI.SetActive(false); submitPressed();
    }
    public void q2a5()
    {
        q2Ans = 5;  q2UI.SetActive(false); submitPressed();
    }
    **/
}
