using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class quizInputManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /**
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "quizFinger")
        {
            if (int.Parse(this.name.Split('-')[0]) == 1)
            {
                GameObject.Find("TurnInteractor").GetComponent<TurnInteractorManager>().q1Ans = int.Parse(this.name.Split('-')[1]);
                this.transform.parent.parent.parent.parent.parent.parent.parent.gameObject.SetActive(false);
                GameObject.Find("TurnInteractor").GetComponent<TurnInteractorManager>().submitPressed();
            }
            else
            {
                GameObject.Find("TurnInteractor").GetComponent<TurnInteractorManager>().q2Ans = int.Parse(this.name.Split('-')[1]);
                this.transform.parent.parent.parent.parent.parent.parent.parent.gameObject.SetActive(false);
                GameObject.Find("TurnInteractor").GetComponent<TurnInteractorManager>().submitPressed();

            }
       
    }
    }
    */
}
