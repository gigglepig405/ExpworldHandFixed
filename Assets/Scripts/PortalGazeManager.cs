using Metaface.Utilities;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;

public class PortalGazeManager : MonoBehaviour
{
    //EmpMirror became larger when gazed / smaller when lose focus
    public bool enlargeOnGaze = false;
    //EmpMirror become larger as the user moves towards the window. Mutually exclusive with enlargeOnGaze
    public bool gogoSizeOnGaze = false;
    //EmpMirror become clear when gazed / transparent when lose focus
    //public bool opaqueOnGaze = false;
    //EmpMirror fixed to world when gazed / stitch to head when lose focus
    public bool fixOnGaze = false;


    //MODE FixOnGaze: PortalA is independent and PortalB do not move by head positions
    //MODE StitchToHead: PortalA always stitch to the user's head and PortalB move by head positions

    bool doneInit = false;
    MidEyeGazeHelper gzhelper;

    GameObject headRef;
    bool firstLOCK;
    Vector3 headLOCKRef; //position recorded first frame on gaze for PLAYER HEAD
    Vector3 portalLOCKRef; //position recorded first frame on gaze for PORTAL
    float headPortalDistanceOnGaze; //recorded distance between portal and head on first frame user gazes on it

    GameObject portalHeadTrajectory;
    GameObject portalA;

    private void Start()
    {
        portalA = GameObject.Find("PortalA");
    }

    // Update is called once per frame
    void Update()
    {
        if (doneInit)
        {
            if (gzhelper.focusOBJ == "PortalGaze") //GAZING PORTAL YES
            {
                if (!fixOnGaze) //StitchToHead MODE
                {

                    if (!firstLOCK) //when looking at portal for first time, do inits
                    {
                        headLOCKRef = headRef.transform.localPosition;
                        portalLOCKRef = portalA.transform.position;
                        headPortalDistanceOnGaze = Vector3.Distance(headRef.transform.position, portalA.transform.position);
                        firstLOCK = true;
                    }
                    else //done inits and now constant update
                    {
                        Vector3 tempVect = new Vector3(headRef.transform.localPosition.x - headLOCKRef.x,
                            headRef.transform.localPosition.y - headLOCKRef.y, 0);
                        portalHeadTrajectory.transform.localPosition = tempVect * 0.025f;
                    }
                }
                else // FixOnGaze MODE
                {
                    if (!firstLOCK) //when looking at portal for first time, do inits
                    {
                        headLOCKRef = headRef.transform.localPosition;
                        portalLOCKRef = portalA.transform.position;
                        headPortalDistanceOnGaze = Vector3.Distance(headRef.transform.position, portalA.transform.position);
                        firstLOCK = true;
                    }

                    portalA.transform.parent = null; //window became part of the world 
                }

                if (enlargeOnGaze && !gogoSizeOnGaze) //portal become larger at fixed size regardless of user distance
                {
                    portalA.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                }
                else if(!enlargeOnGaze && gogoSizeOnGaze) //Portal became larger as the user lean closer to it while gazing
                {
                    float distanceDiff = headPortalDistanceOnGaze - Vector3.Distance(portalLOCKRef, headRef.transform.position);
                    portalA.transform.localScale = new Vector3(0.2f + distanceDiff, 0.2f + distanceDiff, 0.2f + distanceDiff );
                }


            }





            else //reset everything to normal when not looking at it GAZING PORTAL NO
            {
                if (fixOnGaze)
                {
                    portalA.transform.parent = GameObject.Find("MainPlayerHeadRefPoint").transform;
                    portalA.transform.localPosition = new Vector3(0, 0, -0.2f);
                    portalA.transform.localEulerAngles = Vector3.zero;
                }
                firstLOCK = false;
                portalHeadTrajectory.transform.localPosition = Vector3.zero;

                portalA.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                
            }







        }
    }

    public void initGaze() //CALL AFTER AVATAR SPAWN
    {
        gzhelper = GameObject.Find("EyeGazeDebugger").GetComponent<MidEyeGazeHelper>();
        headRef = GameObject.Find("MainPlayerHeadRefPoint").transform.parent.gameObject;
        portalHeadTrajectory = GameObject.Find("PortalHeadTrajectory");
        doneInit = true;
    }
}
