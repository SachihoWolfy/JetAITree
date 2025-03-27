using UnityEngine;

public class AircraftMovement : MonoBehaviour
{
    public float minSpeed = 30f;
    public float maxSpeed = 100f;
    public float acceleration = 10f;
    public float currentSpeed;

    public float maxRollSpeed = 80f;  // Maximum roll speed (degrees per second)
    public float maxPitchSpeed = 60f; // Maximum pitch speed (degrees per second)
    public float maxYawSpeed = 10f;   // Maximum yaw speed (degrees per second)

    public float collisionAvoidanceDistance = 50f;  // Detection range for obstacles
    public float emergencyAvoidanceDistance = 20f; // Closer range for urgent maneuvers

    private Rigidbody rb;

    // Smooth damping variables for each axis
    private float rollInputDampingVelocity = 0f;
    private float pitchInputDampingVelocity = 0f;
    private float yawInputDampingVelocity = 0f;

    // Damping time for each input
    public float dampingTime = 0.3f; // Time for the damping effect (how long it takes to ease into the final value)

    public float stallHeight = 1000f;  // Height at which the stall begins
    public float stallRecoveryForce = 0.7f; // Strength of downward guidance
    public float groundAvoidHeight = 100f;

    public float pitchInput;
    public float rollInput;
    public float yawInput;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentSpeed = minSpeed;
    }
    private void Update()
    {
        timeSinceReflect += Time.deltaTime;
    }

    public void MoveAircraft(Vector3 desiredDirection, float desiredSpeed)
    {
        // Ensure the desired speed is within bounds
        desiredSpeed = Mathf.Clamp(desiredSpeed, minSpeed, maxSpeed);

        // Check if the aircraft is above stall height
        if (transform.position.y > stallHeight)
        {
            // Smoothly guide the nose downward instead of forcing a loop
            Vector3 correctedDirection = Vector3.Lerp(desiredDirection, Vector3.down, stallRecoveryForce);
            desiredDirection = correctedDirection.normalized;
        }
        if (transform.position.y < groundAvoidHeight)
        {
            // Smoothly guide the nose downward instead of forcing a loop
            Vector3 correctedDirection = Vector3.Lerp(desiredDirection, Vector3.up, stallRecoveryForce);
            desiredDirection = correctedDirection.normalized;
        }

        // --- THRUST CONTROL ---
        currentSpeed = Mathf.MoveTowards(currentSpeed, desiredSpeed, acceleration * Time.fixedDeltaTime);

        // --- OBSTACLE AVOIDANCE ---
        Vector3 avoidanceDirection = AvoidObstacles();
        if (avoidanceDirection != Vector3.zero)
        {
            desiredDirection = Vector3.Lerp(desiredDirection, avoidanceDirection, 0.7f).normalized;
        }

        // --- MOVEMENT CONTROL ---
        Vector3 localTargetDir = transform.InverseTransformDirection(desiredDirection);

        // --- ROLL CONTROL ---
        float desiredYawAngle = Vector3.SignedAngle(transform.forward, desiredDirection, Vector3.up);
        Vector3 cross = Vector3.Cross(transform.forward, desiredDirection);
        rollInput = Mathf.Sign(cross.y);
        rollInput = Mathf.SmoothDamp(rollInputDampingVelocity, rollInput, ref rollInputDampingVelocity, dampingTime);

        // --- PITCH CONTROL ---
        Vector3 projectedTarget = Vector3.ProjectOnPlane(desiredDirection, transform.right);
        float pitchAngle = Vector3.SignedAngle(transform.forward, projectedTarget, transform.right);
        pitchInput = Mathf.Clamp(Mathf.Sign(pitchAngle), -1f, 1f);
        pitchInput = Mathf.SmoothDamp(pitchInputDampingVelocity, pitchInput, ref pitchInputDampingVelocity, dampingTime);

        // --- YAW CONTROL ---
        float yawAngle = Vector3.SignedAngle(transform.forward, desiredDirection, Vector3.up);
        yawInput = Mathf.Clamp(Mathf.Sign(yawAngle), -1f, 1f);
        yawInput = Mathf.SmoothDamp(yawInputDampingVelocity, yawInput, ref yawInputDampingVelocity, dampingTime);

        // Apply the damping to inputs
        rollInput *= maxRollSpeed * Time.fixedDeltaTime;
        pitchInput *= maxPitchSpeed * Time.fixedDeltaTime;
        yawInput *= maxYawSpeed * Time.fixedDeltaTime;

        // Apply the rotation (pitch, yaw, and roll)
        Quaternion deltaRotation = Quaternion.Euler(pitchInput, yawInput, rollInput);
        rb.MoveRotation(rb.rotation * deltaRotation);

        // Apply forward velocity
        rb.velocity = transform.forward * currentSpeed;
    }


    // Function to handle obstacle avoidance
    private Vector3 AvoidObstacles()
    {
        RaycastHit hit;
        Vector3 avoidanceVector = Vector3.zero;

        // Check for forward obstacles
        if (Physics.Raycast(transform.position, transform.forward, out hit, collisionAvoidanceDistance))
        {
            float urgency = hit.distance < emergencyAvoidanceDistance ? 1.0f : 0.5f;
            avoidanceVector += -transform.forward * urgency; // Move away from forward obstacle
        }

        // Check for left and right obstacles
        if (Physics.Raycast(transform.position, -transform.right, out hit, collisionAvoidanceDistance))
            avoidanceVector += transform.right; // Move right if left is blocked
        if (Physics.Raycast(transform.position, transform.right, out hit, collisionAvoidanceDistance))
            avoidanceVector += -transform.right; // Move left if right is blocked

        // Check for above and below obstacles
        if (Physics.Raycast(transform.position, transform.up, out hit, collisionAvoidanceDistance))
            avoidanceVector += -transform.up; // Move downward if something is above
        if (Physics.Raycast(transform.position, -transform.up, out hit, collisionAvoidanceDistance))
            avoidanceVector += transform.up * 1.5f; // Move up if something is below (stronger reaction)

        return avoidanceVector.normalized;
    }

    // Function to get the current speed
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }
    float timeSinceReflect;
    // OnCollisionEnter to reflect the aircraft on collision
    void OnCollisionStay(Collision collision)
    {
        if(timeSinceReflect > 1f)
        {
            // Get the normal of the collision surface
            Vector3 collisionNormal = collision.contacts[0].normal;

            // Reflect the aircraft's velocity based on the collision normal
            rb.velocity = Vector3.Reflect(rb.velocity, collisionNormal);

            // Calculate the reflection of the aircraft's forward vector to adjust orientation
            Quaternion reflectionRotation = Quaternion.FromToRotation(transform.forward, Vector3.Reflect(transform.forward, collisionNormal));
            rb.MoveRotation(rb.rotation * reflectionRotation);
        }
    }
    public void ResetAircraft(Vector3 newPosition, Quaternion newRotation)
    {
        rb.position = newPosition;  // Use Rigidbody to move instantly
        rb.rotation = newRotation;
        rb.velocity = Vector3.zero;  // Stop all movement
        rb.angularVelocity = Vector3.zero;

        currentSpeed = minSpeed;  // Reset speed
    }
}
