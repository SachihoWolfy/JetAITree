using UnityEngine;

public class BTEvasiveManeuver : BTNode
{
    private AIAircraft aircraft;

    public BTEvasiveManeuver(AIAircraft ai) { aircraft = ai; }

    public override bool Execute()
    {
        if (aircraft.IsInDanger()) // AI determines if it's at risk
        {
            aircraft.currentState = "Evasive Maneuver";
            aircraft.PerformEvasiveManeuver();
            //aircraft.FindBestTarget();
            return true;
        }
        return false;
    }
}
