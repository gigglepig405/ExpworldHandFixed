using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealityDropBehaviour : MonoBehaviour
{
    bool isOKsign = false;
    GameObject targetHand;

    bool canUpdate;
    bool spawnedPortal; // this is to prevent double spawn because of various collider on a hand

    public GameObject portalPrefab;
    GameObject portalCopy;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void updateOkSign(bool res, GameObject hand)
    {
        if (!res) { 
            isOKsign = false; 
            targetHand = null;

            if (spawnedPortal) // if user is holding a spawnedPortal..
            {
                //detach it from the hand permanently
                portalCopy.transform.parent = null;
                portalCopy = null;

                spawnedPortal = false;  
            }

            
            return; 
        
        }

        if (canUpdate)
        {
            targetHand = hand;
            isOKsign = true;
            return;
        }
    }

    void spawnPortalO()
    {
        if (!spawnedPortal)
        {
            spawnedPortal = true;
            print("Spawn portal..");

            GameObject portalZ  = Instantiate(portalPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            //detach and attach the projection to the partner
            portalZ.transform.GetChild(1).transform.position = GameObject.Find("ProjectionPortal").transform.position;
            portalZ.transform.GetChild(1).transform.eulerAngles = GameObject.Find("ProjectionPortal").transform.eulerAngles;
            portalZ.transform.GetChild(1).transform.parent = GameObject.Find("ProjectionPortal").transform;

            portalCopy = portalZ.transform.GetChild(0).gameObject;

            if (targetHand.name.Contains("Left"))
            {
                portalCopy.transform.position = GameObject.Find("LHThumbPoint").transform.position;
                portalCopy.transform.eulerAngles = GameObject.Find("LHThumbPoint").transform.eulerAngles;
                portalCopy.transform.parent = GameObject.Find("LHThumbPoint").transform;
            }
            else
            {
                portalCopy.transform.position = GameObject.Find("RHThumbPoint").transform.position;
                portalCopy.transform.eulerAngles = GameObject.Find("RHThumbPoint").transform.eulerAngles;
                portalCopy.transform.parent = GameObject.Find("RHThumbPoint").transform;
            }

        }

    }

    

    private void OnTriggerEnter(Collider other)
    {
        canUpdate = true;
    }

    private void OnTriggerExit(Collider other)
    {
        //duplicate portal if user exit this trigger with a OK sign
        //cannot spawn portal if a user is already holding a portal
        if (isOKsign && portalCopy == null)
        {
            spawnPortalO();
        }

        canUpdate = false;
    }
}
