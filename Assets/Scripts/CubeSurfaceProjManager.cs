using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeSurfaceProjManager : MonoBehaviour
{
    GameObject grabCube;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(grabCube != null)
        {
            //this.transform.position = grabCube.transform.position;
            //this.transform.eulerAngles = grabCube.transform.eulerAngles + new Vector3(90,0,0);
        }
        else
        {
            //this.transform.position = new Vector3(999, 999, 999);
        }
    }

    public void setCube(GameObject go)
    {
        this.grabCube = go;

        this.transform.parent = go.transform.GetChild(0);
        this.transform.localPosition = Vector3.zero;

        this.transform.localEulerAngles = new Vector3(90, 0, 0);
        

    }

    public void unsetCube(GameObject go)
    {
        if(grabCube == go)
        {
            grabCube = null;

           //this.transform.parent
        }
    }

}