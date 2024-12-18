using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThematicSceneManager : MonoBehaviour
{
    public enum Theme
    {
        Normal, Horror
    }

    public Theme currentTheme;
    // Start is called before the first frame update
    void Start()
    {
        switch (currentTheme)
        {
            case Theme.Normal:
                disableObjByTheme(1);
                break;

            case Theme.Horror:
                disableObjByTheme(0);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void disableObjByTheme(int index)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            for(int j = 0; j < transform.GetChild(i).childCount; j++)
            {
                transform.GetChild(i).GetChild(j).GetChild(index).gameObject.SetActive(false);
            }
        }
    }
}
