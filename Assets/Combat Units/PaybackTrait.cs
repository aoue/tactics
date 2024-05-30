using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaybackTrait : Trait
{

    public override int[] modify_rolls(int[] rolls, Unit self)
    {
        //expand dmg range to +1 dmg for each 25% of self hp lost.
        double hpLeft = (double)self.get_hp() / (double)self.get_hpMax();

        List<int> newRange = new List<int>();

        if (hpLeft <= 0.25) newRange.Add(20);
        else if (hpLeft <= 0.50) newRange.Add(15);
        else if (hpLeft <= 0.75) newRange.Add(10);
        else newRange.Add(5);

        return newRange.ToArray();
        
        // return rolls;
    }


}
