using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class GroundGunController : MonoBehaviour
{
    [Header("Gun Components")]
    public Transform horizontalRotation; // Rotates left/right (Yaw)
    public Transform verticalRotation;   // Rotates up/down (Pitch)
    public ParticleSystem fireEffect;    // Particle system for firing
    public AudioSource fireSound;
    public AudioSource farFireSound;
    public Transform target;             // The enemy target

    [Header("Turret Settings")]
    public float rotationSpeed = 5f;     // Speed of rotation towards target
    public float fireRange = 500f;       // The range within which we fire

    // Pitch limits
    public float maxPitchAngle = 60f;    // Maximum pitch angle (upward)
    public float minPitchAngle = -60f;   // Minimum pitch angle (downward)
    private GroundTargetStats stats;
    public bool doLineOfSight = false;

    private void Start()
    {
        stats = GetComponent<GroundTargetStats>();
    }

    private void Update()
    {
        if (target == null) return;

        RotateToTarget();

        bool inRange = Vector3.Distance(transform.position, target.position) <= fireRange;
        bool hasLineOfSight = true;

        if (doLineOfSight)
        {
            Vector3 origin = fireEffect.transform.position; // or use gun barrel
            Vector3 direction = target.position - origin;

            // Check if anything is between us and the target
            if (Physics.Linecast(origin, target.position, out RaycastHit hit))
            {
                hasLineOfSight = hit.transform == target;
            }
        }
        else
        {
            hasLineOfSight = true;
        }

        if (inRange && hasLineOfSight && !stats.isDestroyed)
        {
            if (!fireEffect.isPlaying)
            {
                Fire();
            }
        }
        else
        {
            fireEffect.Stop();
            fireSound.Stop();
        }
    }

    private void RotateToTarget()
    {
        if(horizontalRotation != null)
        {
            // --- Step 1: Calculate the Direction to Target for Horizontal Rotation ---
            Vector3 targetDirection = target.position - horizontalRotation.position;
            targetDirection.y = 0; // We only want to rotate on the XZ plane (horizontal direction)
            Quaternion horizontalRotationGoal = Quaternion.LookRotation(targetDirection);

            // Apply the initial horizontal rotation offset (to compensate for the model's initial rotation)
            horizontalRotation.rotation = Quaternion.Slerp(horizontalRotation.rotation, horizontalRotationGoal, rotationSpeed * Time.deltaTime);
        }

        if (verticalRotation != null)
        {
            // --- Step 2: Calculate the Vertical (Pitch) Angle ---
            // Get the height difference between the target and the turret
            float heightDifference = verticalRotation.position.y - target.position.y;

            // Get the horizontal distance on the XZ plane
            float horizontalDistance = Vector3.Distance(new Vector3(target.position.x, 0f, target.position.z), new Vector3(verticalRotation.position.x, 0f, verticalRotation.position.z));

            // Calculate the pitch angle based on the height difference and horizontal distance
            float pitchAngle = Mathf.Atan2(heightDifference, horizontalDistance) * Mathf.Rad2Deg;

            // Apply the pitch angle, accounting for any initial offset
            verticalRotation.localRotation = Quaternion.Euler(Mathf.Clamp(pitchAngle, -80f, 80f), 0f, 0f);
        }
    }


    private void Fire()
    {
        fireEffect.Play();  // Trigger the firing particle system
        if (!fireSound.isPlaying)
        {
            fireSound.Play();
            farFireSound.Play();
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;  // Update the target
    }
}
