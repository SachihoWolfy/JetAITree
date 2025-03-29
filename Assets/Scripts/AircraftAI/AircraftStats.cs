using UnityEngine;

public class AircraftStats : Stats
{
    public AIAircraft aircraft;
    private InfoCanvasController infoCanvasController;
    public ParticleSystem smoke;

    public Team team;
    public string ID = "";
    public int kills = 0;

    protected override void Start()
    {
        base.Start();
        aircraft = GetComponent<AIAircraft>();
        infoCanvasController = FindObjectOfType<InfoCanvasController>();
        infoCanvasController.UpdateTopThreeList();
    }

    private void FixedUpdate()
    {
        UpdateStats();
    }

    void UpdateStats()
    {
        UpdateID(aircraft.aircraftID);
        UpdateKills(aircraft.kills);
    }

    public void UpdateID(string desiredID)
    {
        if (!string.Equals(ID, desiredID))
        {
            ID = desiredID;
            gameObject.name = ID;
        }
    }

    public void UpdateKills(int desiredKills)
    {
        kills = desiredKills;
    }

    void StopInvincibility()
    {
        invincible = false;
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);

        if (hp <= criticalHP)
        {
            smoke.Play();
        }

        if (hp <= 0)
        {
            smoke.Stop();
            infoCanvasController.AddKill(aircraft.team);
            Respawn();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Damagable"))
        {
            TakeDamage(20);
        }
    }

    void Respawn()
    {
        invincible = true;
        aircraft.Respawn();
        hp = maxHP;
        Invoke(nameof(StopInvincibility), 5f);
        infoCanvasController.UpdateTopThreeList();
    }
}
