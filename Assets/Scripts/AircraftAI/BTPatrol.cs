public class BTPatrol : BTNode
{
    private AIAircraft aircraft;

    public BTPatrol(AIAircraft ai) { aircraft = ai; }

    public override bool Execute()
    {
        aircraft.Patrol();
        return true;
    }
}
