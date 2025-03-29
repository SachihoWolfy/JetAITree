using System.Runtime;

public class BTEngagePursuer : BTNode
{
    private AIAircraft ai;

    public BTEngagePursuer(AIAircraft ai)
    {
        this.ai = ai;
    }

    public override bool Execute()
    {
        if (ai.IsInDanger()&& ai.target != ai.threats[0] && ai.threats[0]!=null)
        {
            ai.currentState = "Trying to engage Pursuer";
            ai.TargetPursuer();
            return true;
        }
        return false;
    }
}
