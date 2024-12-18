using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandPoseBehaviour : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //need to update incase one of two hands become no OK
    public void oksign_activate()
    {
        GameObject.Find("RDHead").GetComponent<RealityDropBehaviour>().updateOkSign(true, this.gameObject);
        //print("OK sign..");
    }

    public void oksign_deactivate()
    {
        GameObject.Find("RDHead").GetComponent<RealityDropBehaviour>().updateOkSign(false, this.gameObject);
        //print("OK sign dispel..");
    }
}
