using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChangerScript : MonoBehaviour
{
    public GameObject[] colorList;
    int currentIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateColor()
    {
        currentIndex++;

        if (currentIndex == 3) currentIndex = 0;

        for(int i =0; i<3; i++)
        {
            if (i == currentIndex) { colorList[i].SetActive(true); }
            else colorList[i].SetActive(false);
        }
    }
}
