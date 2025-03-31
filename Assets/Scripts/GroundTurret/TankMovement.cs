using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class TankMovement : MonoBehaviour
{
    [Header("Tank Components")]
    public GroundVehicle groundVehicle;  // Reference to GroundVehicle to control movement
    public GroundGunController groundGunController;  // Reference to GroundGunController
    public LayerMask coverLayer; // Define which layers are considered cover

    [Header("Movement Settings")]
    public float safeDistance = 10f; // Minimum distance to maintain from target
    public float checkCoverDistance = 15f; // Distance to check for cover
    public float coverScanAngle = 45f; // Angle to scan for cover in front of the tank
    public float coverScanRadius = 20f; // Radius of the area to scan for cover
    public float cornerScanRadius = 10f; // Radius to check for corner coverage

    private Transform target;
    private bool isSeekingCover = false;

    private List<Transform> threats = new List<Transform>(); // List of threats currently targeting the tank

    private void Update()
    {
        if (groundGunController.target != null)
        {
            target = groundGunController.target;
            MoveTank();
        }
    }

    private void MoveTank()
    {
        // Find threats (enemies targeting this tank)
        FindThreats();

        // Check if the target is visible
        if (IsTargetInSight())
        {
            // If we can see the target and are in danger, look for cover
            if (IsExposedToTarget())
            {
                SeekCover();
            }
            else
            {
                // Move towards the target if no danger
                if (!isSeekingCover)
                {
                    // If raycast hits the target, stop moving closer
                    if (!IsTargetInRaycastSight())
                    {
                        MoveTowardsTarget();
                    }
                }
            }
        }
        else
        {
            // If no LOS, move towards the target safely
            if (!isSeekingCover)
            {
                MoveTowardsTarget();
            }
        }
    }

    private void FindThreats()
    {
        threats.Clear();
        foreach (var groundTarget in GroundTargetManager.Instance.allGroundTargets)
        {
            // Add to the list of threats if they are targeting this tank
            if (groundTarget.target == this.transform)
            {
                threats.Add(groundTarget.transform);
            }
        }
    }

    private bool IsTargetInSight()
    {
        // Check if there's a clear line of sight to the target
        RaycastHit hit;
        Vector3 direction = target.position - transform.position;

        if (Physics.Raycast(transform.position, direction, out hit, Mathf.Infinity))
        {
            if (hit.transform == target)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsExposedToTarget()
    {
        // Check if the tank is in line of sight of the target and is within a danger zone
        Vector3 directionToTarget = target.position - transform.position;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToTarget, out hit, Mathf.Infinity))
        {
            // If the target is within a certain range and visible, return true
            return hit.transform == target && directionToTarget.magnitude < safeDistance;
        }
        return false;
    }

    private bool IsTargetInRaycastSight()
    {
        // Cast a ray from the tank to the target and check if it hits the target
        RaycastHit hit;
        Vector3 direction = target.position - transform.position;

        if (Physics.Raycast(transform.position, direction, out hit, Mathf.Infinity))
        {
            // If the ray hits the target, return true to stop movement
            if (hit.transform == target)
            {
                return true;
            }
        }

        return false;
    }

    private void MoveTowardsTarget()
    {
        // Calculate a safe point and set it as the move target
        Vector3 moveDirection = target.position - transform.position;
        moveDirection.y = 0; // Keep movement on the XZ plane

        // Maintain safe distance from the target
        if (moveDirection.magnitude < safeDistance)
        {
            moveDirection = moveDirection.normalized * safeDistance;
        }

        // Set the target position to move towards (slightly offset to avoid collision)
        groundVehicle.SetTarget(transform.position + moveDirection);
    }

    private void SeekCover()
    {
        // Scan for cover in the direction of the target
        Transform bestCover = FindCover();
        if (bestCover != null)
        {
            isSeekingCover = true;
            groundVehicle.SetTarget(bestCover.position);
        }
        else
        {
            // If no cover found, move toward the target in a safer direction
            isSeekingCover = false;
            MoveTowardsTarget();
        }
    }

    private Transform FindCover()
    {
        Vector3 directionToTarget = target.position - transform.position;
        Vector3 leftSide = transform.position + Quaternion.Euler(0, -coverScanAngle, 0) * directionToTarget;
        Vector3 rightSide = transform.position + Quaternion.Euler(0, coverScanAngle, 0) * directionToTarget;

        RaycastHit hit;

        // Scan to left and right of the target for cover
        if (Physics.Raycast(leftSide, directionToTarget, out hit, coverScanRadius, coverLayer))
        {
            // Return cover if it blocks the view to the target
            if (hit.transform != null && IsCoverBlockingThreats(hit.transform))
            {
                return hit.transform;
            }
        }

        if (Physics.Raycast(rightSide, directionToTarget, out hit, coverScanRadius, coverLayer))
        {
            // Return cover if it blocks the view to the target
            if (hit.transform != null && IsCoverBlockingThreats(hit.transform))
            {
                return hit.transform;
            }
        }

        // Check for corners (L-shaped cover)
        if (CheckForCornerCover(directionToTarget))
        {
            return GetBestCornerCover();
        }

        return null; // No cover found
    }

    private bool CheckForCornerCover(Vector3 directionToTarget)
    {
        RaycastHit hitLeft, hitRight;
        Vector3 leftSide = transform.position + Quaternion.Euler(0, -90f, 0) * directionToTarget;
        Vector3 rightSide = transform.position + Quaternion.Euler(0, 90f, 0) * directionToTarget;

        // Cast a ray at a corner angle to detect L-shaped obstacles
        if (Physics.Raycast(leftSide, directionToTarget, out hitLeft, cornerScanRadius, coverLayer) &&
            Physics.Raycast(rightSide, directionToTarget, out hitRight, cornerScanRadius, coverLayer))
        {
            return true;
        }

        return false;
    }

    private bool IsCoverBlockingThreats(Transform cover)
    {
        foreach (Transform threat in threats)
        {
            Vector3 directionToThreat = threat.position - cover.position;
            RaycastHit hit;

            // Cast a ray from the cover to the threat to check if the cover blocks the threat's line of sight
            if (Physics.Raycast(cover.position, directionToThreat, out hit, Mathf.Infinity))
            {
                if (hit.transform == threat)
                {
                    return true; // The cover is blocking this threat's line of sight
                }
            }
        }
        return false; // The cover doesn't block any threat
    }

    private Transform GetBestCornerCover()
    {
        // Search for the closest corner by comparing hit points from both rays
        RaycastHit hitLeft, hitRight;
        Vector3 directionToTarget = target.position - transform.position;
        Vector3 leftSide = transform.position + Quaternion.Euler(0, -90f, 0) * directionToTarget;
        Vector3 rightSide = transform.position + Quaternion.Euler(0, 90f, 0) * directionToTarget;

        if (Physics.Raycast(leftSide, directionToTarget, out hitLeft, cornerScanRadius, coverLayer) &&
            Physics.Raycast(rightSide, directionToTarget, out hitRight, cornerScanRadius, coverLayer))
        {
            // You can enhance this by checking which side has better cover (e.g., closer to the target)
            if (hitLeft.distance < hitRight.distance)
            {
                return hitLeft.transform;
            }
            return hitRight.transform;
        }

        return null;
    }
}
