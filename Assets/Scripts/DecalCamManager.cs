using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalCamManager : MonoBehaviour
{
    public GameObject decal1, decal2, decal3;
    GameObject gazeCollider;

    // Start is called before the first frame update
    void Start()
    {
        gazeCollider = GameObject.Find("GazeTrackBall");
    }

    // Update is called once per frame
    void Update()
    {
        decal1.SetActive(true);
        decal2.SetActive(true);
        decal3.SetActive(true);

        //decal1 > decal2
        if (Vector3.Distance(gazeCollider.transform.position, decal1.transform.position) > 
            Vector3.Distance(gazeCollider.transform.position, decal2.transform.position) ||
            Vector3.Distance(gazeCollider.transform.position, decal1.transform.position) >
            Vector3.Distance(gazeCollider.transform.position, decal3.transform.position))
        {
            decal1.SetActive(false);
        }

        //decal2 > decal3
        if(Vector3.Distance(gazeCollider.transform.position, decal2.transform.position) > 
            Vector3.Distance(gazeCollider.transform.position, decal3.transform.position) ||
            Vector3.Distance(gazeCollider.transform.position, decal2.transform.position) >
            Vector3.Distance(gazeCollider.transform.position, decal1.transform.position))
        {
            decal2.SetActive(false);
        }
        if(Vector3.Distance(gazeCollider.transform.position, decal3.transform.position) >
            Vector3.Distance(gazeCollider.transform.position, decal1.transform.position) ||
            Vector3.Distance(gazeCollider.transform.position, decal3.transform.position) >
            Vector3.Distance(gazeCollider.transform.position, decal2.transform.position))
        {
            decal3.SetActive(false);
        }

    }
}
