using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
public class TankMovement : MonoBehaviour
{
    [Header("Tank Components")]
    public GroundVehicle groundVehicle;
    public GroundGunController groundGunController;
    public LayerMask coverLayer;
    private GroundTargetStats stats;

    [Header("Movement Settings")]
    public float idealEngageDistance = 25f;
    public float minSafeDistance = 10f;
    public float retreatDistance = 15f;
    public float flankDistance = 20f;

    private Transform target;
    private List<Transform> threats = new List<Transform>();

    private void Start()
    {
        stats = GetComponent<GroundTargetStats>();
    }

    private void Update()
    {
        if (stats.isDestroyed)
        {
            target = null;
            return;
        }

        target = groundGunController.target ?? transform;
        FindThreats();

        if (target != null)
        {
            Vector2 move = CalculateStrategicMove();
            Vector3 worldTarget = new Vector3(move.x, transform.position.y, move.y);
            groundVehicle.SetTarget(worldTarget);
        }
    }

    private void FindThreats()
    {
        threats.Clear();
        foreach (var enemy in GroundTargetManager.Instance.allGroundTargets)
        {
            if (enemy.target == transform)
            {
                threats.Add(enemy.transform);
            }
        }
    }

    private Vector2 CalculateStrategicMove()
    {
        Vector2 currentPos = new Vector2(transform.position.x, transform.position.z);
        Vector2 targetPos = new Vector2(target.position.x, target.position.z);
        Vector2 toTarget = (targetPos - currentPos);
        float distance = toTarget.magnitude;

        // If too close to target, back away
        if (distance < minSafeDistance)
        {
            return currentPos - toTarget.normalized * retreatDistance;
        }

        // Try to maintain optimal distance and slight offset (flank)
        Vector2 flankOffset = Vector2.Perpendicular(toTarget.normalized) * flankDistance;
        Vector2 strategicPos = targetPos - toTarget.normalized * idealEngageDistance + flankOffset;

        // Check for visible threats and adjust if needed
        foreach (Transform threat in threats)
        {
            if (IsVisible(threat))
            {
                Vector2 threatPos = new Vector2(threat.position.x, threat.position.z);
                Vector2 awayFromThreat = (currentPos - threatPos).normalized * retreatDistance;
                strategicPos += awayFromThreat;
            }
        }

        return strategicPos;
    }

    private bool IsVisible(Transform other)
    {
        Vector3 dir = other.position - transform.position;
        return Physics.Raycast(transform.position, dir, out RaycastHit hit, 1000f) && hit.transform == other;
    }
}

public static class CoverFinder
{
    public static Transform FindBestCover(Transform seeker, Transform target, List<Transform> threats, LayerMask coverLayer, float scanAngle, float scanRadius, float cornerRadius)
    {
        Vector3 directionToTarget = target.position - seeker.position;

        // Try direct cover on left/right
        Transform leftCover = ScanForCover(seeker, directionToTarget, -scanAngle, scanRadius, coverLayer, threats);
        if (leftCover != null) return leftCover;

        Transform rightCover = ScanForCover(seeker, directionToTarget, scanAngle, scanRadius, coverLayer, threats);
        if (rightCover != null) return rightCover;

        // Check for L-shaped cover (corners)
        if (CheckForCornerCover(seeker, directionToTarget, cornerRadius, coverLayer))
        {
            return GetBestCornerCover(seeker, directionToTarget, cornerRadius, coverLayer);
        }

        return null;
    }

    private static Transform ScanForCover(Transform seeker, Vector3 directionToTarget, float angle, float radius, LayerMask coverLayer, List<Transform> threats)
    {
        Vector3 scanDirection = Quaternion.Euler(0, angle, 0) * directionToTarget;
        Vector3 scanOrigin = seeker.position;

        if (Physics.Raycast(scanOrigin, scanDirection, out RaycastHit hit, radius, coverLayer))
        {
            if (IsCoverBlockingThreats(hit.transform, threats))
            {
                return hit.transform;
            }
        }

        return null;
    }

    private static bool IsCoverBlockingThreats(Transform cover, List<Transform> threats)
    {
        foreach (Transform threat in threats)
        {
            Vector3 directionToThreat = threat.position - cover.position;
            if (Physics.Raycast(cover.position, directionToThreat, out RaycastHit hit, Mathf.Infinity))
            {
                if (hit.transform == threat)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private static bool CheckForCornerCover(Transform seeker, Vector3 directionToTarget, float radius, LayerMask coverLayer)
    {
        Vector3 left = seeker.position + Quaternion.Euler(0, -90f, 0) * directionToTarget.normalized * 2f;
        Vector3 right = seeker.position + Quaternion.Euler(0, 90f, 0) * directionToTarget.normalized * 2f;

        return Physics.Raycast(left, directionToTarget, radius, coverLayer) &&
               Physics.Raycast(right, directionToTarget, radius, coverLayer);
    }

    private static Transform GetBestCornerCover(Transform seeker, Vector3 directionToTarget, float radius, LayerMask coverLayer)
    {
        Vector3 left = seeker.position + Quaternion.Euler(0, -90f, 0) * directionToTarget.normalized * 2f;
        Vector3 right = seeker.position + Quaternion.Euler(0, 90f, 0) * directionToTarget.normalized * 2f;

        RaycastHit hitLeft, hitRight;

        bool leftHit = Physics.Raycast(left, directionToTarget, out hitLeft, radius, coverLayer);
        bool rightHit = Physics.Raycast(right, directionToTarget, out hitRight, radius, coverLayer);

        if (leftHit && rightHit)
        {
            return hitLeft.distance < hitRight.distance ? hitLeft.transform : hitRight.transform;
        }

        return leftHit ? hitLeft.transform : rightHit ? hitRight.transform : null;
    }
}
