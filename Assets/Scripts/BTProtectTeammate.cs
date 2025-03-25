public class BTProtectTeammate : BTNode
{
    private AIAircraft ai;

    public BTProtectTeammate(AIAircraft ai)
    {
        this.ai = ai;
    }

    public override bool Execute()
    {
        ai.currentState = "Protecting";
        ai.FindBestTarget();
        if (ai.target != null && !ai.IsInDanger())
        {
            ai.EngageDogfight();
            return true;
        }
        ai.PerformEvasiveManeuver();
        return false;
    }
}
