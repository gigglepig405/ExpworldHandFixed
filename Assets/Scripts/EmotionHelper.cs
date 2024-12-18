using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Movement.Effects;
using UnityEngine.Events;

public class EmotionHelper : MonoBehaviour
{
    [SerializeField]
    private OVRFaceExpressions faceManager;

    [SerializeField]
    private bool  disableOVRFaceExpressions = false;

    [SerializeField]
    private List<Emotion> emotions = new List<Emotion>();

    private Emotion currentEmotion = null;

    public EmotionEvent onEmotionChange = new EmotionEvent();

    [System.Serializable]
    public class Emotion
    {
        [SerializeField]
        private EmotionType type;
        public EmotionType Type => type;
        [SerializeField]
        private List<ExpressionWeight> expressions = new List<ExpressionWeight>();

        public List<ExpressionWeight> Expressions => expressions;

        [SerializeField]
        private Color color = Color.white;
        public Color Color => color;
    }

    public enum EmotionType
    {
        Happy,
        Angry,
        Sad,
        Surprise,
        Neutral
    }



    [System.Serializable]
    public class ExpressionWeight
    {
        [SerializeField]
        [InspectorName("expression")]
        private OVRFaceExpressions.FaceExpression expression;

        public OVRFaceExpressions.FaceExpression Expression => expression;

        // [SerializeField]
        // private float weighting;
        // public float Weighting => weighting;

        [SerializeField]
        private float min;
        public float Min => min;

        [SerializeField]
        private float max;
        public float Max => max;

    }

    public void OverrideWeightsWithValues(float[] weights)
    {
        this.weights = weights;

    }

    float[] weights = null;


    void Update()
    {
        foreach (Emotion e in emotions)
        {
            bool isEmotion = true;
            foreach (ExpressionWeight ew in e.Expressions)
            {
                if (weights != null && weights.Length > 0 && weights.Length == (int)OVRFaceExpressions.FaceExpression.Max)
                {
                    int index = (int)ew.Expression;
                    float weight = weights[index];
                    if (!(weight >= ew.Min && weight <= ew.Max))
                    {
                        isEmotion = false;
                    }
                }
                else
                {
                    float weight;
                    if (!disableOVRFaceExpressions && faceManager != null && faceManager.TryGetFaceExpressionWeight(ew.Expression, out weight))
                    {
                        if (!(weight >= ew.Min && weight <= ew.Max))
                        {
                            isEmotion = false;
                        }
                    }
                    else
                        isEmotion = false;
                }
            }

            if (isEmotion)
            {
                if (currentEmotion == null || e != currentEmotion)
                {
                    currentEmotion = e;
                    onEmotionChange.Invoke(e);
                }
            }

        }

        weights = null;
    }

    [System.Serializable]
    public class EmotionEvent : UnityEvent<Emotion>
    {

    }
}
