using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

public class AIAircraft : MonoBehaviour
{
    public Transform target;
    public Transform groundTarget;
    public string currentState = "Idle";
    public Team team;
    public ParticleSystem gun;

    public AircraftMovement aircraftMovement;
    private BTSelector behaviorTree;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    public List<AIAircraft> allAircraft;
    public Dictionary<AIAircraft, Transform> teamTargets = new Dictionary<AIAircraft, Transform>();
    public List<AIAircraft> teammates = new List<AIAircraft>();
    public List<AIAircraft> threats = new List<AIAircraft>();

    void Start()
    {
        allAircraft.AddRange(FindObjectsOfType<AIAircraft>());
        aircraftMovement = GetComponent<AircraftMovement>();

        // Store initial position and rotation for respawning
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        // Set up the behavior tree
        BTSelector root = new BTSelector();

        // Add a branch for responding to teammates in danger
        BTSelector teammateProtectionSelector = new BTSelector();
        teammateProtectionSelector.AddChild(new BTCheckTeammateInDanger(this));
        teammateProtectionSelector.AddChild(new BTProtectTeammate(this)); // Implement the protection logic

        // Add a branch for responding to being engaged by a threat
        BTSelector threatEngagementSelector = new BTSelector();
        threatEngagementSelector.AddChild(new BTCheckIfBeingEngaged(this));
        threatEngagementSelector.AddChild(new BTEngagePursuer(this)); // Implement the pursuit behavior

        // Existing behavior
        BTSequence evadeSequence = new BTSequence();
        evadeSequence.AddChild(new BTEvasiveManeuver(this));

        BTSequence dogfightSequence = new BTSequence();
        dogfightSequence.AddChild(new BTDogfight(this));

        BTSequence strafingSequence = new BTSequence();
        strafingSequence.AddChild(new BTStrafingRun(this));

        // Root sequence: add the new branches
        root.AddChild(teammateProtectionSelector);
        root.AddChild(threatEngagementSelector);
        root.AddChild(evadeSequence);
        root.AddChild(dogfightSequence);
        root.AddChild(strafingSequence);

        behaviorTree = root;
    }

    void FixedUpdate()
    {
        if (target == null)
        {
            foreach (var enemy in allAircraft.Where(a => a.team != this.team))
            {
                if (target == null)
                {
                    target = enemy.transform;
                }
            }
        }
        UpdateThreats();
        UpdateTeamTargets();
        behaviorTree.Execute();
    }

    // ------------------ THREAT & TEAM AWARENESS ------------------

    private void UpdateThreats()
    {
        threats.Clear();
        foreach (var enemy in allAircraft.Where(a => a.team != this.team))
        {
            if (enemy.target == this.transform)
            {
                threats.Add(enemy);
            }
        }
    }

    private void UpdateTeamTargets()
    {
        teamTargets.Clear();
        foreach (var ally in allAircraft.Where(a => a.team == this.team && a != this))
        {
            if (ally.target != null)
            {
                teamTargets[ally] = ally.target;
            }
        }
    }

    public List<AIAircraft> GetThreats()
    {
        return threats;
    }

    public Dictionary<AIAircraft, Transform> GetTeamTargets()
    {
        return teamTargets;
    }

    public void Respawn()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        target = null;
        currentState = "Idle";
    }

    public void TargetPursuer()
    {
        target = threats[0].transform;
    }

    float engagementTime;
    Transform prevTarget;
    public Transform FindBestTarget()
    {
        prevTarget = target;
        UpdateThreats();
        List<AIAircraft> enemies = allAircraft.Where(a => a.team != this.team).ToList();
        AIAircraft bestTarget = null;
        float bestScore = float.MinValue;

        // Engagement timer and threshold
        float engagementThreshold = 5f; // Seconds before the AI starts penalizing the current target

        // If AI is already engaged with a target, start tracking engagement time
        if (target != null)
        {
            engagementTime += Time.deltaTime;
        }

        foreach (var enemy in enemies)
        {
            float score = 0;
            if (enemy.transform == prevTarget) {
                score -= 500;
                    }
            // Prioritize enemies targeting us
            if (enemy.target == this.transform) score += 500;

            if(enemy == target && engagementTime > engagementThreshold)
            {
                score -= engagementTime - engagementThreshold;
            }

            // Don't deprioritize enemies that teammates are already targeting
            if (teammates.Any(t => t.target == enemy.transform)) score += 100;

            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            score += Mathf.Clamp(500 - distance, 0, 500); // Closer enemies have higher priority

            // If this target has a higher score, set it as the best target
            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = enemy;
            }
        }

        // If the best target is different from the current one, reset the engagement timer
        if (bestTarget != target)
        {
            engagementTime = 0f;
        }

        // Return the best target found
        return bestTarget?.transform;
    }



    public void MoveAircraft(Vector3 direction, float desiredSpeed)
    {
        aircraftMovement.MoveAircraft(direction, desiredSpeed);
    }

    public bool IsInDanger()
    {
        float maxDot = 0f;
        foreach(var threat in threats)
        {
            Vector3 directionToAI = (transform.position - threat.transform.position).normalized;
            Vector3 enemyForward = threat.transform.forward;
            float dotProduct = Vector3.Dot(enemyForward, directionToAI);
            if(dotProduct > maxDot && (transform.position - threat.transform.position).magnitude < 500f)
            {
                maxDot = dotProduct;
            }
        }
        return maxDot > 0.5f;
    }

    public void PerformEvasiveManeuver()
    {
        gun.Stop();
        currentState = "Evasive Maneuver";
        if (target != null)
        {
            Vector3 enemyForward = target.forward;
            Vector3 evasiveDirection = Vector3.Cross(enemyForward, Vector3.up).normalized;
            float desiredSpeed = Mathf.Clamp(aircraftMovement.currentSpeed + 20f, aircraftMovement.minSpeed, aircraftMovement.maxSpeed);
            aircraftMovement.MoveAircraft(evasiveDirection, desiredSpeed);
        }
    }

    public void EngageDogfight()
    {
        if (target != null)
        {
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            float dotProduct = Vector3.Dot(transform.forward, directionToTarget);

            float threshold = 0.98f; // Adjust this value as needed
            if (dotProduct >= threshold)
            {
                gun.Play();
            }
            else
            {
                gun.Stop();
            }

            aircraftMovement.MoveAircraft(directionToTarget, aircraftMovement.maxSpeed);
            currentState = "Dogfighting";
        }
    }

    public void PerformStrafingRun()
    {
        currentState = "Strafing Run";
        if (groundTarget != null)
        {
            Vector3 directionToGroundTarget = (groundTarget.position - transform.position).normalized;
            aircraftMovement.MoveAircraft(directionToGroundTarget, aircraftMovement.maxSpeed);
        }
    }

    public void Patrol()
    {
        currentState = "Patrolling";
        aircraftMovement.MoveAircraft(Vector3.forward, aircraftMovement.minSpeed);
    }

    public bool HasGroundTarget()
    {
        return groundTarget != null && groundTarget.gameObject.activeSelf;
    }

    public bool CanEngageEnemy()
    {
        if (target == null) return false;
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget > 500f) return false;

        if (Physics.Raycast(transform.position, (target.position - transform.position).normalized, out RaycastHit hit, distanceToTarget))
        {
            if (hit.transform != target) return false;
        }

        float targetAltitude = target.position.y;
        return targetAltitude >= 0f && targetAltitude <= 10000f;
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, aircraftMovement.collisionAvoidanceDistance);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, aircraftMovement.emergencyAvoidanceDistance);

            if (target != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, target.position);
            }

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, 1f);
            Handles.Label(transform.position + Vector3.up * 2, currentState);
        }
    }
}

public enum Team
{
    Red,
    Blue
}
