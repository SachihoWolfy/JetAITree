using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using System.Threading;

public class CameraBehavior : MonoBehaviour
{
    public List<AIAircraft> allAircraft;
    public CinemachineCamera cam;
    public CinemachineTargetGroup group;

    private bool backInput;
    private bool nextInput;
    public AIAircraft curTarget;
    private int targetIndex = 0;

    void Start()
    {
        allAircraft.AddRange(FindObjectsOfType<AIAircraft>());
        if (allAircraft.Count > 0)
        {
            curTarget = allAircraft[0];
            cam.Target.TrackingTarget = curTarget.transform;
            UpdateGroup();
        }
    }

    void Update()
    {
        UpdateGroup();
        if (cam.Target.TrackingTarget != curTarget.transform)
        {
            cam.Target.TrackingTarget = curTarget.transform;
        }

        UpdateInput();

        if (backInput)
        {
            PreviousTarget();
        }
        else if (nextInput)
        {
            NextTarget();
        }
    }

    void UpdateInput()
    {
        backInput = Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.Mouse1);
        nextInput = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.Mouse0);
    }

    void UpdateGroup()
    {
        group.Targets.Clear(); // Clear existing targets

        if (curTarget == null) return;

        HashSet<Transform> uniqueTargets = new HashSet<Transform>(); // Prevent duplicates

        // Add the currently tracked AI aircraft
        if (curTarget.transform != null)
        {
            group.Targets.Add(new CinemachineTargetGroup.Target
            {
                Object = curTarget.transform,
                Weight = 1f,
                Radius = 2f
            });
            uniqueTargets.Add(curTarget.transform);
        }

        // Add the AI's current target (if exists and isn't already added)
        if (curTarget.target != null && uniqueTargets.Add(curTarget.target))
        {
            group.Targets.Add(new CinemachineTargetGroup.Target
            {
                Object = curTarget.target,
                Weight = 0.9f,
                Radius = 1.8f
            });
        }

        // Add threats (enemy aircraft targeting this aircraft) using `threats` list
        foreach (var threat in curTarget.threats)
        {
            if (threat != null && uniqueTargets.Add(threat.transform))
            {
                group.Targets.Add(new CinemachineTargetGroup.Target
                {
                    Object = threat.transform,
                    Weight = 0.8f,
                    Radius = 1.5f
                });
            }
        }

        if (curTarget.strafing && uniqueTargets.Add(curTarget.groundTarget))
        {
            group.Targets.Add(new CinemachineTargetGroup.Target
            {
                Object = curTarget.groundTarget.transform,
                Weight = 0.9f,
                Radius = 1.5f
            });
        }
    }


    void NextTarget()
    {
        if (allAircraft.Count == 0) return;

        targetIndex = (targetIndex + 1) % allAircraft.Count;
        curTarget = allAircraft[targetIndex];
        cam.Target.TrackingTarget = curTarget.transform;
        UpdateGroup();
    }

    void PreviousTarget()
    {
        if (allAircraft.Count == 0) return;

        targetIndex = (targetIndex - 1 + allAircraft.Count) % allAircraft.Count;
        curTarget = allAircraft[targetIndex];
        cam.Target.TrackingTarget = curTarget.transform;
        UpdateGroup();
    }
}
