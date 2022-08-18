using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerConservationOrder : Order
{
    //+1 PW at the end of each unit's turn.
    public override int order_power_cost(int cost)
    {
        return cost - 2;
        //return cost - 1;
    }

}
