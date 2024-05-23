using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnstoppableTrait : Trait
{
    public override void on_mission_start(Unit self)
    {
        triggered = false;
    }
    public override void on_own_death(Unit self)
    {
        //once per mission ignore a fatal attack.
        //return to half health and full break.


        if (triggered) return;

        triggered = true;
        self.set_hp(self.get_hpMax() / 2);
        self.set_brk(self.get_brkMax());
        
    }
}
