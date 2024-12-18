using System.Collections;
using UnityEngine;
using static OVRInput;

[System.Serializable]

public class EmotionalHapticPlayer : MonoBehaviour
{  
    [SerializeField]
    private EmotionHelper emotionHelper;
    public EmotionHelper.EmotionType CurrentEmotion;
    [Header("Heart rate & fluctuation values")]
    public float BPM = 60;
    public float fluctuationInterval = 2;
    [Header("Neutral BPM Range")]
    public float neutralMinBPM = 60f;
    public float neutralMaxBPM = 80f;
    public float neutralAmpMultiplier = .5f;
    [Header("Sadness BPM Range")]
    public float sadnessMinBPM = 60f;
    public float sadnessMaxBPM = 70f;
    public float sadnessAmpMultiplier = .2f;

    [Header("Anger BPM Range")]
    public float angerMinBPM = 80f;
    public float angerMaxBPM = 120f;
    public float angerAmpMultiplier = 1f;

    [Header("Happiness BPM Range")]
    public float happinessMinBPM = 70f;
    public float happinessMaxBPM = 100f;
    public float happinessAmpMultiplier = .7f;

    [Header("Surprise BPM Range")]
    public float surpriseMinBPM = 80f;
    public float surpriseMaxBPM = 110f;
    public float surpriseDurationBeforeRecovery = 5f;
    public float surpriseAmpMultiplier = .85f;

    HapticsAmplitudeEnvelopeVibration heartbeatEffect;
    private Coroutine heartbeatCoroutine;
    private Coroutine bpmFluctuationCoroutine;

    void Start()
    {
        heartbeatEffect = new HapticsAmplitudeEnvelopeVibration();
        SetEmotion(EmotionHelper.EmotionType.Neutral); // Default emotion
        StartHeartbeat();
        emotionHelper.onEmotionChange.AddListener( emotion => SetEmotion(emotion.Type));
    }

    float GetAmplitudeMultiplier()
    {
        switch (CurrentEmotion)
        {
            case EmotionHelper.EmotionType.Neutral:
                return neutralAmpMultiplier; // Moderate amplitude
            case EmotionHelper.EmotionType.Sad:
                return sadnessAmpMultiplier; // Weak amplitude
            case EmotionHelper.EmotionType.Angry:
                return 1.0f; // Strong amplitude
            case EmotionHelper.EmotionType.Happy:
                return angerAmpMultiplier; // Medium-strong amplitude
            case EmotionHelper.EmotionType.Surprise:
                return surpriseAmpMultiplier; // Medium amplitude with spikes
            default:
                return 0.5f; // Default to moderate amplitude
        }
    }

    void SetHeartbeatBPM(float bpm)
    {
        BPM = bpm;
        float beatDuration = 60.0f / BPM; // Calculate the duration of one beat in seconds
        float cycleDuration = 60.0f / BPM; // Full cycle for "lub-dub"
        int sampleRate = 100; // Number of samples per second
        heartbeatEffect.SamplesCount = (int)(sampleRate * cycleDuration);
        heartbeatEffect.Samples = new float[heartbeatEffect.SamplesCount];

        // Fill the samples array to create a more realistic heartbeat effect
        for (int i = 0; i < heartbeatEffect.SamplesCount; i++)
        {
            float time = (float)i / sampleRate;
            if (time < 0.1f * beatDuration) // Strong beat (lub) attack phase
            {
                heartbeatEffect.Samples[i] = GetAmplitudeMultiplier() * (time / (0.1f * beatDuration));
            }
            else if (time < 0.2f * beatDuration) // Strong beat (lub) decay phase
            {
                heartbeatEffect.Samples[i] = GetAmplitudeMultiplier() * (1.0f - (time - 0.1f * beatDuration) / (0.1f * beatDuration));
            }
            else if (time < 0.3f * beatDuration) // Brief pause
            {
                heartbeatEffect.Samples[i] = 0.0f;
            }
            else if (time < 0.35f * beatDuration) // Weak beat (dub) attack phase
            {
                heartbeatEffect.Samples[i] = GetAmplitudeMultiplier() * ((time - 0.3f * beatDuration) / (0.05f * beatDuration));
            }
            else if (time < 0.4f * beatDuration) // Weak beat (dub) decay phase
            {
                heartbeatEffect.Samples[i] = GetAmplitudeMultiplier() * (1.0f - (time - 0.35f * beatDuration) / (0.05f * beatDuration));
            }
            else // Rest phase
            {
                heartbeatEffect.Samples[i] = 0.0f;
            }
        }

        heartbeatEffect.Duration = cycleDuration; // Set the duration for one cycle
    }


    public void StartHeartbeat()
    {
        StopHeartbeat();
        heartbeatCoroutine = StartCoroutine(PlayHeartbeatEffect());
    }

    public void StopHeartbeat()
    {
        if (heartbeatCoroutine != null)
            StopCoroutine(heartbeatCoroutine);
    }

    private IEnumerator PlayHeartbeatEffect()
    {
        while (true)
        {
            // Play the haptic effect
            SetControllerHapticsAmplitudeEnvelope(heartbeatEffect, Controller.All);
            yield return new WaitForSeconds(heartbeatEffect.Duration);
        }
    }

    private IEnumerator FluctuateBPM(float minBPM, float maxBPM)
    {
        while (true)
        {
            float newBPM = Random.Range(minBPM, maxBPM);
            SetHeartbeatBPM(newBPM);
            yield return new WaitForSeconds(fluctuationInterval); // Adjust the frequency of BPM changes as needed
        }
    }

    public void SetEmotion(EmotionHelper.EmotionType emotion)
    {
        if (emotion == CurrentEmotion) return;

        CurrentEmotion = emotion;

        if (bpmFluctuationCoroutine != null)
            StopCoroutine(bpmFluctuationCoroutine);

        switch (emotion)
        {
            case EmotionHelper.EmotionType.Neutral:
                bpmFluctuationCoroutine = StartCoroutine(FluctuateBPM(neutralMinBPM, neutralMaxBPM));
                break;
            case EmotionHelper.EmotionType.Sad:
                bpmFluctuationCoroutine = StartCoroutine(FluctuateBPM(sadnessMinBPM, sadnessMaxBPM));
                break;
            case EmotionHelper.EmotionType.Angry:
                bpmFluctuationCoroutine = StartCoroutine(FluctuateBPM(angerMinBPM, angerMaxBPM));
                break;
            case EmotionHelper.EmotionType.Happy:
                bpmFluctuationCoroutine = StartCoroutine(FluctuateBPM(happinessMinBPM, happinessMaxBPM));
                break;
            case EmotionHelper.EmotionType.Surprise:
                heartbeatCoroutine = StartCoroutine(PlaySurpriseEffect());
                break;
        }
    }

    private IEnumerator PlaySurpriseEffect()
    {
        // Heart rate spike for surprise
        bpmFluctuationCoroutine = StartCoroutine(FluctuateBPM(surpriseMinBPM, surpriseMaxBPM));

        yield return new WaitForSeconds(heartbeatEffect.Duration + surpriseDurationBeforeRecovery);

        // Recover to neutral heart rate
        SetEmotion(EmotionHelper.EmotionType.Neutral);
    }
}
