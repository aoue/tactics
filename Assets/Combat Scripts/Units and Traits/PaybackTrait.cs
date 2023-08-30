using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaybackTrait : Trait
{

    public override double[] modify_rolls(double[] rolls, Unit self)
    {
        //expand dmg range to +1 dmg for each 25% of self hp lost.
        double hpLeft = (double)self.get_hp() / (double)self.get_hpMax();

        List<double> newRange = new List<double>();
        newRange.Add(1);

        if (hpLeft <= 0.75) newRange.Add(2);
        if (hpLeft <= 0.50) newRange.Add(3);
        if (hpLeft <= 0.25) newRange.Add(4);

        return newRange.ToArray();
    }


}
