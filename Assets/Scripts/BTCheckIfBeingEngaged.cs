public class BTCheckIfBeingEngaged : BTNode
{
    private AIAircraft ai;

    public BTCheckIfBeingEngaged(AIAircraft ai)
    {
        this.ai = ai;
    }

    public override bool Execute()
    {
        if (ai.IsInDanger())
        {
            ai.target = ai.FindBestTarget(); // Find the best target for evasion or counterattack
            return true;
        }
        return false;
    }
}
