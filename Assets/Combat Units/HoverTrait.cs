using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverTrait : Trait
{
    
    //passive; lets the user ignore many types of terrain hazards.
    public override int modify_movementCost(Tile t)
    {
        //e.g. check tile type.
        //if it's a certain type, then do whatever.

        if (t.get_movementCost() > 0) return 1;
        return t.get_movementCost();
    }
}
