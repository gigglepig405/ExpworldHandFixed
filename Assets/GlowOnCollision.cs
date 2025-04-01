using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowOnCollision : MonoBehaviour
{
    [Header("SetLightColor")]
    public Color emissionColor = Color.red; 
    public float emissionIntensity = 2.0f; 

    private Material mat;

    private void Start()
    {

        mat = GetComponent<Renderer>().material;
      
        mat.DisableKeyword("_EMISSION");
    }

    private void OnCollisionEnter(Collision collision)
    {
  
        if (collision.gameObject.CompareTag("Cube"))
        {
      
            mat.EnableKeyword("_EMISSION");
       
            mat.SetColor("_EmissionColor", emissionColor * emissionIntensity);
        }
    }
}

