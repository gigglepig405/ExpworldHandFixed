using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaybackAsyncInteractor : MonoBehaviour
{
    ASyncManagerScript asm;
    public bool isGlobal = false;

    // Start is called before the first frame update
    void Start()
    {
        asm = GameObject.Find("AsyncManager").GetComponent<ASyncManagerScript>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "quizFinger")
        {
            asm.globalRewind = isGlobal;


            asm.startRewind = true;

        }
    }
}
