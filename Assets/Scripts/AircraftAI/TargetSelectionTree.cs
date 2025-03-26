using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

public class AITargetSelectionTree : MonoBehaviour
{
    public AIAircraft aircraft;
    private BTSelector targetSelectionTree;
    public float safetyValue = 0.5f;
    public float confidenceValue = 0.5f;
    private float maxSafety = 1.0f;
    private float maxConfidence = 1.0f;
    private float safetyIncreaseRate = 0.1f;
    private float safetyDecayRate = 0.2f;
    private float confidenceIncreaseRate = 0.1f;
    private float confidenceDecayRate = 0.05f;

    public string CurrentStatus = "Idle";

    private float timeSinceLastSwitch = 0.0f;
    private float targetSwitchDelay = 10.0f;
    void Start()
    {
        aircraft = GetComponent<AIAircraft>();
        targetSelectionTree = BuildTargetSelectionTree();
    }

    void FixedUpdate()
    {
        UpdateSafetyAndConfidence();
        // Update time since last target switch
        timeSinceLastSwitch += Time.deltaTime;

        // Only execute the tree if the cooldown period has passed
        if (timeSinceLastSwitch >= targetSwitchDelay)
        {
            targetSelectionTree.Execute();
        }
    }

    private void UpdateSafetyAndConfidence()
    {
        float maxdot = 0f;
        foreach (var threat in aircraft.GetThreats())
        {
            Vector3 directionToAI = (aircraft.transform.position - threat.transform.position).normalized;
            float dotProduct = Vector3.Dot(threat.transform.forward, directionToAI);
            if (dotProduct > maxdot)
            {
                maxdot = dotProduct;
            }
        }
        if (maxdot > 0.7f)
        {
            safetyValue = Mathf.Clamp(safetyValue - Time.deltaTime * safetyDecayRate * aircraft.threats.Count, 0, 1);
        }
        else
        {
            safetyValue = Mathf.Clamp(safetyValue + Time.deltaTime * safetyIncreaseRate, 0, 1);
        }

        if (aircraft.target != null)
        {
            Vector3 directionToTarget = (aircraft.target.position - aircraft.transform.position).normalized;
            float dotProduct = Vector3.Dot(aircraft.transform.forward, directionToTarget);
            if (dotProduct > 0.7f)
            {
                confidenceValue = Mathf.Clamp(confidenceValue + Time.deltaTime * confidenceIncreaseRate, 0f, maxConfidence);
            }
            else
            {
                confidenceValue = Mathf.Clamp(confidenceValue - Time.deltaTime * confidenceDecayRate, 0f, maxConfidence);
            }

        }
    }

    private BTSelector BuildTargetSelectionTree()
    {
        BTSelector root = new BTSelector();

        // Manage Target Selector
        BTSelector manageTargetSelector = new BTSelector();

        // Keep Current Target Sequence
        BTSequence keepCurrentTarget = new BTSequence();
        keepCurrentTarget.AddChild(new BTCondition(() => aircraft.target != null && confidenceValue > 0.7f && safetyValue > 0.5f));
        keepCurrentTarget.AddChild(new BTAction(() => { CurrentStatus = "Keeping Current Target"; }));

        // Switch Target Selector
        BTSelector switchTargetSelector = new BTSelector();

        // Switch to Pursuer Sequence
        BTSequence switchToPursuer = new BTSequence();
        switchToPursuer.AddChild(new BTCondition(() => safetyValue < 0.3f));
        switchToPursuer.AddChild(new BTAction(() => {
            timeSinceLastSwitch = 0.0f;
            aircraft.target = FindBestPursuer();
            CurrentStatus = "Switching to Pursuer";
        }));

        // Switch to Threatening Teammate's Enemy Sequence
        BTSequence switchToThreateningTeammate = new BTSequence();
        switchToThreateningTeammate.AddChild(new BTCondition(() => GetLeastSafeTeammate() != null));
        switchToThreateningTeammate.AddChild(new BTAction(() => {
            timeSinceLastSwitch = 0.0f;
            aircraft.target = FindThreateningEnemy(GetLeastSafeTeammate());
            CurrentStatus = "Switching to Threatening Teammate's Enemy";
        }));

        // Switch to Best Target Sequence
        BTSequence switchToBestTarget = new BTSequence();
        switchToBestTarget.AddChild(new BTCondition(() => aircraft.target == null || confidenceValue < 0.3f));
        switchToBestTarget.AddChild(new BTAction(() => {
            timeSinceLastSwitch = 0.0f;
            aircraft.target = FindBestTarget();
            CurrentStatus = "Switching to Best Target";
        }));

        // Ensure Initial Target Sequence
        BTSequence ensureInitialTarget = new BTSequence();
        ensureInitialTarget.AddChild(new BTCondition(() => aircraft.target == null));
        ensureInitialTarget.AddChild(new BTAction(() => {
            timeSinceLastSwitch = 0.0f;
            aircraft.target = FindBestTarget();
            CurrentStatus = "Ensuring Initial Target";
        }));

        // Clear Target if Fully Safe Sequence
        BTSequence clearTargetIfSafe = new BTSequence();
        clearTargetIfSafe.AddChild(new BTCondition(() => safetyValue >= maxSafety));
        clearTargetIfSafe.AddChild(new BTAction(() => {
            timeSinceLastSwitch = 0.0f;
            aircraft.target = null;
            CurrentStatus = "Clearing Target - Fully Safe";
        }));

        // Add sequences to selectors
        manageTargetSelector.AddChild(keepCurrentTarget);
        switchTargetSelector.AddChild(switchToPursuer);
        switchTargetSelector.AddChild(switchToThreateningTeammate);
        switchTargetSelector.AddChild(switchToBestTarget);

        // Combine selectors and sequences
        root.AddChild(manageTargetSelector);
        root.AddChild(switchTargetSelector);
        root.AddChild(ensureInitialTarget);
        root.AddChild(switchToThreateningTeammate);
        root.AddChild(clearTargetIfSafe);

        return root;
    }



    private Transform FindBestPursuer()
    {
        return aircraft.GetThreats().OrderByDescending(t => Vector3.Dot(t.transform.forward, (aircraft.transform.position - t.transform.position).normalized)).FirstOrDefault()?.transform;
    }

    private AIAircraft GetLeastSafeTeammate()
    {
        // Find the first teammate that is in danger or has a threat tied to them
        var leastSafeTeammate = aircraft.teammates
            .Where(t => t.IsInDanger()) // Filter only teammates that are in danger
            .OrderBy(t => t.GetThreats().Count) // Order by the number of threats tied to them (if any)
            .FirstOrDefault(); // Get the first teammate in danger with threats (or null if none)

        return leastSafeTeammate; // Will return null if no teammates in danger or with threats
    }


    private Transform FindThreateningEnemy(AIAircraft teammate)
    {
        return aircraft.allAircraft.Where(a => a.team != aircraft.team && a.target == teammate.transform).FirstOrDefault().transform;
    }

    private Transform FindBestTarget()
    {
        List<AIAircraft> enemies = aircraft.allAircraft.Where(a => a.team != aircraft.team).ToList();
        return enemies.OrderByDescending(e => Vector3.Distance(aircraft.transform.position, e.transform.position)).FirstOrDefault().transform;
    }
}
