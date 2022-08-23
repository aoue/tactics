using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlightTrait : Trait
{
    public override int modify_movementCost(Tile t)
    {
        //can cross all byt impassable.
        if (t.get_movementCost() == -1)
        {
            return -1;
        }
        else
        {
            return 1;
        }
    }

}
