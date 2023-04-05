using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoutingOrder : Order
{
    //this order simply increases unit's movement by 1.

    public override int order_movement(int baseMovement)
    {
        return baseMovement + 1;
    }


}
