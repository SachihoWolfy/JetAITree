using System.Collections.Generic;
using UnityEngine;

public class GroundTargetManager : MonoBehaviour
{
    public static GroundTargetManager Instance { get; private set; }

    public List<GroundTargetStats> allGroundTargets = new List<GroundTargetStats>();

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
    }

    private void FindAllGroundTargets()
    {
        allGroundTargets.Clear();
        GroundTargetStats[] targets = FindObjectsOfType<GroundTargetStats>();

        foreach (var target in targets)
        {
            allGroundTargets.Add(target);
        }

        Debug.Log("Found " + allGroundTargets.Count + " ground targets.");
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
}
