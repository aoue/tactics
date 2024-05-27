using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AutoLeech : Trait
{
    //this is a passive that provides some kind of bonus if any allied unit is within 2 tiles.

    public override int modify_dmg_dealt(int dmg, Unit self, Unit enemy, Unit[] self_allies)
    {
        foreach(Unit u in self_allies)
        {
            if (u != null && u != self)
            {
                //if we're in 2 tiles, then effect!
                double d = Math.Abs(self.x - u.x) + Math.Abs(self.y - u.y);
                if (d <= 2){
                    //Debug.Log("auto leech returning dmg + 1");
                    return dmg + 1;
                }
                    
            }
                
        }
        return dmg;
    }



}
