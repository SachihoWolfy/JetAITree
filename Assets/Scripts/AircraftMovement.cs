using UnityEngine;

public class AircraftMovement : MonoBehaviour
{
    public float minSpeed = 30f;
    public float maxSpeed = 100f;
    public float acceleration = 10f;
    public float currentSpeed;

    public float maxRollSpeed = 80f; 
    public float maxPitchSpeed = 60f;
    public float maxYawSpeed = 10f;   

    public float collisionAvoidanceDistance = 50f;  
    public float emergencyAvoidanceDistance = 20f; 

    private Rigidbody rb;

    private float rollInputDampingVelocity = 0f;
    private float pitchInputDampingVelocity = 0f;
    private float yawInputDampingVelocity = 0f;

    public float dampingTime = 0.3f; 

    public float stallHeight = 1000f;  
    public float stallRecoveryForce = 0.7f; 
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
        desiredSpeed = Mathf.Clamp(desiredSpeed, minSpeed, maxSpeed);

        if (transform.position.y > stallHeight)
        {
            Vector3 correctedDirection = Vector3.Lerp(desiredDirection, Vector3.down, stallRecoveryForce);
            desiredDirection = correctedDirection.normalized;
        }
        if (transform.position.y < groundAvoidHeight)
        {
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

        rollInput *= maxRollSpeed * Time.fixedDeltaTime;
        pitchInput *= maxPitchSpeed * Time.fixedDeltaTime;
        yawInput *= maxYawSpeed * Time.fixedDeltaTime;

        Quaternion deltaRotation = Quaternion.Euler(pitchInput, yawInput, rollInput);
        rb.MoveRotation(rb.rotation * deltaRotation);

        rb.velocity = transform.forward * currentSpeed;
    }


    private Vector3 AvoidObstacles()
    {
        RaycastHit hit;
        Vector3 avoidanceVector = Vector3.zero;

        if (Physics.Raycast(transform.position, transform.forward, out hit, collisionAvoidanceDistance))
        {
            float urgency = hit.distance < emergencyAvoidanceDistance ? 1.0f : 0.5f;
            avoidanceVector += -transform.forward * urgency; 
        }

        if (Physics.Raycast(transform.position, -transform.right, out hit, collisionAvoidanceDistance))
            avoidanceVector += transform.right; 
        if (Physics.Raycast(transform.position, transform.right, out hit, collisionAvoidanceDistance))
            avoidanceVector += -transform.right; 

        if (Physics.Raycast(transform.position, transform.up, out hit, collisionAvoidanceDistance))
            avoidanceVector += -transform.up; 
        if (Physics.Raycast(transform.position, -transform.up, out hit, collisionAvoidanceDistance))
            avoidanceVector += transform.up * 1.5f; 

        return avoidanceVector.normalized;
    }

    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }
    float timeSinceReflect;
    void OnCollisionStay(Collision collision)
    {
        if(timeSinceReflect > 1f)
        {
            Vector3 collisionNormal = collision.contacts[0].normal;

            rb.velocity = Vector3.Reflect(rb.velocity, collisionNormal);

            Quaternion reflectionRotation = Quaternion.FromToRotation(transform.forward, Vector3.Reflect(transform.forward, collisionNormal));
            rb.rotation = rb.rotation * reflectionRotation;
        }
    }
    public void ResetAircraft(Vector3 newPosition, Quaternion newRotation)
    {
        rb.position = newPosition; 
        rb.rotation = newRotation;
        rb.velocity = Vector3.zero; 
        rb.angularVelocity = Vector3.zero;

        currentSpeed = minSpeed;
    }
}
