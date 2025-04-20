using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BomberAI : AIAircraft
{
    public override void EngageDogfight()
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
            aircraftMovement.MoveAircraft(directionToTarget, aircraftMovement.maxSpeed, false, false);
        }
    }
    public override void PerformStrafingRun()
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
            float threshold = 0.9f;

            if (dotProduct >= threshold && distanceToTarget < 300f)
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
}
