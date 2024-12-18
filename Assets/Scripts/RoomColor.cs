using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomColor : MonoBehaviour
{
    [SerializeField]
    private List<MeshRenderer> meshRenderers;
    
    [SerializeField]
    private EmotionHelper emotionHelper;

    private Coroutine waitRoutine = null;

    private void Start()
    {
        //listen to emotion helper on emotion change and set the color to be a color associate with the emotion type
        emotionHelper.onEmotionChange.AddListener(OnEmotionChanged);
    }

    //The OnEmotionChanged function
    private void OnEmotionChanged(EmotionHelper.Emotion emotion)
    {
        if (emotion != null)
        {
            if (waitRoutine != null)
            {
                StopCoroutine(waitRoutine);
                waitRoutine = null;
            }
            //if there is an emotion, set the color to be the color associated with the emotion type
            SetColor(emotion.Color);
        }
        else
        {
            //if there is no emotion, set the color to be white
            if (waitRoutine != null)
            {
                StopCoroutine(waitRoutine);
                waitRoutine = null;
            }
            waitRoutine = StartCoroutine(WaitForFunction());
        }
    }

    IEnumerator WaitForFunction()
    {
        yield return new WaitForSeconds(2);
        SetColor(Color.gray);
        //Debug.Log("color set to gray");
    }


    //sets the color of the mesh renders with color param
    public void SetColor(Color color)
    {
        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            meshRenderer.material.color = color;
            //Debug.Log("color set to " + color);
        }
    }

}
