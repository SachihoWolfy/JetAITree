using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

public class AIAircraft : MonoBehaviour
{
    public Transform target;
    public Transform groundTarget;
    public string currentState = "Idle";
    public ManueverStatus m_status;
    public TargetingStatus t_status;
    public Team team;
    public ParticleSystem gun;
    public AudioSource gunAudio;
    public AudioSource brrtAudio;
    public bool strafing = false;

    public AircraftMovement aircraftMovement;
    private BTSelector behaviorTree;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    public List<AIAircraft> allAircraft;
    public Dictionary<AIAircraft, Transform> teamTargets = new Dictionary<AIAircraft, Transform>();
    public List<AIAircraft> teammates = new List<AIAircraft>();
    public List<AIAircraft> enemies = new List<AIAircraft>();
    public List<AIAircraft> threats = new List<AIAircraft>();
    private bool doEratic;
    private TargetSelectionTree t_tree;

    public string aircraftID = "";
    public int kills = 0;

    void Start()
    {
        allAircraft.AddRange(FindObjectsOfType<AIAircraft>());
        aircraftMovement = GetComponent<AircraftMovement>();
        t_tree = GetComponent<TargetSelectionTree>();

        // Store initial position and rotation for respawning
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        // Set up the behavior tree
        BTSelector root = new BTSelector();

        // Existing behavior
        BTSequence evadeSequence = new BTSequence();
        evadeSequence.AddChild(new BTEvasiveManeuver(this));

        BTSequence dogfightSequence = new BTSequence();
        dogfightSequence.AddChild(new BTDogfight(this));

        BTSequence strafingSequence = new BTSequence();
        strafingSequence.AddChild(new BTStrafingRun(this));

        // Root sequence: add the new branches
        root.AddChild(evadeSequence);
        root.AddChild(dogfightSequence);
        root.AddChild(strafingSequence);

        behaviorTree = root;
        target = null;

        ChangeID();
    }

    public void ChangeID()
    {
        aircraftID = NVJOBNameGen.GiveAName(3);
        gameObject.name = aircraftID;
    }

    void FixedUpdate()
    {
        UpdateTeammates();
        UpdateEnemies();
        UpdateThreats();
        UpdateTeamTargets();
        behaviorTree.Execute();
        engagementTime += Time.deltaTime;

        doEratic = t_tree.confidenceValue > 0.6f;

    }

    // ------------------ THREAT & TEAM AWARENESS ------------------

    private void UpdateEnemies()
    {
        enemies.Clear();
        enemies.AddRange(allAircraft);
        foreach (var teammate in teammates) 
        {
            enemies.Remove(teammate);
        }
        enemies.Remove(this);
    }
    private void UpdateThreats()
    {
        threats.Clear();
        foreach (var enemy in enemies)
        {
            if (enemy.target == this.transform)
            {
                threats.Add(enemy);
            }
        }
    }
    private void UpdateTeammates() 
    {
        teammates.Clear();
        foreach (var aircraft in allAircraft)
        {
            if (aircraft.team == team && aircraft != this)
            {
                teammates.Add(aircraft);
            }
        }
        aircraftMovement.teammates.Clear();
        aircraftMovement.teammates.AddRange(teammates);
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
        aircraftMovement.ResetAircraft(initialPosition, initialRotation);
        target = null;
        currentState = "Idle";
        ChangeID();
        kills = 0;
    }

    public void TargetPursuer()
    {
        target = threats[0].transform;
    }

    public void TargetProtection()
    {
        // NOPE.
    }

    float engagementTime;
    float engagementThreshold = 5f; 
    Transform prevTarget;
    public Transform FindBestTarget(float teammateMod = 0f)
    {
        if (engagementTime < engagementThreshold) return target;
        prevTarget = target;
        UpdateThreats();
        List<AIAircraft> enemies = allAircraft.Where(a => a.team != this.team).ToList();
        AIAircraft bestTarget = null;
        float bestScore = float.MinValue;
        foreach (var enemy in enemies)
        {
            float score = 0;

            if (enemy.target == this.transform) score += 500;

            if(enemy == target && engagementTime > engagementThreshold)
            {
                score -= engagementTime - engagementThreshold;
            }

            if (teammates.Any(t => t.target == enemy.transform) && enemy.threats.Count<2) score += 100;

            if (teammates.Any(t => t.threats.Contains(enemy))) score += 100 * teammateMod;

            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            score += Mathf.Clamp(1000 - distance / 2, 0, 1000); 

            Vector3 directionToAI = (transform.position - enemy.transform.position).normalized;
            Vector3 enemyForward = enemy.transform.forward;
            float dotProduct = Vector3.Dot(enemyForward, directionToAI);
            score += dotProduct * 1000;

            if (score > bestScore && enemy.team != team)
            {
                bestScore = score;
                if(enemy != target)
                bestTarget = enemy;
            }
        }

        // If the best target is different from the current one, reset the engagement timer. PS: Going insane
        if (bestTarget != target)
        {
            engagementTime = 0f;
        }

        prevTarget = target;
        return bestTarget.transform;
    }



    public void MoveAircraft(Vector3 direction, float desiredSpeed, bool doInputScaling = false)
    {
        aircraftMovement.MoveAircraft(direction, desiredSpeed, doInputScaling);
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
        strafing = false;
        m_status = ManueverStatus.Evading;
        gun.Stop();
        if (gunAudio.isPlaying)
        {
            brrtAudio.Play();
        }
        gunAudio.Stop();
        if (threats[0] != null)
        {
            Vector3 enemyForward = threats[0].transform.forward;
            Vector3 evasiveDirection = Vector3.Cross(enemyForward, Vector3.up).normalized;
            float desiredSpeed = Mathf.Clamp(aircraftMovement.currentSpeed + 20f, aircraftMovement.minSpeed, aircraftMovement.maxSpeed);
            aircraftMovement.MoveAircraft(evasiveDirection, desiredSpeed, false, true);
        }
    }

    public void EngageDogfight()
    {
        strafing = false;
        m_status = ManueverStatus.Dogfighting;
        if (target != null)
        {
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            float dotProduct = Vector3.Dot(transform.forward, directionToTarget);
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            float threshold = 0.98f;
            if (dotProduct >= threshold && distanceToTarget < 150f)
            {
                gun.Play();
                if (!gunAudio.isPlaying)
                {
                    brrtAudio.Stop();
                    gunAudio.Play();
                }
            }
            else
            {
                if (gunAudio.isPlaying)
                {
                    brrtAudio.Play();
                }
                gun.Stop();
                gunAudio.Stop();
            }
            aircraftMovement.MoveAircraft(directionToTarget, aircraftMovement.maxSpeed, false, doEratic);
        }
    }

    public void PerformStrafingRun()
    {
        strafing = true;
        currentState = "Strafing Run";
        m_status = ManueverStatus.Strafing;

        if (groundTarget != null)
        {
            Vector3 directionToGroundTarget = (groundTarget.position - transform.position).normalized;
            float distanceToTarget = Vector3.Distance(transform.position, groundTarget.position);

            float targetSpeed = Mathf.Clamp(distanceToTarget / 2f, aircraftMovement.minSpeed, aircraftMovement.maxSpeed); 
            aircraftMovement.MoveAircraft(directionToGroundTarget, targetSpeed, true);

            float dotProduct = Vector3.Dot(transform.forward, directionToGroundTarget);
            float threshold = 0.98f;

            if (dotProduct >= threshold && distanceToTarget < 200f)
            {
                gun.Play();
                if (!gunAudio.isPlaying)
                {
                    brrtAudio.Stop();
                    gunAudio.Play();
                }
            }
            else
            {
                if (gunAudio.isPlaying)
                {
                    brrtAudio.Play();
                }
                gun.Stop();
                gunAudio.Stop();
            }
        }
    }


    public void Patrol()
    {
        if (gunAudio.isPlaying)
        {
            brrtAudio.Play();
        }
        gun.Stop();
        gunAudio.Stop();
        currentState = "Patrolling";
        aircraftMovement.MoveAircraft(Vector3.forward, aircraftMovement.minSpeed, true);
    }

    public bool HasGroundTarget()
    {
        return groundTarget != null && groundTarget.gameObject.activeSelf;
    }

    public bool CanEngageEnemy()
    {
        if (target == null) return false;
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget > 700f && !target.GetComponent<AIAircraft>().strafing) return false;

        if (Physics.Raycast(transform.position, (target.position - transform.position).normalized, out RaycastHit hit, distanceToTarget))
        {
            if (hit.transform != target) return false;
        }

        float targetAltitude = target.position.y;
        return targetAltitude >= 0f && targetAltitude <= 10000f;
    }
    /*
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
    */
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Damagable"))
        {
            collision.gameObject.GetComponent<AircraftStats>().TakeDamage(20);
            GetComponent<AircraftStats>().TakeDamage(20);
        }
    }
}

public enum Team
{
    Red,
    Blue
}
