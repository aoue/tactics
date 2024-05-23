using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiAPTrait : Trait
{
    public override int modify_dmg_dealt(int dmg, Unit self, Unit enemy, Unit[] self_allies)
    {
        //bonus damage against units with ap remaining.
        if (enemy.get_ap() > 0) return dmg + 1;
        return dmg;
    }
}
