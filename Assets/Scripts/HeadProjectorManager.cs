using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//PROBABLY NOT USING AT ALL!!
public class HeadProjectorManager : MonoBehaviour
{
    
    TaskManagerScript tm;

    public Transform selfHand, partnerHand;

    public GameObject grabCube;
    Camera myCam;

    bool hasInit = false;

    public bool isObjectHead;


    // Start is called before the first frame update
    void Start()
    {

        tm = GameObject.Find("TaskManager").GetComponent<TaskManagerScript>();

        //else //if(tm.cond == TaskManagerScript.Conditions.FaceObject) //we move this to the child of taskobj so we could update it according to requirement
       // {
            //this.transform.parent = GameObject.Find("TaskObjs").transform;
            // this.transform.position = Vector3.zero;
            // this.transform.eulerAngles = Vector3.zero;
       // }
    }

    // Update is called once per frame
    void Update()
    {
        if (tm.use3DHead)
        {
            if (!hasInit)
            {
                NetworkPlayerGestureSync[] goS = FindObjectsOfType<NetworkPlayerGestureSync>();

                selfHand = GameObject.Find("HandProjectorR").transform.transform;

                partnerHand = GameObject.Find("PuppetRH_Head").transform;
                hasInit = true;
            }


            //THIS SCRIPT IS NOW SHARE AMONG TWO HEADS (HAND HEAD and OBJ HEAD)
            if (isObjectHead && tm.onObj)
            {
                //Target (whether local or partner) is automated by grabCube so no worries
                if (this.grabCube != null)
                {
                    this.transform.parent = this.grabCube.transform.GetChild(10).transform;
                    this.transform.localPosition = Vector3.zero;

                    Vector3 direction = myCam.transform.position - this.transform.position;
                    direction.y = 0f;

                    if (direction != Vector3.zero)
                    {
                        Quaternion rotation = Quaternion.LookRotation(direction);
                        transform.rotation = rotation;
                    }

                    Vector3 tempEuler = this.transform.localEulerAngles;
                    this.transform.localEulerAngles = new Vector3(-90, tempEuler.y, 0);
                }
                
            }
            else if(!isObjectHead && tm.onHand)//on HAND 
            {


                if (tm.onSelf) // PROJECT TO SELF
                {

                    this.transform.position = selfHand.position;
                    this.transform.eulerAngles = selfHand.eulerAngles;
                }

                else if (tm.onPartner)
                {
                    this.transform.position = partnerHand.position;
                    this.transform.eulerAngles = partnerHand.eulerAngles + new Vector3(0, 0, 180);
                }   
            }
            else
            {
                this.transform.parent = null;
                this.gameObject.transform.position = new Vector3(999, 999, 999);
            }
        }
        else
        {
            this.transform.parent = null;
            this.gameObject.transform.position = new Vector3(999, 999, 999);
        }
    }



    public void setCube(GameObject go)
    {        
        this.grabCube = go;
        myCam = Camera.main;
    }

    public void unsetCube(GameObject go)
    {
        if (grabCube == go)
        {
            grabCube = null;
            this.transform.parent = go.transform.parent;
        }
    }
    
}
