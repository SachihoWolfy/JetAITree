public class BTCheckTeammateInDanger : BTNode
{
    private AIAircraft ai;

    public BTCheckTeammateInDanger(AIAircraft ai)
    {
        this.ai = ai;
    }

    public override bool Execute()
    {
        foreach (var teammate in ai.teammates)
        {
            if (teammate.IsInDanger())
            {
                ai.target = teammate.target; // Target the enemy that's threatening the teammate
                return true;
            }
        }
        return false;
    }
}
