public class BTEngagePursuer : BTNode
{
    private AIAircraft ai;

    public BTEngagePursuer(AIAircraft ai)
    {
        this.ai = ai;
    }

    public override bool Execute()
    {
        ai.currentState = "Trying to engage Pursuer";
        ai.FindBestTarget();
        if (ai.target != null && !ai.IsInDanger())
        {
            return true;
        }
        return false;
    }
}
