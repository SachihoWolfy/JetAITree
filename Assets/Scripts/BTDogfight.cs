public class BTDogfight : BTNode
{
    private AIAircraft aircraft;

    public BTDogfight(AIAircraft ai) { aircraft = ai; }

    public override bool Execute()
    {
        if (aircraft.CanEngageEnemy())
        {
            aircraft.EngageDogfight();
            return true;
        }
        return false;
    }
}
