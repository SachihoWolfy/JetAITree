using UnityEngine;

public class GroundTargetStats : Stats
{
    public Transform spawnPoint;
    public float respawnRadius = 10f;
    public bool canRespawn = true;
    public bool isDestroyed = false;

    public string ID = "";
    public Team team;
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
    }

    public override void TakeDamage(int damage)
    {
        if (isDestroyed) return;

        base.TakeDamage(damage);

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
        if (canRespawn)
        {
            Invoke(nameof(Respawn), 5f);
        }
        else
        {
            Debug.Log(ID + " is destroyed and must be repaired!");
        }
    }

    public void Respawn()
    {
        if (!canRespawn) return;

        isDestroyed = false;
        hp = maxHP;

        Vector3 randomOffset = Random.insideUnitSphere * respawnRadius;
        randomOffset.y = 0;

        transform.position = spawnPoint.position + randomOffset;
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
            hp = maxHP;
            isDestroyed = false;
            Debug.Log(ID + " has been fully repaired!");
        }
    }

    public void UpdateID(string desiredID)
    {
        if (!string.Equals(ID, desiredID))
        {
            ID = desiredID;
        }
        gameObject.name = ID;
    }
}
