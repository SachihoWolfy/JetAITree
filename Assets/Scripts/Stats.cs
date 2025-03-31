using UnityEngine;

public class Stats : MonoBehaviour
{
    public int hp = 20;
    public bool invincible;
    public AudioSource hitAudio;
    public GameObject explodePrefab;
    public ParticleSystem hitParticles;
    public int kills = 0;
    public Team team;
    public string ID;

    protected int maxHP;
    protected int criticalHP;

    protected virtual void Start()
    {
        maxHP = hp;
        criticalHP = hp / 4;
    }

    public virtual void TakeDamage(int damage)
    {
        if (invincible)
        {
            return;
        }

        hp -= damage;
        hitParticles.Play();


        if (hitAudio && !hitAudio.isPlaying)
        {
            hitAudio.pitch = Random.Range(0.8f, 1.2f);
            hitAudio.PlayOneShot(hitAudio.clip);
        }

        if (hp <= 0)
        {
            Explode();
        }
    }
    public void StopInvincibility()
    {
        invincible = false;
    }
    public void UpdateID(string desiredID)
    {
        ID = desiredID;
        gameObject.name = ID;
    }
    protected virtual void Explode()
    {
        if (explodePrefab)
        {
            Instantiate(explodePrefab, transform.position, transform.rotation);
        }
    }
}
