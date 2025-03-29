public class BTStrafingRun : BTNode
{
    private AIAircraft aircraft;

    public BTStrafingRun(AIAircraft ai) { aircraft = ai; }

    public override bool Execute()
    {
        if (aircraft.HasGroundTarget())
        {
            aircraft.PerformStrafingRun();
            return true;
        }
        return false;
    }
}
