using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlightTrait : Trait
{
    public override int modify_movementCost(Tile t)
    {
        //can cross some types of impassable terrain.
        //including:
        // -2: water

        if (t.get_movementCost() == -2 || t.get_movementCost() > 0)
        {
            //we can cross it with movement cost 1
            return 1;
        }
        else
        {
            //we cannot cross it.
            return -1;
        }
    }

}
