using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AircraftStats : MonoBehaviour
{
    public int hp = 20;
    public AudioSource hitAudio;
    public GameObject explodePrefab;

    private AIAircraft aircraft;
    private InfoCanvasController infoCanvasController;
    public ParticleSystem smoke;
    public ParticleSystem hitSystem;
    private int maxHP;
    private int criticalHP;
    public bool invincible;

    void Start()
    {
        aircraft = GetComponent<AIAircraft>();
        infoCanvasController = FindObjectOfType<InfoCanvasController>();
        maxHP = hp;
        criticalHP = hp / 4;
    }
    void stopInvicibility()
    {
        invincible = false;
    }
    public void TakeDamage(int damage)
    {
        if (invincible)
        {
            return;
        }
        hp -= damage;
        hitSystem.Play();
        if (!hitAudio.isPlaying)
        {
            hitAudio.pitch = Random.Range(0.8f, 1.2f);
            hitAudio.PlayOneShot(hitAudio.clip);
        }
        if(hp <= criticalHP)
        {
            smoke.Play();
        }

        if (hp <= 0)
        {
            Explode();
            smoke.Stop();
            infoCanvasController.AddKill(aircraft.team);
            Respawn();
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Damagable"))
        {
            TakeDamage(20);
        }
    }

    void Explode()
    {
        if (explodePrefab != null)
        {
            Instantiate(explodePrefab, transform.position, transform.rotation);
        }
    }

    void Respawn()
    {
        invincible = true;
        aircraft.Respawn();
        hp = 20;
        Invoke("stopInvicibility", 5f);
    }
}
