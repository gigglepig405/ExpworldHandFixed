using System.Collections;
using System.Collections.Generic;
using ShimmeringUnity;
using TMPro;
using UnityEngine;

public class shimmerEvents : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI nameText, valueText;

    public string EventName
    {
        set => nameText.text = value;
    }

    public string EventValue
    {
        set
        {
            valueText.text = value;
        }
    }

}
