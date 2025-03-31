using System.Collections.Generic;
using UnityEngine;

public class CIWSTargetController : MonoBehaviour
{
    private GroundGunController gunController;
    private GroundTargetStats stats;
    public float updateInterval = 2f; // How often to update target
    public float maxTargetRange = 100f; // Max range to track targets

    private float nextUpdateTime;

    private void Start()
    {
        gunController = GetComponent<GroundGunController>();
        stats = GetComponent<GroundTargetStats>();
    }
    private void Update()
    {
        if (Time.time >= nextUpdateTime)
        {
            nextUpdateTime = Time.time + updateInterval;
            UpdateTarget();
        }
    }

    private void UpdateTarget()
    {
        AircraftStats closestTarget = FindClosestEnemyAircraft();
        if (closestTarget != null)
        {
            gunController.SetTarget(closestTarget.transform);
        }
        else
        {
            gunController.SetTarget(null); // No valid targets
        }
    }

    private AircraftStats FindClosestEnemyAircraft()
    {
        List<AircraftStats> allAirTargets = GroundTargetManager.Instance.allAirTargets;
        AircraftStats closest = null;
        float closestDistance = maxTargetRange;

        foreach (AircraftStats aircraft in allAirTargets)
        {
            if (aircraft.team == stats.team) continue; // Skip friendly aircraft
            if (aircraft.hp <= 0) continue; // Skip destroyed aircraft

            float distance = Vector3.Distance(transform.position, aircraft.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = aircraft;
            }
        }

        return closest;
    }
}
