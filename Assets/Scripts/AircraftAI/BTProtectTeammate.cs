public class BTProtectTeammate : BTNode
{
    private AIAircraft ai;

    public BTProtectTeammate(AIAircraft ai)
    {
        this.ai = ai;
    }

    public override bool Execute()
    {
        if (ai.target != null && !ai.IsInDanger())
        {
            ai.currentState = "Protecting";
            ai.EngageDogfight();
            return true;
        }
        return false;
    }
}
