using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BomberTargetting : TargetSelectionTree
{
    public override void Start()
    {
        base.Start();
        targetSwitchDelay = 5.0f;
    }
    public override BTSelector BuildTargetSelectionTree()
    {
        // Lets lay this out. First the root.
        BTSelector root = new BTSelector();

        // Manage Target Selector
        BTSelector manageTargetSelector = new BTSelector();

        // Keep Current Target Sequence
        BTSequence keepCurrentTarget = new BTSequence();
        keepCurrentTarget.AddChild(new BTCondition(() => ((aircraft.target != null && confidenceValue > 0.8f && safetyValue > 0.5f) || FindBestTarget() == aircraft.target) && (targetAI.threats.Count <= 3)));
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
        switchToThreateningTeammate.AddChild(new BTCondition(() => CheckDistance(FindThreateningEnemy(GetLeastSafeTeammate())) || GetLeastSafeTeammate().threats.Count > 2));
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
        clearTargetIfSafe.AddChild(new BTCondition(() => safetyValue >= 0));
        clearTargetIfSafe.AddChild(new BTAction(() => {
            timeSinceLastSwitch = 0.0f;
            aircraft.target = null;
            currentState = "Clearing Target - Fully Safe";
            status = TargetingStatus.Ground;
            aircraft.groundTarget = GroundTargetManager.Instance.GetRandomEnemyGroundTarget(aircraft.team).transform;
        }));

        // Combine selectors and sequences. This is the tree. Hecc yeah!
        root.AddChild(manageTargetSelector);
        root.AddChild(switchTargetSelector);
        root.AddChild(switchToPursuer);
        root.AddChild(clearTargetIfSafe);

        return root;
    }
}
