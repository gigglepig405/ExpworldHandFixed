using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RecordAsyncInteractor : MonoBehaviour
{
    public TextMeshPro label;

    ASyncManagerScript asm;

    int mode; //1 = record; 2 = stop record
    // Start is called before the first frame update
    void Start()
    {
        mode = 1;
        asm = GameObject.Find("AsyncManager").GetComponent<ASyncManagerScript>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "quizFinger")
        {
            if(mode == 1)
            {
                mode = 2;
                label.text = "Recording...";
                asm.startRecording = true;
            }

            else if(mode == 2)
            {
                mode = 1;
                label.text = "Record";

                asm.stopRecording = true;
            }

        }
    }

}
