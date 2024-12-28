using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Grabber : MonoBehaviour
{
    public Transform grabPoint; 
    public float grabRange = 5.0f; 
    private GrabbableObject grabbedObject;

    void Update()
    {
        if (Input.GetButtonDown("Fire1")) 
        {
            TryGrab();
        }

        if (Input.GetButtonUp("Fire1") && grabbedObject != null)
        {
            Release();
        }
    }

    void TryGrab()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, grabRange))
        {
            GrabbableObject grabbable = hit.collider.GetComponent<GrabbableObject>();
            if (grabbable != null)
            {
                grabbedObject = grabbable;
                grabbedObject.Grab(grabPoint);
            }
        }
    }

    void Release()
    {
        if (grabbedObject != null)
        {
            grabbedObject.Release();
            grabbedObject = null;
        }
    }
}
