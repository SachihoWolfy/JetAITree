using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlSurfaces : MonoBehaviour
{
    [Header("Control Surfaces")]
    public Transform leftElevator;
    public Transform rightElevator;
    public Transform leftAileron;
    public Transform rightAileron;
    public Transform rudder;

    [Header("Deflection Angles")]
    public float maxElevatorDeflection = 20f; // Degrees
    public float maxAileronDeflection = 15f;  // Degrees
    public float maxRudderDeflection = 25f;   // Degrees

    private AircraftMovement aircraft;

    void Start()
    {
        aircraft = GetComponent<AircraftMovement>();
    }

    void Update()
    {
        float pitchInput = aircraft.pitchInput;
        float rollInput = aircraft.rollInput;
        float yawInput = aircraft.yawInput;

        // Apply rotations
        AnimateSurfaces(pitchInput, rollInput, yawInput);
    }

    void AnimateSurfaces(float pitch, float roll, float yaw)
    {
        // Elevators (Pitch Control) - Rotate around Z-axis
        if (leftElevator != null)
            leftElevator.localRotation = Quaternion.Euler(0, 0, pitch + (roll/2) * maxElevatorDeflection);

        if (rightElevator != null)
            rightElevator.localRotation = Quaternion.Euler(0, 0, pitch - (roll/2) * maxElevatorDeflection);

        // Ailerons (Roll Control) - Rotate around Z-axis (Opposite Directions)
        if (leftAileron != null)
            leftAileron.localRotation = Quaternion.Euler(0, 0, roll * maxAileronDeflection);

        if (rightAileron != null)
            rightAileron.localRotation = Quaternion.Euler(0, 0, -roll * maxAileronDeflection);

        // Rudder (Yaw Control) - Rotate around Y-axis
        if (rudder != null)
            rudder.localRotation = Quaternion.Euler(0, -yaw * maxRudderDeflection, 0);
    }
}
