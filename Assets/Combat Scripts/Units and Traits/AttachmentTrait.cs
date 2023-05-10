using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AttachmentTrait : Trait
{
    //this is a passive that provides some kind of bonus if a certain unit is within x tile.
    //here, mc takes 0.75x damage if within 3 tiles of friday. (id 1)

    [SerializeField] private string unitNameStr; 

    public override int modify_dmg_received(int dmg, Unit self, Unit enemy, Unit[] self_allies)
    {
        foreach(Unit u in self_allies)
        {
            if (u != null && u.get_unitName() == unitNameStr)
            {
                //if we're in 2 tiles, then effect!
                if ( Math.Abs(self.x - u.x) + Math.Abs(self.y - u.y) <= 3)
                    return (int)(dmg * 0.75f);
            }
                
        }
        return dmg;

    }

}

