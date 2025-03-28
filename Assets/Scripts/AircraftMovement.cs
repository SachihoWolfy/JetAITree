using System;
using System.Collections.Generic;
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
    public List<AIAircraft> teammates;

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

    public void MoveAircraft(Vector3 desiredDirection, float desiredSpeed, bool doInputScaling = false, bool doErratic = false)
    {
        if (doErratic)
        {
            MoveErratically(desiredDirection, desiredSpeed, doInputScaling);
        }
        else
        {
            MoveSmoothly(desiredDirection, desiredSpeed, doInputScaling);
        }
    }

    private Vector3 SimulateErraticMovement(Vector3 desiredDirection, float desiredSpeed, bool doInputScaling = false)
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

        // --- OBSTACLE AVOIDANCE ---
        Vector3 avoidanceDirection = AvoidObstacles();
        if (avoidanceDirection != Vector3.zero)
        {
            desiredDirection = Vector3.Lerp(desiredDirection, avoidanceDirection, 0.7f).normalized;
        }

        // --- MOVEMENT CONTROL ---
        Vector3 localTargetDir = transform.InverseTransformDirection(desiredDirection);

        // --- ANGLE CONTROL ---
        float angleToDesiredDirection = Vector3.Angle(transform.forward, desiredDirection);
        float inputScale = doInputScaling ? Mathf.InverseLerp(0f, 2.5f, angleToDesiredDirection) : 1f;

        // --- ROLL CONTROL ---
        float desiredYawAngle = Vector3.SignedAngle(transform.forward, desiredDirection, Vector3.up);
        Vector3 cross = Vector3.Cross(transform.forward, desiredDirection);
        float simulatedRollInput = Mathf.Sign(cross.y);
        simulatedRollInput = Mathf.SmoothDamp(rollInputDampingVelocity, simulatedRollInput, ref rollInputDampingVelocity, dampingTime);
        simulatedRollInput *= inputScale * maxRollSpeed * Time.fixedDeltaTime;

        // --- PITCH CONTROL ---
        Vector3 projectedTarget = Vector3.ProjectOnPlane(desiredDirection, transform.right);
        float pitchAngle = Vector3.SignedAngle(transform.forward, projectedTarget, transform.right);
        float simulatedPitchInput = Mathf.Clamp(Mathf.Sign(pitchAngle), -1f, 1f);
        simulatedPitchInput = Mathf.SmoothDamp(pitchInputDampingVelocity, simulatedPitchInput, ref pitchInputDampingVelocity, dampingTime);
        simulatedPitchInput *= inputScale * maxPitchSpeed * Time.fixedDeltaTime;

        // --- YAW CONTROL ---
        float yawAngle = Vector3.SignedAngle(transform.forward, desiredDirection, Vector3.up);
        float simulatedYawInput = Mathf.Clamp(Mathf.Sign(yawAngle), -1f, 1f);
        simulatedYawInput = Mathf.SmoothDamp(yawInputDampingVelocity, simulatedYawInput, ref yawInputDampingVelocity, dampingTime);
        simulatedYawInput *= inputScale * maxYawSpeed * Time.fixedDeltaTime;

        // Simulate the rotation
        Quaternion simulatedDeltaRotation = Quaternion.Euler(simulatedPitchInput, simulatedYawInput, simulatedRollInput);
        Vector3 simulatedNewDirection = simulatedDeltaRotation * transform.forward;

        return simulatedNewDirection.normalized;
    }


    private void MoveSmoothly(Vector3 desiredDirection, float desiredSpeed, bool doInputScaling = false)
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

        // --- ANGLE CONTROL (for damping) ---
        float angleToDesiredDirection = Vector3.Angle(transform.forward, desiredDirection);
        float inputScale = doInputScaling ? Mathf.InverseLerp(0f, 2.5f, angleToDesiredDirection) : 1f;

        // --- ROLL CONTROL (More Aggressive) ---
        Vector3 localCross = Vector3.Cross(Vector3.forward, localTargetDir);
        rollInput = -Mathf.Sign(localCross.y);

        // Reduce damping to allow sharper roll input
        rollInput = Mathf.SmoothDamp(rollInputDampingVelocity, rollInput * 1.5f, ref rollInputDampingVelocity, dampingTime * 0.75f);
        rollInput = Mathf.Clamp(rollInput, -1f, 1f);
        rollInput *= maxRollSpeed * Time.fixedDeltaTime * inputScale;

        // --- PITCH CONTROL (Prioritize if Target is in Front) ---
        Vector3 localProjectedTarget = Vector3.ProjectOnPlane(localTargetDir, Vector3.right);
        float pitchAngle = Vector3.SignedAngle(Vector3.forward, localProjectedTarget, Vector3.right);

        // Prioritize pitch more aggressively when the target is in front
        float pitchMultiplier = (angleToDesiredDirection < 30f) ? 1.2f : 1f;
        pitchInput = Mathf.Clamp(pitchAngle / 30f, -1f, 1f);
        pitchInput = Mathf.SmoothDamp(pitchInputDampingVelocity, pitchInput * pitchMultiplier, ref pitchInputDampingVelocity, dampingTime * 0.75f);

        pitchInput *= maxPitchSpeed * Time.fixedDeltaTime * inputScale;

        // --- YAW CONTROL (Sharpen Turns when Needed) ---
        float yawAngle = Vector3.SignedAngle(Vector3.forward, localTargetDir, Vector3.up);

        // If target is far to the side, prioritize yaw for faster tracking
        float yawWeight = (Mathf.Abs(yawAngle) > 60f) ? 1.5f : 1f;
        yawInput = Mathf.Clamp(yawAngle / 30f, -1f, 1f);
        yawInput = Mathf.SmoothDamp(yawInputDampingVelocity, yawInput * yawWeight, ref yawInputDampingVelocity, dampingTime * 0.75f);

        yawInput *= maxYawSpeed * Time.fixedDeltaTime * inputScale;

        // Apply the rotation based on the adjusted inputs
        Quaternion deltaRotation = Quaternion.Euler(pitchInput, yawInput, rollInput);
        rb.MoveRotation(rb.rotation * deltaRotation);

        // Set the velocity based on the current speed
        rb.velocity = transform.forward * currentSpeed;
    }


    // Old glitchy code because it made for some cool manuevers.
    private void MoveErratically(Vector3 desiredDirection, float desiredSpeed, bool doInputScaling = false)
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

        // --- ANGLE CONTROL ---
        float angleToDesiredDirection = Vector3.Angle(transform.forward, desiredDirection);
        float inputScale = doInputScaling ? Mathf.InverseLerp(0f, 2.5f, angleToDesiredDirection) : 1f;

        // --- ROLL CONTROL ---
        float desiredYawAngle = Vector3.SignedAngle(transform.forward, desiredDirection, Vector3.up);
        Vector3 cross = Vector3.Cross(transform.forward, desiredDirection);
        rollInput = Mathf.Sign(cross.y); // Back to the original roll direction
        rollInput = Mathf.SmoothDamp(rollInputDampingVelocity, rollInput, ref rollInputDampingVelocity, dampingTime);

        // Apply input scaling
        rollInput *= inputScale * maxRollSpeed * Time.fixedDeltaTime;

        // --- PITCH CONTROL ---
        Vector3 projectedTarget = Vector3.ProjectOnPlane(desiredDirection, transform.right);
        float pitchAngle = Vector3.SignedAngle(transform.forward, projectedTarget, transform.right);
        pitchInput = Mathf.Clamp(Mathf.Sign(pitchAngle), -1f, 1f);
        pitchInput = Mathf.SmoothDamp(pitchInputDampingVelocity, pitchInput, ref pitchInputDampingVelocity, dampingTime);

        pitchInput *= inputScale * maxPitchSpeed * Time.fixedDeltaTime;

        // --- YAW CONTROL ---
        float yawAngle = Vector3.SignedAngle(transform.forward, desiredDirection, Vector3.up);
        yawInput = Mathf.Clamp(Mathf.Sign(yawAngle), -1f, 1f);
        yawInput = Mathf.SmoothDamp(yawInputDampingVelocity, yawInput, ref yawInputDampingVelocity, dampingTime);

        yawInput *= inputScale * maxYawSpeed * Time.fixedDeltaTime;

        // Apply the rotation based on the adjusted inputs
        Quaternion deltaRotation = Quaternion.Euler(pitchInput, yawInput, rollInput);
        rb.MoveRotation(rb.rotation * deltaRotation);

        // Set the velocity based on the current speed
        rb.velocity = transform.forward * currentSpeed;
    }



    private Vector3 AvoidObstacles()
    {
        RaycastHit hit;
        Vector3 avoidanceVector = Vector3.zero;

        // --- COLLISION AVOIDANCE VIA RAYCASTS ---
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

        // --- TEAMMATE AVOIDANCE ---
        foreach (AIAircraft teammate in teammates)
        {
            if (teammate == this) continue; // Skip self

            Vector3 toTeammate = teammate.transform.position - transform.position;
            float distance = toTeammate.magnitude;

            if (distance < emergencyAvoidanceDistance) // If within avoidance range
            {
                float weight = Mathf.InverseLerp(collisionAvoidanceDistance, 0, distance); // Closer = stronger repulsion
                avoidanceVector += -toTeammate.normalized * weight;
            }
        }

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
