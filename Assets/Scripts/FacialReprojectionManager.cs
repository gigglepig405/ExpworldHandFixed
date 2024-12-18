using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacialReprojectionManager : MonoBehaviour
{
    [Header("1: Hand-Surface | 2: Hand-Panel | 3: Obj-Surface | 4: Obj-Panel | 5: Hand+Obj-Surface")]
    public int condition;
    public bool startReprojection;
    bool hasInited;

    GameObject facialPlayer;
    GameObject projectPlayer;


    GameObject decalFacialCam;
    GameObject handLProj;
    GameObject handRProj;

    GameObject handLPanel;
    GameObject handRPanel;

    // Start is called before the first frame update
    void Start()
    {
        decalFacialCam = this.transform.GetChild(0).gameObject;
        handLProj = this.transform.GetChild(1).gameObject;
        handRProj = this.transform.GetChild(2).gameObject;
        handLPanel = this.transform.GetChild(3).gameObject;
        handRPanel = this.transform.GetChild(4).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (startReprojection)
        {
            startReprojection = false;
            ApplyReprojection();
        }
        
    }

    public void initFacial()
    {
        if (!hasInited)
        {
            hasInited = true;
            startReprojection=true;
        }
    }


    void ApplyReprojection()
    {
        /**
        facialPlayer = GameObject.FindGameObjectWithTag("Puppet");

        Transform[] children = facialPlayer.transform.GetComponentsInChildren<Transform>();
        foreach (var child in children)
        {
            if (child.name == "FacialCamTarget")
            {
                decalFacialCam.transform.parent = child;
                break;
            }
        }

        projectPlayer = GameObject.FindGameObjectWithTag("Main");

        children = projectPlayer.transform.GetComponentsInChildren<Transform>();
        foreach (var child in children)
        {
            if (child.name == "HandProjectorL")
            {
                handLProj.transform.parent = child;
                handLPanel.transform.GetComponent<BillboardManager>().InitBillboard(child.gameObject);
                break;
            }
        }

        children = projectPlayer.transform.GetComponentsInChildren<Transform>();
        foreach (var child in children)
        {
            if (child.name == "HandProjectorR")
            {
                handRProj.transform.parent = child;
                handRPanel.transform.GetComponent<BillboardManager>().InitBillboard(child.gameObject);
                break;
            }
        }

        decalFacialCam.transform.localPosition = Vector3.zero;
        decalFacialCam.transform.localEulerAngles = Vector3.zero;
        */

        if (condition == 1 || condition == 5)
        {
            handLProj.transform.localPosition = Vector3.zero;
            handLProj.transform.localEulerAngles = Vector3.zero;

            handRProj.transform.localPosition = Vector3.zero;
            handRProj.transform.localEulerAngles = Vector3.zero;

            handLProj.SetActive(true);
            handRProj.SetActive(true);

           // handLPanel.SetActive(false);
          //  handRPanel.SetActive(false);
            
        }

        else if (condition == 2)
        {

            handRPanel.transform.localPosition = Vector3.zero;
            handRPanel.transform.localEulerAngles = Vector3.zero;

            handRPanel.transform.localPosition = Vector3.zero;
            handRPanel.transform.localEulerAngles = Vector3.zero;

            handLProj.SetActive(false);
            handRProj.SetActive(false);

           // handLPanel.SetActive(true);
           // handRPanel.SetActive(true);

        }
        else
        {
           // handLPanel.SetActive(false);
           // handRPanel.SetActive(false);
            handLProj.SetActive(false);
            handRProj.SetActive(false);
        }

        GameObject.Find("TaskObjs").transform.GetComponent<TaskCubeManager>().InitAllBB();

    }
}
