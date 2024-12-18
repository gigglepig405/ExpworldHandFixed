using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModeButtonBehaviour : MonoBehaviour
{
    public Material activeMat;
    public Material inactiveMat;

    bool isActive;

    public enum ButtonType
    {
        Emoji, Effect, Face, Hand, Obj, Self, Partner, Dom, Stc, Sub 
    }

    public ButtonType buttonType;

    TaskManagerScript tm;

    private void Start()
    {
        tm = GameObject.Find("TaskManager").GetComponent<TaskManagerScript>();
        if (transform.parent.GetComponent<ModeSwitcherBehaviour>().isEmpMirror)
        {
            if(buttonType == ButtonType.Dom)
            {
                isActive = true;
                transform.GetChild(0).GetComponent<Renderer>().material = activeMat;
                tm.UpdateModeFromButton((int)buttonType, true);
            }
            else
            {
                transform.GetChild(0).GetComponent<Renderer>().material = inactiveMat;
            }
        }
        else
        {
            if(buttonType == ButtonType.Self || buttonType == ButtonType.Hand)
            {
                isActive = true;
                transform.GetChild(0).GetComponent<Renderer>().material = activeMat;
                //GameObject.Find("SelfMode").GetComponent<ModeButtonBehaviour>().disactivateButton();
                tm.UpdateModeFromButton((int)buttonType, true);
            }
            else
            {
                transform.GetChild(0).GetComponent<Renderer>().material = inactiveMat;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "quizFinger")
        {
            //Emoji, Eff, Face, Hand, Obj
            if((int)buttonType < 5)
            {
                isActive = !isActive;

                if (isActive)
                {
                    transform.GetChild(0).GetComponent<Renderer>().material = activeMat;
                    tm.UpdateModeFromButton((int)buttonType, true);
                }
                else
                {
                    tm.UpdateModeFromButton((int)buttonType, false);
                    transform.GetChild(0).GetComponent<Renderer>().material = inactiveMat;
                }
            }
            // Self <-> Partner
            else if((int)buttonType < 7)
            {
                if (!isActive)
                {
                    //SELF
                    if((int)buttonType  == 5)
                    {
                        isActive = true;
                        transform.GetChild(0).GetComponent<Renderer>().material = activeMat;
                        GameObject.Find("PartnerMode").GetComponent<ModeButtonBehaviour>().disactivateButton();
                        tm.UpdateModeFromButton((int)buttonType, true);

                    }
                    //PARTNER
                    else
                    {
                        isActive = true;
                        transform.GetChild(0).GetComponent<Renderer>().material = activeMat;
                        GameObject.Find("SelfMode").GetComponent<ModeButtonBehaviour>().disactivateButton();
                        tm.UpdateModeFromButton((int)buttonType, true);

                    }
                }
            }
            //Dom <- Stc -> Sub
            else
            {
                if (!isActive)
                {
                    //SELF
                    if ((int)buttonType == 7)
                    {
                        isActive = true;
                        transform.GetChild(0).GetComponent<Renderer>().material = activeMat;
                        GameObject.Find("StcMode").GetComponent<ModeButtonBehaviour>().disactivateButton();
                        GameObject.Find("SubMode").GetComponent<ModeButtonBehaviour>().disactivateButton();
                        tm.UpdateModeFromButton((int)buttonType, true);

                    }
                    else if ((int)buttonType == 8)
                    {
                        isActive = true;
                        transform.GetChild(0).GetComponent<Renderer>().material = activeMat;
                        GameObject.Find("DomMode").GetComponent<ModeButtonBehaviour>().disactivateButton();
                        GameObject.Find("SubMode").GetComponent<ModeButtonBehaviour>().disactivateButton();
                        tm.UpdateModeFromButton((int)buttonType, true);

                    }
                    //PARTNER
                    else
                    {
                        isActive = true;
                        transform.GetChild(0).GetComponent<Renderer>().material = activeMat;
                        GameObject.Find("DomMode").GetComponent<ModeButtonBehaviour>().disactivateButton();
                        GameObject.Find("StcMode").GetComponent<ModeButtonBehaviour>().disactivateButton();
                        tm.UpdateModeFromButton((int)buttonType, true);

                    }
                }
            }
        }
    }

    public void disactivateButton()
    {
        this.isActive = false;
        transform.GetChild(0).GetComponent<Renderer>().material = inactiveMat;
    }
}
