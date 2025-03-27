using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AircraftStats : MonoBehaviour
{
    public int hp = 20;
    public AudioSource hitAudio;
    public GameObject explodePrefab;

    private AIAircraft aircraft;

    void Start()
    {
        aircraft = GetComponent<AIAircraft>();
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;

        if (!hitAudio.isPlaying)
        {
            hitAudio.pitch = Random.Range(0.8f, 1.2f); // Randomize pitch between 0.8 and 1.2
            hitAudio.PlayOneShot(hitAudio.clip);
        }

        if (hp <= 0)
        {
            Explode();
            Respawn();
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
        aircraft.Respawn();
        hp = 20;
    }
}
