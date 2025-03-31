using System.Collections.Generic;
using UnityEngine;

public class ParticleDamage : MonoBehaviour
{
    public int damageAmount = 20;
    public Stats ourStats;

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
            Stats otherStats = other.GetComponent<Stats>();
            if (otherStats != null && otherStats != ourStats && otherStats.team != ourStats.team)
            {
                if (otherStats.hp - damageAmount <= 0 && !otherStats.invincible)
                {
                    ourStats.kills++;
                }
                otherStats.TakeDamage(damageAmount);
            }
        }
    }
}
