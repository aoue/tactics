using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ALSLaserTrait : Trait
{
    //a trait whose special ability is a bonus versus heavy type targets.
    //deals 2x dmg to them.

    public override int modify_dmg_dealt(int dmg, Unit self, Unit enemy, Unit[] self_allies)
    {
        //Debug.Log("ALS-Anti-H: enemy aff = " + enemy.get_aff());

        if (enemy.get_aff() == 2) return (dmg * 4);

        return dmg;
    }

}
