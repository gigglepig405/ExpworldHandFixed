using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectEmotionManager : MonoBehaviour
{
    GameObject[] emojiMood;
    GameObject happMood, neuMood, angMood, sadMood, surMood ;

    GameObject[] effMood;
    GameObject happyEff, neuEff, angEff, sadEff, surEff;
    bool checkForEmo = false;
    PhotonView pv;
    GameObject cubeSnap;


    TaskManagerScript tms;
    // Start is called before the first frame update
    void Start()
    {
        pv = GetComponent<PhotonView>();

        happMood = transform.GetChild(0).gameObject;
        neuMood = transform.GetChild(1).gameObject;
        angMood = transform.GetChild(2).gameObject;
        sadMood = transform.GetChild(3).gameObject;
        surMood = transform.GetChild(4).gameObject;

        emojiMood =new GameObject[] { happMood, neuMood, angMood, sadMood, surMood };

        happyEff = transform.GetChild(5).gameObject;
        neuEff = transform.GetChild(6).gameObject;
        angEff = transform.GetChild(7).gameObject;
        sadEff = transform.GetChild(8).gameObject;
        surEff = transform.GetChild(9).gameObject;

        effMood = new GameObject[] { happyEff, neuEff, angEff, sadEff, surEff };

        for (int i = 0; i < emojiMood.Length; i++)
        {
            emojiMood[i].SetActive(false);
            effMood[i].SetActive(false);
        }


        tms = GameObject.Find("TaskManager").GetComponent<TaskManagerScript>();

    }

    // Update is called once per frame
    void Update()
    {
        if (checkForEmo)
        {
            if (tms.partner_emo == "Happy")
            {
                UpdateEmotionByIndex(0);
            }
            else if (tms.partner_emo == "Angry")
            {
                UpdateEmotionByIndex(2);
            }
            else if (tms.partner_emo == "Sad")
            {
                UpdateEmotionByIndex(3);
            }
            else if (tms.partner_emo == "Surprise")
            {
                UpdateEmotionByIndex(4);
            }
            else
            {
                UpdateEmotionByIndex(1);
            }
        }
    }

    void UpdateEmotionByIndex(int index)
    {

        for (int i = 0; i < emojiMood.Length; i++)
        {
            if(i == index)
            {
                if(tms.useEmoji)  emojiMood[i].SetActive(true);
                if(tms.useEff)    effMood[i].SetActive(true);
            }
            else
            {
                emojiMood[i].SetActive(false);
                effMood[i].SetActive(false);
            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.tag == "CubeSnapper")
        {
            if (other.GetComponent<SnapObjectReporter>().canSnap) //only apply snap if there isnt any cube on it
                cubeSnap = other.gameObject;
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == cubeSnap)
        {
            cubeSnap = null;
        }
    }

    //when user grabs this object
    public void ApplyUserEmo()
    {
        pv.RequestOwnership();


/*        if(tms.onObj && tms.onPartner) //IF COND == OBJ_PARTNER, DO NETWORK UPDATE
        {
            print(gameObject.name + " OEM called network update");
            tms.UpdateObjectBehaviour(gameObject, true);
            return;
        }
        if (tms.onObj && tms.onSelf)
        {
            //ELSE DONT NEED UPDATE NETWORK
            tms.UpdateObjectBehaviour(gameObject, false);
        }*/

        tms.ApplyObjectEmotion(gameObject);

    }

    public void ApplySnapIfExist()
    {
        if (cubeSnap)
        {
            this.transform.position = cubeSnap.transform.position;
            this.transform.eulerAngles = new Vector3(0, 0f, 0);
        }
    }

    public void set3DHead()
    {
        GameObject.Find("3DHead").transform.GetComponent<HeadProjectorManager>().setCube(this.gameObject);
    }


    public void enableEmotion()
    {
        checkForEmo = true;

        print("ObjectEmotionManager.enableEmotion called on " + this.gameObject.name);
    }

    public void disableEmotion()
    {
        checkForEmo = false;
        for (int i = 0; i < emojiMood.Length; i++)
        {
            emojiMood[i].SetActive(false);
            effMood[i].SetActive(false);
        }
    }
}
