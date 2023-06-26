using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaybackTrait : Trait
{
    public override int modify_dmg_dealt(int dmg, Unit self, Unit enemy, Unit[] self_allies)
    {
        //+1 dmg for each 25% of self hp lost.
        double hpLeft = (double)self.get_hp() / (double)self.get_hpMax();

        if (hpLeft <= 0.25) return dmg + 3;
        if (hpLeft <= 0.50) return dmg + 2;
        if (hpLeft <= 0.75) return dmg + 1;
        return dmg;
    }


}
