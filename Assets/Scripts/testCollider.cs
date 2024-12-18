using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testCollider : MonoBehaviour
{
    public GameObject ball;
    public LayerMask hitMask;

    public bool unX;

    TaskManagerScript tm;

    Transform partnerTarget;
    GameObject shrTarget;

    bool init = false;

    Transform staticPos;

    private void Start()
    {
        ball = GameObject.Find("HeadDirectionTarget");
        tm = GameObject.Find("TaskManager").GetComponent<TaskManagerScript>();

        if (tm.useEmpMirror)
        {
            staticPos = GameObject.Find("StaticMirrorPos").transform;
        }
    }

    public void initMirrorHead()
    {
        if (GameObject.Find("PuppetPlayer") != null)
        {
            partnerTarget = GameObject.Find("PartnerHeadRefPoint").transform;
        }
        else
        {
            partnerTarget = GameObject.Find("DebugHeadTrajectory").transform;
        }

        shrTarget = GameObject.Find("ShrHeadPoint");
        init = true;
    }


    // Update is called once per frame
    void Update()
    {
        if (init)
        {
            if (tm.currentMirrorMode == TaskManagerScript.MirrorType.LocalDominant)
            {
                this.transform.eulerAngles = new Vector3(0, this.transform.eulerAngles.y, 0);

                // Cast a ray from the pointer's position in its forward direction
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.forward * 10, out hit, float.PositiveInfinity, hitMask))
                {
                    // Check if the raycast hit the plane
                    if (hit.transform.gameObject.name.Contains("EmpBarrier"))
                    {
                        ball.transform.position = hit.point;
                    }
                }
            }

            else if (tm.currentMirrorMode == TaskManagerScript.MirrorType.PartnerDominant)
            {
                partnerTarget.eulerAngles = new Vector3(0, partnerTarget.eulerAngles.y, 0);

                // Cast a ray from the pointer's position in its forward direction
                RaycastHit hit;
                if (Physics.Raycast(partnerTarget.position, partnerTarget.forward * 10, out hit, float.PositiveInfinity, hitMask))
                {
                    // Check if the raycast hit the plane
                    if (hit.transform.gameObject.name.Contains("EmpBarrier"))
                    {
                        ball.transform.position = hit.point;
                    }
                }
            }

            else if (tm.currentMirrorMode == TaskManagerScript.MirrorType.Static)
            {
                this.transform.eulerAngles = new Vector3(0, this.transform.eulerAngles.y, 0);
                partnerTarget.eulerAngles = new Vector3(0, partnerTarget.eulerAngles.y, 0);

                ball.transform.position = staticPos.transform.position;       
                
            }

            /*
            else if (tm.cond == TaskManagerScript.Conditions.BGMirror)
            {
                ball.transform.position = transform.position + transform.forward * 0.75f;
            }
            */
            else
            {
                ball.transform.position = new Vector3(999, 999, 999);
            }
        }
    }
}
