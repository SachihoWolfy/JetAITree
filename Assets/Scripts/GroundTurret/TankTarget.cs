using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankTarget : MonoBehaviour
{
    [Header("Tank Target Settings")]
    public float detectionRange = 1000f; // Range at which targets are detected
    private GroundGunController gunController; // Reference to the GroundGunController
    private GroundTargetStats ourStats;

    private GroundTargetStats closestTarget; // The closest target to aim at

    void Start()
    {
        gunController = GetComponent<GroundGunController>();
        ourStats = GetComponent<GroundTargetStats>();
    }

    void Update()
    {
        FindClosestTarget();
    }

    private void FindClosestTarget()
    {
        closestTarget = null;
        float shortestDistance = detectionRange;

        foreach (GroundTargetStats target in GroundTargetManager.Instance.allGroundTargets)
        {
            if (target == null || !target.gameObject.activeInHierarchy) continue; // Skip inactive targets

            float distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance < shortestDistance && target.team != ourStats.team)
            {
                closestTarget = target;
                shortestDistance = distance;
            }
        }
        if (closestTarget != null)
        {
            gunController.SetTarget(closestTarget.transform);
            ourStats.target = closestTarget;
        }
    }
}

