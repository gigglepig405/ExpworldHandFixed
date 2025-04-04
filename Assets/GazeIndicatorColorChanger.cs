using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GazeIndicatorColorChanger : MonoBehaviour
{
  
    public Color targetColor = Color.red;

 
    private void OnTriggerEnter(Collider other)
    {
      
        if (other.CompareTag("TaskCube"))
        {
            Renderer rend = other.GetComponent<Renderer>();
            if (rend != null)
            {
          
                rend.material.color = targetColor;
            }
        }
    }
}
