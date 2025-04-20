using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;

public class TargetSelectionTree : MonoBehaviour
{
    public AIAircraft aircraft;
    private BTSelector targetSelectionTree;
    public float safetyValue = 0.5f;
    public float confidenceValue = 0.5f;
    private float maxDistance = 500f;
    protected float maxSafety = 1.0f;
    private float maxConfidence = 1.0f;
    private float safetyIncreaseRate = 0.05f;
    private float safetyDecayRate = 0.2f;
    private float confidenceIncreaseRate = 0.1f;
    private float confidenceDecayRate = 0.05f;

    public string currentState = "Idle";

    protected float timeSinceLastSwitch = 0.0f;
    protected float targetSwitchDelay = 10.0f;

    protected AIAircraft targetAI;

    public TargetingStatus status;
    public virtual void Start()
    {
        aircraft = GetComponent<AIAircraft>();
        targetSelectionTree = BuildTargetSelectionTree();
    }

    void FixedUpdate()
    {
        aircraft.t_status = status;
        UpdateSafetyAndConfidence();
        timeSinceLastSwitch += Time.deltaTime;

        if (timeSinceLastSwitch >= targetSwitchDelay)
        {
            targetSelectionTree.Execute();
            if(aircraft.target!=null)
            targetAI = aircraft.target.GetComponent<AIAircraft>();
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
            if (dotProduct > 0.8f && CheckDistance(aircraft.target))
            {
                confidenceValue = Mathf.Clamp(confidenceValue + Time.deltaTime * confidenceIncreaseRate, 0f, maxConfidence);
            }
            else
            {
                confidenceValue = Mathf.Clamp(confidenceValue - Time.deltaTime * confidenceDecayRate, 0f, maxConfidence);
            }

        }
    }

    public virtual BTSelector BuildTargetSelectionTree()
    {
        // Lets lay this out. First the root.
        BTSelector root = new BTSelector();

        // Manage Target Selector
        BTSelector manageTargetSelector = new BTSelector();

        // Keep Current Target Sequence
        BTSequence keepCurrentTarget = new BTSequence();
        keepCurrentTarget.AddChild(new BTCondition(() => ((aircraft.target != null && confidenceValue > 0.8f && safetyValue > 0.5f) || FindBestTarget() == aircraft.target) && (targetAI.threats.Count() <= 3)));
        keepCurrentTarget.AddChild(new BTAction(() => { 
            currentState = "Keeping Current Target";
            status = TargetingStatus.KeepTarget;
        }));

        // Switch Target Selector
        BTSelector switchTargetSelector = new BTSelector();

        // Switch to Pursuer Sequence
        BTSequence switchToPursuer = new BTSequence();
        switchToPursuer.AddChild(new BTCondition(() => safetyValue < 0.3f && CheckDistance(FindBestPursuer())));
        switchToPursuer.AddChild(new BTAction(() => {
            timeSinceLastSwitch = 0.0f;
            aircraft.target = FindBestPursuer();
            currentState = "Switching to Pursuer";
            status = TargetingStatus.Pursuer;
        }));

        // Switch to Threatening Teammate's Enemy Sequence
        BTSequence switchToThreateningTeammate = new BTSequence();
        switchToThreateningTeammate.AddChild(new BTCondition(() => GetLeastSafeTeammate() != null));
        switchToThreateningTeammate.AddChild(new BTCondition(() => CheckDistance(FindThreateningEnemy(GetLeastSafeTeammate()))||GetLeastSafeTeammate().threats.Count>2));
        switchToThreateningTeammate.AddChild(new BTAction(() => {
            timeSinceLastSwitch = 0.0f;
            aircraft.target = FindThreateningEnemy(GetLeastSafeTeammate());
            currentState = "Switching to Threatening Teammate's Enemy";
            status = TargetingStatus.AllyDefense;
        }));

        // Switch to Best Target Sequence
        BTSequence switchToBestTarget = new BTSequence();
        switchToBestTarget.AddChild(new BTCondition(() => (aircraft.target != null && CheckDistance(FindBestTarget()))));
        switchToBestTarget.AddChild(new BTAction(() => {
            timeSinceLastSwitch = 0.0f;
            aircraft.target = FindBestTarget();
            currentState = "Switching to Best Target";
            status = TargetingStatus.Best;
        }));

        // Ensure that no one on the enemy team is strafing.
        BTSequence targetStrafers = new BTSequence();
        targetStrafers.AddChild(new BTCondition(() => FindStrafingEnemy() != null));
        targetStrafers.AddChild(new BTAction(() =>
        {
            timeSinceLastSwitch = 0.0f;
            aircraft.target = FindStrafingEnemy();
            currentState = "Defending Against Strafers";
            status = TargetingStatus.Strafers;
        }));

        // Clear Target if Fully Safe Sequence
        BTSequence clearTargetIfSafe = new BTSequence();
        clearTargetIfSafe.AddChild(new BTCondition(() => safetyValue >= maxSafety));
        clearTargetIfSafe.AddChild(new BTAction(() => {
            timeSinceLastSwitch = 0.0f;
            aircraft.target = null;
            currentState = "Clearing Target - Fully Safe";
            status = TargetingStatus.Ground;
            aircraft.groundTarget = GroundTargetManager.Instance.GetRandomEnemyGroundTarget(aircraft.team).transform;
        }));

        // Add sequences to selectors
        manageTargetSelector.AddChild(keepCurrentTarget);
        switchTargetSelector.AddChild(switchToPursuer);
        switchTargetSelector.AddChild(switchToThreateningTeammate);
        switchTargetSelector.AddChild(switchToBestTarget);
        switchTargetSelector.AddChild(targetStrafers);

        // Combine selectors and sequences. This is the tree. Hecc yeah!
        root.AddChild(manageTargetSelector);
        root.AddChild(switchTargetSelector);
        root.AddChild(switchToThreateningTeammate);
        root.AddChild(clearTargetIfSafe);

        return root;
    }

    float GetDistance(Transform target)
    {
        float distance = 1000000.0f;
        if (target != null)
        {
            distance = Vector3.Distance(this.transform.position, target.transform.position);
        }
        return distance;
    }
    protected bool CheckDistance(Transform target)
    {
        return GetDistance(target) < maxDistance;
    }
    protected Transform FindBestPursuer()
    {
        return aircraft.threats.OrderByDescending(t => Vector3.Dot(t.transform.forward, (aircraft.transform.position - t.transform.position).normalized)).FirstOrDefault()?.transform;
    }

    protected AIAircraft GetLeastSafeTeammate()
    {
        var leastSafeTeammate = aircraft.teammates
            .Where(t => t.IsInDanger() && t.threats.Count>0) 
            .OrderBy(t => t.threats.Count)
            .FirstOrDefault(); 

        return leastSafeTeammate; 
    }


    protected Transform FindThreateningEnemy(AIAircraft teammate)
    {
        List<AIAircraft> enemies = aircraft.enemies;
        AIAircraft potentialTarget = null;
        if(enemies.Where(a => a.team != aircraft.team && a.target == teammate.transform).Count() < 1)
        {
            float maxEnemyConfidence = 0f;
            foreach(var enemy in enemies)
            {
                float enemyConfidence = enemy.GetComponent<TargetSelectionTree>().confidenceValue;
                if (enemyConfidence > maxEnemyConfidence)
                {
                    maxEnemyConfidence = enemyConfidence;
                    potentialTarget = enemy;
                }
            }
        }
        else
        {
            potentialTarget = enemies.Where(a => a.team != aircraft.team && a.target == teammate.transform).First();
        }
        return potentialTarget.transform;
    }
    protected Transform FindStrafingEnemy()
    {
        foreach (AIAircraft enemy in aircraft.enemies)
        {
            if (enemy.strafing && enemy.threats.Count == 0)
            {
                return enemy.transform;
            }
        }
        return null;
    }

    protected Transform FindBestTarget()
    {
        var target = aircraft.FindBestTarget();
        if (target == null)
        {
            return target;
        }
        return target.transform;
    }
    /*
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Handles.Label(transform.position + Vector3.up * -3 + Vector3.right * 3, currentState);
        }
    }
    */
}
