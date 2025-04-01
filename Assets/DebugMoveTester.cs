using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction.HandGrab;
using Oculus.Interaction;
using UnityEngine;

public class DebugMoveTester : MonoBehaviour
{
    public MoveTowardsTargetProvider provider;
    public Transform targetTransform;

    private IMovement _movement;

    void Start()
    {
        _movement = provider.CreateMovement();

        _movement.StopAndSetPose(new Pose(transform.position, transform.rotation));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
 
            _movement.MoveTo(new Pose(targetTransform.position, targetTransform.rotation));
        }
    
        _movement.Tick();
    }
}
