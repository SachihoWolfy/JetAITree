using UnityEngine;
using UnityEditor;
using UnityEditor.TerrainTools;
public class GroundTargetStats : Stats
{
    public Transform spawnPoint;
    private Vector3 spawnPointPosition;
    private Quaternion spawnPointRotation;
    public float respawnRadius = 10f;
    public float respawnTime = 5f;
    public bool canRespawn = true;
    public bool isDestroyed = false;
    public Stats target;

    public ParticleSystem fireParticles;
    public ParticleSystem smokeParticles;
    [SerializeField] public bool generateRandomName = true;

    protected override void Start()
    {
        base.Start();
        if (generateRandomName)
        {
            GenerateName();
        }
        else
        {
            UpdateID(ID);
        }
        if (spawnPoint == null)
        {
            spawnPointPosition = transform.position;
            spawnPointRotation = transform.rotation;
        }
        else
        {
            spawnPointPosition = spawnPoint.position;
            spawnPointRotation = spawnPoint.rotation;
        }
    }

    public override void TakeDamage(int damage)
    {
        if (isDestroyed) return;

        base.TakeDamage(damage);

        if(hp <= criticalHP)
        {
            smokeParticles.Play();
        }

        if (hp <= 0)
        {
            hp = 0;
            isDestroyed = true;
            EnterDestroyedState();
        }
    }

    private void GenerateName()
    {
        ID = NVJOBNameGen.GiveAName(3);
        gameObject.name = ID;
    }

    private void EnterDestroyedState()
    {
        fireParticles.Play();
        invincible = true;
        if (canRespawn)
        {
            Invoke(nameof(Respawn), respawnTime);
        }
        else
        {
            Debug.Log(ID + " is destroyed and must be repaired!");
        }
    }

    public void Respawn()
    {
        if (!canRespawn) return;
        Invoke(nameof(StopInvincibility), 5);

        fireParticles.Stop();
        smokeParticles.Stop();
        isDestroyed = false;
        hp = maxHP;

        Vector3 randomOffset = Random.insideUnitSphere * respawnRadius;
        randomOffset.y = 0;

        transform.position = spawnPointPosition + randomOffset;
        transform.rotation = spawnPointRotation;
        Debug.Log(ID + " has respawned!");
    }

    [ContextMenu("Repair Ground Target")]
    public void Repair()
    {
        Repair(maxHP);
    }
    [ContextMenu("Kill Ground Target")]
    public void Kill()
    {
        TakeDamage(maxHP);
    }

    public void Repair(int repairAmount)
    {
        if (!isDestroyed) return;

        hp += repairAmount;
        if (hp >= maxHP)
        {
            invincible = false;
            fireParticles.Stop();
            smokeParticles.Stop();
            hp = maxHP;
            isDestroyed = false;
            Debug.Log(ID + " has been fully repaired!");
        }
    }
}