using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonTaskBehaviour : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "quizFinger")
        {
            transform.parent.GetComponent<TaskButtonHandler>().ButtonTaskComplete();
            gameObject.SetActive(false);
        }
    }
}
