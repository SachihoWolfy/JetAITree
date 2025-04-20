using UnityEngine;
using UnityEngine.AI;

public class GroundVehicle : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f; // Speed of movement
    private NavMeshAgent agent;  // Reference to the NavMeshAgent

    [Header("Target Settings")]
    public Vector3 target;  // Target location to move towards
    private bool isMoving = false; // Check if the vehicle is moving

    void Start()
    {
        // Initialize the NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
    }

    void Update()
    {
        if (target != null)
        {
            MoveToTarget();
        }
    }

    private void MoveToTarget()
    {
        if (!isMoving)
        {
            agent.SetDestination(target);  
            isMoving = true;
        }

        if (Vector3.Distance(transform.position, target) <= agent.stoppingDistance)
        {
            isMoving = false;
            agent.ResetPath(); 
        }
    }

    public void SetTarget(Vector3 newTarget)
    {
        target = newTarget;
        isMoving = false; 
    }
}
