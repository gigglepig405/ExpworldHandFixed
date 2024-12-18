using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EmpathicMirrorBehaviour : MonoBehaviour
{

    public GameObject portalGroup;
    public GameObject portB; //for StackedMirror mode only

    GameObject puppetHead;
    GameObject playerHead;

    GameObject headBall;
    TaskManagerScript tm;

    public bool updateMirrorStatus = true;

    //for some visual effect to reduce motionsick?
    public GameObject portalEffect;

    bool mirrorReady = false;

    private void Start()
    {
        tm = GameObject.Find("TaskManager").GetComponent<TaskManagerScript>();
        headBall = GameObject.Find("HeadDirectionTarget");
    }

    // Update is called once per frame
    void Update()
    {
        if (!mirrorReady)
        {   //Launch 2 player mirror if detected player
            if (GameObject.Find("MainPlayer") != null && GameObject.Find("PuppetPlayer") != null)
            {
                playerHead = GameObject.Find("HeadRefPoint_OffsetNormal");
                puppetHead = GameObject.Find("PuppetHeadInv");
                print("Empathic mirror");
                mirrorReady = true;
            }

            //Launch single player mirror if detected debug mode
            else if (GameObject.Find("DEBUG_PLAYER") != null)
            {
                playerHead = GameObject.Find("HeadRefPoint_OffsetNormal");
                puppetHead = GameObject.Find("DebugHeadTrajectory");
                print("Debug mirror");
                mirrorReady = true;
            }
        }

        //5 = Dom; 6 = Share; 7 = Sub; 8 = Overlay
        else if (mirrorReady && tm.useEmpMirror)
        {
            //init code & handler for cond change
            if (updateMirrorStatus)
            {
                initMirror(); //reset variables whenever conditions been changed/updated
            }

            //Empathic Mirror behaviour changes depend on the tm.COND

            //Do we need to adjust mirror height?
            if (tm.useEmpMirror)
            {
                this.transform.position = headBall.transform.position;

                //To disregard Y-axis
                Vector3 playerTarget = new Vector3(playerHead.transform.position.x, this.transform.position.y, playerHead.transform.position.z);
                Vector3 puppetTarget = new Vector3(puppetHead.transform.position.x, this.transform.position.y, puppetHead.transform.position.z);

                this.transform.LookAt(playerTarget);
                this.transform.LookAt(Vector3.Lerp(playerTarget, puppetTarget, 0.5f));
            }
        }
    }

    public void enableMirrorUpdate()
    {

        if (tm.useEmpMirror)
        {
            portalGroup.SetActive(true);
        }
    }

    public void disableMirrorUpdate()
    {
        portalGroup.SetActive(false);
    }

    void initMirror()
    {
        portalGroup.SetActive(true);

        if (tm.useEmpMirror)
        {
            portB.transform.parent = portalGroup.transform;
            portB.transform.localPosition = new Vector3(0, 0, 0.25f);
            portB.transform.localEulerAngles = Vector3.zero;

            portalGroup.transform.GetChild(0).transform.localScale = new Vector3(4, 3, 1);
            portalEffect.SetActive(false);
        }
        /*
        else if (tm.cond == TaskManagerScript.Conditions.BGMirror)
        {
            portalGroup.transform.GetChild(0).transform.localScale = new Vector3(2 , 2, 1);

            portB.transform.parent = null;
            portalEffect.SetActive(true);
        }*/

        else
        {
            //maybe move the portal elsewhere?
            portalGroup.SetActive(false);
            portalGroup.transform.position = new Vector3(999, 999, 999);
        }

        updateMirrorStatus = false;
    }



    float CalculateAngleFromAtoBFromC(Vector2 positionA, Vector2 positionB, Vector2 positionC)
    {
        Vector2 vectorCA = positionA - positionC;
        Vector2 vectorCB = positionB - positionC;

        // Calculate the angles of vectors CA and CB
        float angleCA = Mathf.Atan2(vectorCA.y, vectorCA.x) * Mathf.Rad2Deg;
        float angleCB = Mathf.Atan2(vectorCB.y, vectorCB.x) * Mathf.Rad2Deg;

        // Calculate the angle from A to B based on C
        float angleAtoBfromC = angleCB - angleCA;

        return angleAtoBfromC;
    }










}
