using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LocalEmotionIndicator : MonoBehaviour
{
    TaskManagerScript tm;
    public TextMeshPro tmpro;
    // Start is called before the first frame update
    void Start()
    {
        tm = FindObjectOfType(typeof(TaskManagerScript)) as TaskManagerScript;

    }

    // Update is called once per frame
    void Update()
    {
        tmpro.text = tm.self_emo;
    }
}
