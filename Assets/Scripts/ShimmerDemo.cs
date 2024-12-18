using System.Collections;
using System.Collections.Generic;
using System.Drawing.Text;
using OVR;
using ShimmeringUnity;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;

public class ShimmerDemo : MonoBehaviour
{
    [SerializeField]
    private ShimmerDataLogger shimmerDataLogger;

    [SerializeField]
    private ShimmerHeartRateMonitor ShimmerHeartRateMonitor;

    [SerializeField]
    private Transform shimmerLayout;

    [SerializeField]
    private shimmerEvents shimmerComponentPrefab;

    //The component for the heartRate if it has been referenced
    private shimmerEvents heartRateComponent;

    //List of components for all of the signals in the data logger
    private Dictionary<ShimmerDataLogger.Signal, shimmerEvents> createdComponents
        = new Dictionary<ShimmerDataLogger.Signal, shimmerEvents>();

    void Start()
    {
        if (ShimmerHeartRateMonitor)
        {
            //Creat the heartrate prefab
            heartRateComponent = Instantiate(shimmerComponentPrefab);
            heartRateComponent.transform.SetParent(shimmerLayout, false);
            heartRateComponent.EventName = "Heart Rate: ";
            heartRateComponent.EventValue = "";
        }

        //loop through signals and create compnents from the prefab and place them into the shimmer layout
        foreach (var signal in shimmerDataLogger.Signals)
        {
            shimmerEvents comp = Instantiate(shimmerComponentPrefab);
            comp.transform.SetParent(shimmerLayout, false);
            comp.EventName = signal.Name.ToString();
            comp.EventValue = "";
            createdComponents.Add(signal, comp);
        }

        shimmerComponentPrefab.gameObject.SetActive(false);
    }

    void Update()
    {
        if (ShimmerHeartRateMonitor && heartRateComponent)
        {
            heartRateComponent.EventValue = ShimmerHeartRateMonitor.HeartRate.ToString();
        }

        //loop throught createdComponents and update the created components via their singal references
        //and set the text value to be the new signal value
        foreach (var kvp in createdComponents)
        {
            kvp.Value.EventValue = kvp.Key.Value;
        }
    }

}
