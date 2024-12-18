using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AffectiveMirrorBehaviour : MonoBehaviour
{
    GameObject[] emojiMood;
    GameObject happMood, neuMood, angMood, sadMood, surMood;

    GameObject[] effMood;
    GameObject happyEff, neuEff, angEff, sadEff, surEff;

    TaskManagerScript tms;

    public bool updateMirrorEmo;

    // Start is called before the first frame update
    void Start()
    {
        happMood = transform.GetChild(0).gameObject;
        neuMood = transform.GetChild(1).gameObject;
        angMood = transform.GetChild(2).gameObject;
        sadMood = transform.GetChild(3).gameObject;
        surMood = transform.GetChild(4).gameObject;

        emojiMood = new GameObject[] { happMood, neuMood, angMood, sadMood, surMood };

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
        if (updateMirrorEmo)
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
            if (i == index)
            {
                if (tms.useEmoji) emojiMood[i].SetActive(true);
                if (tms.useEff) effMood[i].SetActive(true);
            }
            else
            {
                emojiMood[i].SetActive(false);
                effMood[i].SetActive(false);
            }
        }

        updateMirrorEmo = false;

    }


}
