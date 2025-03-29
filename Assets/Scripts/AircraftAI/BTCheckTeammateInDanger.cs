using UnityEngine;

public class BTCheckTeammateInDanger : BTNode
{
    private AIAircraft ai;

    public BTCheckTeammateInDanger(AIAircraft ai)
    {
        this.ai = ai;
    }

    public override bool Execute()
    {
        float minDistance = 10000f;
        Transform curThreat = null;
        foreach (var teammate in ai.teammates)
        {
            if (teammate.IsInDanger())
            {
                if((teammate.threats[0].transform.position - ai.transform.position).magnitude < minDistance)
                {
                    minDistance = (teammate.threats[0].transform.position - ai.transform.position).magnitude;
                    curThreat = teammate.threats[0].transform;
                }
                
            }
            if(curThreat != null)
            {
                return true;
            }
        }
        return false;
    }
}
