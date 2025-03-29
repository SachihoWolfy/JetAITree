using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundTarget : MonoBehaviour
{
    public Team team;
    [SerializeField]
    public bool generateRandomName = true;
    public string ID;
    public GroundTargetStats stats;

    private void Start()
    {
        stats = GetComponent<GroundTargetStats>();
        if (generateRandomName)
        {
            GenerateName();
        }
        gameObject.name = ID;
        stats.ID = ID;
        stats.team = team;
    }
    private void GenerateName()
    {
        ID = NVJOBNameGen.GiveAName(3);
    }
}