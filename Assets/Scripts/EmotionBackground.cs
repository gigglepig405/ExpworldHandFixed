using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EmotionBackground;
using static EmotionHelper;

public class EmotionBackground : MonoBehaviour
{

    [SerializeField]
    private EmotionHelper emotionHelper;

    [SerializeField]
    private List<EmotionObject> emotionObjects = new List<EmotionObject>();

    [System.Serializable]
    public class EmotionObject
    {
        [SerializeField]
        private EmotionHelper.EmotionType type;
        public EmotionHelper.EmotionType Type => type;

        [SerializeField]
        public GameObject emojiGO;

        [SerializeField]
        public GameObject effectGO;
    }

    bool doneInit = false;
    TaskManagerScript tm;
    GameObject targetProjector;

    public void UpdateEmoAttachment()
    {
        if (!doneInit)
        {
            emotionHelper.onEmotionChange.AddListener(OnEmotionChanged);
            tm = GameObject.Find("TaskManager").GetComponent<TaskManagerScript>();
            doneInit = true;
        }

        if (tm.onSelf)
        {

            targetProjector = GameObject.Find("HandProjectorR");

            this.transform.parent = targetProjector.transform;
            this.transform.localPosition = Vector3.zero;
            this.transform.localEulerAngles = new Vector3(0, 180f, 0);
        }
        else if (tm.onPartner)
        {
            targetProjector = GameObject.Find("PuppetRH");
            this.transform.parent = targetProjector.transform;
            this.transform.localPosition = Vector3.zero;
            this.transform.localEulerAngles = new Vector3(0, 180f, 0);
        }
        else //move it else where without hiding cause it processes emotion value
        {
            this.transform.parent = null;
            this.transform.localPosition = new Vector3(999f,999f,999f);
        }
    }

    public void UpdatePartnerEmotionChanged(string newEmotion)
    {
        foreach (EmotionObject emotionObject in emotionObjects)
        {
            if(tm.useEmoji)
                emotionObject.emojiGO.SetActive(newEmotion == emotionObject.Type.ToString());
            if(tm.useEff)
                emotionObject.effectGO.SetActive(newEmotion == emotionObject.Type.ToString());

        }
    }

    private void OnEmotionChanged(EmotionHelper.Emotion emotion)
    {
        if (emotion != null)
        {
            tm.UpdateEmotions(emotion.Type.ToString());
   
        }
    }

}
