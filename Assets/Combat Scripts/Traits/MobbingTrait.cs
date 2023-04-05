using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MobbingTrait : Trait
{
    //provides attack bonus if within 2 range of allies

    [SerializeField] private string unitNameStr; 

    public override int modify_dmg_received(int dmg, Unit self, Unit enemy, Unit[] self_allies)
    {
        foreach(Unit u in self_allies)
        {
            if (u != null && u.get_unitName() == unitNameStr)
            {
                //if we're in 2 tiles, then effect!
                if ( Math.Abs(self.x - u.x) + Math.Abs(self.y - u.y) <= 3)
                    return (int)(dmg * 1.25f);
            }
                
        }
        return dmg;

    }
    
}
