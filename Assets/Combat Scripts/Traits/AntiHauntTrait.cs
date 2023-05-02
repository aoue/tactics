using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiHauntTrait : Trait
{
    //Deals bonus damage against units bearing the [haunt] class
    public virtual int modify_dmg_dealt(int dmg, Unit self, Unit enemy, Unit[] self_allies)
    {
        if ( enemy.get_unitTypes().Contains(UnitType.HAUNT)) return (int)((double)dmg * 1.3);
        return dmg;
    }

}
