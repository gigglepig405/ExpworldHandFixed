using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction.Samples;
using Oculus.Interaction.Input;

public class TaskManagerScript : MonoBehaviourPunCallbacks
{
    [Header("User Study Parameters")]
    //b = baseline, w = wallportal, s = stackedportal

    public bool use3DHead;
    public bool useEmoji;
    public bool useEff;
    bool prevUse3DHead, prevUseEmoji, prevUseEff;

    public bool onSelf;
    public bool onPartner;
    public bool onHand;
    public bool onObj;
    bool prevOnSelf, prevOnPartner, prevOnHand, prevOnObj;

    public bool useEmpMirror;
    public enum MirrorType { LocalDominant, Static, PartnerDominant }
    public MirrorType currentMirrorMode;
    //int prevMirrorMode = 0;
    /**
    public enum Conditions 
    {Baseline, 
        EmoSelfHand, EmoPartnerHand, EmoSelfObject, EmoPartnerObject
        ,DomMirror, ShrMirror, SubMirror, BGMirror
    }
    public Conditions cond;
    */
    public int taskNum; //0 = train, 1~5 main
    public bool SetUpTask;

    [Header("For Dev Use")]
    public string self_emo;
    public string partner_emo;


    public GameObject[] emoT_List;

    //FOR ExpHand2 USER STUDY OBJ
    GameObject selfPrevGrabGO;
    GameObject partnerPrevGrabGO;
    public GameObject taskScoreIndicator;
    TaskLevelSpawner taskSet;
    public GameObject modePanelXR;
   

    private void Start()
    {
        taskSet = GameObject.Find("TaskSets").GetComponent<TaskLevelSpawner>();

    }

    // Update is called once per frame
    void Update()
    {
        if(use3DHead != prevUse3DHead)
        {
            EmotionBackground eb = FindObjectOfType(typeof(EmotionBackground)) as EmotionBackground;
            eb.UpdateEmoAttachment();
            prevUse3DHead = use3DHead;
        }

        if(useEmoji != prevUseEmoji)
        {
            EmotionBackground eb = FindObjectOfType(typeof(EmotionBackground)) as EmotionBackground;
            eb.UpdateEmoAttachment();
            prevUseEmoji = useEmoji;
            if (onSelf) ApplyObjectEmotion(selfPrevGrabGO);
            else ApplyObjectEmotion(partnerPrevGrabGO);
        }
        if(useEff != prevUseEff)
        {
            EmotionBackground eb = FindObjectOfType(typeof(EmotionBackground)) as EmotionBackground;
            eb.UpdateEmoAttachment();
            prevUseEff = useEff;
            if (onSelf) ApplyObjectEmotion(selfPrevGrabGO);
            else ApplyObjectEmotion(partnerPrevGrabGO);
        }
        if (onHand != prevOnHand)
        {
            EmotionBackground eb = FindObjectOfType(typeof(EmotionBackground)) as EmotionBackground;
            eb.UpdateEmoAttachment();
            prevOnHand = onHand;
        }
        if (onObj != prevOnObj)
        {
            EmotionBackground eb = FindObjectOfType(typeof(EmotionBackground)) as EmotionBackground;
            eb.UpdateEmoAttachment();
            prevOnObj = onObj;
            if (onSelf) ApplyObjectEmotion(selfPrevGrabGO);
            else ApplyObjectEmotion(partnerPrevGrabGO);
        }
        if (onSelf != prevOnSelf)
        {
            EmotionBackground eb = FindObjectOfType(typeof(EmotionBackground)) as EmotionBackground;
            eb.UpdateEmoAttachment();
            prevOnSelf = onSelf;
            TransferHead(false);
        }
        if(onPartner != prevOnPartner)
        {
            EmotionBackground eb = FindObjectOfType(typeof(EmotionBackground)) as EmotionBackground;
            eb.UpdateEmoAttachment();
            prevOnPartner = onPartner;
            TransferHead(true);
        }


        if (SetUpTask)
        {
            SetUpTask = false;
            photonView.RPC("networkStartRecord", RpcTarget.All); //need to move this to til questionnaire submitted
        }
    }

    //Emoji, Effect, Face, Hand, Obj, Self, Partner, Dom, Stc, Sub
    public void UpdateModeFromButton(int buttonType, bool cond)
    {
        switch (buttonType)
        {
            case 0:
                useEmoji = cond;
                break;
            case 1:
                useEff = cond;
                break;
            case 2:
                use3DHead = cond;
                break;
            case 3: // FROM Xobj TO Xhand
                onHand = cond;
                break;
            case 4:  // FROM Xhand to Xobj
                onObj = cond;
                break;
            case 5: //FROM partner TO self 
                onPartner = false;
                prevOnPartner = false;
                onSelf = true;
                break;
            case 6: // FROM self TO partner
                onSelf = false;
                prevOnSelf = false;
                onPartner = true;
                break;
            case 7: //Dom <-  stc <- sub
                currentMirrorMode = MirrorType.LocalDominant;
                break;
            case 8:
                currentMirrorMode = MirrorType.Static;
                break;
            case 9:
                currentMirrorMode = MirrorType.PartnerDominant;
                break;
        }
    }

    [PunRPC]
    public void networkStartRecord()
    {

        //Start calibration
        NetworkPlayerBodySync[] playerRefs = FindObjectsOfType(typeof(NetworkPlayerBodySync)) as NetworkPlayerBodySync[];
        Debug.Log("Found " + playerRefs.Length + " NetworkPlayerBodySync instances on scene.");
        foreach (NetworkPlayerBodySync item in playerRefs)
        {
            item.calibPos = true;
        }

        //RESYNC Facial data in case its empty
        if (GameObject.Find("MainPlayer") != null)
        {
            GameObject.Find("MainPlayer").transform.parent.GetComponent<NetworkPlayerGestureSync>().startPlayback = true;
        }

        if (GameObject.Find("PuppetPlayer") != null)
        {
            GameObject.Find("PuppetPlayer").transform.parent.GetComponent<NetworkPlayerGestureSync>().startPlayback = true;
        }

        EmotionBackground eb = FindObjectOfType(typeof(EmotionBackground)) as EmotionBackground;
        eb.UpdateEmoAttachment();

        //Start emotion update on non empathic mirror condition
        if (!useEmpMirror)
        {
            //Spawn task level and environment
            taskSet.initTaskEnv(taskNum);
            modePanelXR.SetActive(true);

        }
        else
        {
            //empathic mirror init here
            testCollider tc = FindObjectOfType(typeof(testCollider)) as testCollider;
            tc.initMirrorHead();
        }

        //Start Datalog
        //GameObject.Find("DataLogger").GetComponent<StudyDataLogger>().InitiateLogger();

    }

//************* OBJECT BASED EMOTION START ***********

    //SOURCE: ObjectEmotionManager ONSELECT()
    //this decides WHERE emotions should visualise (OBJECT I PICK OR PARTNER PICK)
    public void ApplyObjectEmotion(GameObject newGO)
    {
        //inform other client for partner grab object
        photonView.RPC("UpdateNetworkObjEmo", RpcTarget.Others, newGO.name);

        //clear disable or clear prev obj effects
        if (selfPrevGrabGO != null)
        { //We cant disable a null object "first time"
            selfPrevGrabGO.GetComponent<ObjectEmotionManager>().disableEmotion(); //disable emoji since its the only multi-copy item
        }

        selfPrevGrabGO = newGO;

        if (onObj)
        {
            if (useEmoji && onSelf) selfPrevGrabGO.GetComponent<ObjectEmotionManager>().enableEmotion();
            if (use3DHead && onSelf) selfPrevGrabGO.GetComponent<ObjectEmotionManager>().set3DHead();
        }

    }

    //This method only gets called WHEN PARTNER PICKED an OBJ
    [PunRPC]
    public void UpdateNetworkObjEmo(string objName)
    {
        //clear disable or clear prev obj effects
        if (partnerPrevGrabGO != null)
        { //We cant disable a null object "first time"
            partnerPrevGrabGO.GetComponent<ObjectEmotionManager>().disableEmotion(); //disable emoji since its the only multi-copy item
        }

        partnerPrevGrabGO = GameObject.Find(objName);

        if (onObj)
        {
            if (useEmoji && onPartner) partnerPrevGrabGO.GetComponent<ObjectEmotionManager>().enableEmotion();
            if (use3DHead && onPartner) partnerPrevGrabGO.GetComponent<ObjectEmotionManager>().set3DHead();
        }
    }


    public void TransferHead(bool toPartner)
    {
        if (toPartner) //Disable emotions on object previously by LOCAL
        {
            if (selfPrevGrabGO != null)
            { //We cant disable a null object "first time"
                selfPrevGrabGO.GetComponent<ObjectEmotionManager>().disableEmotion(); //disable emoji since its the only multi-copy item
            }
            if (partnerPrevGrabGO != null)
            {
                if (useEmoji && onPartner && onObj) partnerPrevGrabGO.GetComponent<ObjectEmotionManager>().enableEmotion();
                if (use3DHead && onPartner && onObj) partnerPrevGrabGO.GetComponent<ObjectEmotionManager>().set3DHead();
            }
        }
        else //Disable emotions on object previously by PARTNER
        {
            if (partnerPrevGrabGO != null)
            {
                partnerPrevGrabGO.GetComponent<ObjectEmotionManager>().disableEmotion();
            }
            if (selfPrevGrabGO != null)
            {
                if (useEmoji && onSelf && onObj) selfPrevGrabGO.GetComponent<ObjectEmotionManager>().enableEmotion();
                if (use3DHead && onSelf && onObj) selfPrevGrabGO.GetComponent<ObjectEmotionManager>().set3DHead();
            }
        }
    }
    //****************** OBJECT BASED EMOTION END *****************



    //******** HAND BASED EMOTION START ***********

    //SOURCE: Allison EmotionHelper / EmotionBackground
    public void UpdateEmotions(string emo)
    {
        self_emo = emo;
        isInEmoSync();
        photonView.RPC("UpdateNetworkPartnerEmotion", RpcTarget.Others, emo);
    }

    [PunRPC]
    public void UpdateNetworkPartnerEmotion(string emo)
    {
        this.partner_emo = emo;
        isInEmoSync();
        //NEEDS to  be in network side because we don't update emotion on local emotion update
        //Start emotion update on COND 1 OR 2 (HAND)
        if (!useEmpMirror && onHand)
        {
            EmotionBackground eb = FindObjectOfType(typeof(EmotionBackground)) as EmotionBackground;
            eb.UpdatePartnerEmotionChanged(emo);
        }

        if (useEmpMirror)
        {
            GameObject.Find("AffectiveMirror").GetComponent<AffectiveMirrorBehaviour>().updateMirrorEmo = true;
        }
    }
//********** HAND BASED EMOTION END*********


    void isInEmoSync()
    {
        if (!useEmpMirror)
        {
            if (this.partner_emo == self_emo)
            {
                taskScoreIndicator.SetActive(true);
            }
            else
            {
                taskScoreIndicator.SetActive(false);
            }
        }
    }



}
