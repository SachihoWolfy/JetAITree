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
            return true;
        }
        return false;
    }
}
