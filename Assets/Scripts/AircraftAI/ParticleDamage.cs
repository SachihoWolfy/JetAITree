using System.Collections.Generic;
using UnityEngine;

public class ParticleDamage : MonoBehaviour
{
    public int damageAmount = 20;
    public AircraftStats ourStats;

    private ParticleSystem partSystem;
    private ParticleSystem.Particle[] particles;
    private List<ParticleCollisionEvent> collisionEvents;

    void Start()
    {
        partSystem = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Damagable")) 
        {
            AircraftStats stats = other.GetComponent<AircraftStats>();
            if (stats != null && stats != ourStats && stats.GetComponent<AIAircraft>().team != ourStats.GetComponent<AIAircraft>().team)
            {
                if (stats.hp - damageAmount <= 0)
                {
                    ourStats.aircraft.kills++;
                }
                stats.TakeDamage(damageAmount);
            }
        }
    }
}
