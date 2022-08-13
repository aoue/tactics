using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AttachmentTrait : Trait
{
    //this is a passive that provides some kind of bonus if a certain unit is within x tile.
    //here, mc takes 0.75x damage if within 3 tiles of friday. (id 1)

    public override int modify_dmg_received(int dmg, Unit self, Unit enemy, Unit[] self_allies)
    {
        foreach(Unit u in self_allies)
        {
            //if the unit is YVE
            if (u != null && u.get_uniqueUnitID() == 1)
            {
                //if we're in 3 tiles, then only take 0.75x damage! 
                if ( Math.Abs(self.x - u.x) + Math.Abs(self.y - u.y) <= 3)
                    return (int)(dmg * 0.75f);
            }
                
        }

        return dmg;
    }


}

