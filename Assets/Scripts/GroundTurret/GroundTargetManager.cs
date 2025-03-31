using System.Collections.Generic;
using UnityEngine;

public class GroundTargetManager : MonoBehaviour
{
    public static GroundTargetManager Instance { get; private set; }

    public List<GroundTargetStats> allGroundTargets = new List<GroundTargetStats>();
    public List<AircraftStats> allAirTargets = new List<AircraftStats>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        FindAllGroundTargets();
        FindAllAirTargets();
    }

    private void FindAllGroundTargets()
    {
        allGroundTargets.Clear();
        allGroundTargets.AddRange(FindObjectsOfType<GroundTargetStats>());
    }

    private void FindAllAirTargets()
    {
        allAirTargets.Clear();
        allAirTargets.AddRange(FindObjectsOfType<AircraftStats>());
    }
    public void RepairTarget(string targetID, int repairAmount)
    {
        foreach (var target in allGroundTargets)
        {
            if (target.ID == targetID && target.isDestroyed)
            {
                target.Repair(repairAmount);
                return;
            }
        }
        Debug.LogWarning("No destroyed ground target with ID: " + targetID);
    }

    public void RespawnAll()
    {
        foreach (var target in allGroundTargets)
        {
            if (target.isDestroyed && target.canRespawn)
            {
                target.Respawn();
            }
        }
    }
    public GroundTargetStats GetRandomEnemyGroundTarget(Team team)
    {
        List<GroundTargetStats> enemyTargets = new List<GroundTargetStats>();

        foreach (var target in allGroundTargets)
        {
            if (target.team != team && !target.isDestroyed)
            {
                enemyTargets.Add(target);
            }
        }

        if (enemyTargets.Count == 0)
        {
            foreach (var target in allGroundTargets)
            {
                if (target.team != team)
                {
                    return target;
                }
            }
        }

        return enemyTargets[Random.Range(0, enemyTargets.Count)];
    }

}
