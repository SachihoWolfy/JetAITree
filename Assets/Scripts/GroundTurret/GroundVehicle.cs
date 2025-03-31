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

    // Move the vehicle towards the target
    private void MoveToTarget()
    {
        if (!isMoving)
        {
            agent.SetDestination(target);  // Set the target position
            isMoving = true;
        }

        // Check if we've reached the target
        if (Vector3.Distance(transform.position, target) <= agent.stoppingDistance)
        {
            isMoving = false;
            agent.ResetPath();  // Stop the movement once we reach the target
        }
    }

    // You can call this method to update the target dynamically
    public void SetTarget(Vector3 newTarget)
    {
        target = newTarget;
        isMoving = false; // Reset movement
    }
}
