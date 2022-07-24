using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BattleBrain
{
    //calculator for combat.

    //light, medium, heavy
    float[,] affinityMultArray = new float[3, 3]
    {
        {1.0f, 0.75f, 1.5f },
        {1.5f, 1.0f, 0.75f },
        {0.75f, 1.5f, 1.0f }
    };

    public int calc_damage(Unit u1, Unit u2, Trait t, Tile occupied_tile)
    {
        //use attacker's phys a or mag a?
        int atk;
        if (t.get_usesPhysAttack()) atk = u1.get_physa();
        else atk = u1.get_maga();

        int def;
        if (t.get_usesPhysDefense()) def = u2.get_physd();
        else def = u2.get_magd();

        float coverMod = 1f - occupied_tile.get_cover();

        int dmg = Mathf.Max(1, (int)(((atk + t.get_power()) - def) * coverMod));

        //once calc is done
        //(also, yes, order of traits here will definitely matter, because of order of operations.)
        // -run modify_dmg_dealt() for each of u1's traits
        for (int i = 0; i < u1.get_traitList().Length; i++)
        {
            if (u1.get_traitList()[i] != null && u1.get_traitList()[i].get_isPassive())
            {
                dmg = u1.get_traitList()[i].modify_dmg_dealt(dmg, u1, u2);
            }
        }

        // -run modify_dmg_taken() for each of u2's traits
        for (int i = 0; i < u2.get_traitList().Length; i++)
        {
            if (u2.get_traitList()[i] != null && u2.get_traitList()[i].get_isPassive())
            {
                dmg = u2.get_traitList()[i].modify_dmg_received(dmg, u1, u2);
            }
        }

        //finally, apply affinity triangle multiplier
        dmg = (int)(affinityMultArray[u1.get_aff(), u2.get_aff()] * dmg);

        return dmg;
    }
    public int calc_heal(Unit u1, Unit u2, Trait t)
    {
        int atk;
        if (t.get_usesPhysAttack()) atk = u1.get_physa();
        else atk = u1.get_physa();

        int heal = Mathf.Max(1, (atk + t.get_power()) / 2);

        //once calc is done
        //(also, yes, order of traits here will definitely matter, because of order of operations.)
        // -run modify_dmg_dealt() for each of u1's traits
        for (int i = 0; i < u1.get_traitList().Length; i++)
        {
            if (u1.get_traitList()[i] != null && u1.get_traitList()[i].get_isPassive())
            {
                heal = u1.get_traitList()[i].modify_heal_dealt(heal, u1, u2);
            }
        }

        // -run modify_dmg_taken() for each of u2's traits
        for (int i = 0; i < u2.get_traitList().Length; i++)
        {
            if (u2.get_traitList()[i] != null && u2.get_traitList()[i].get_isPassive())
            {
                heal = u2.get_traitList()[i].modify_heal_received(heal, u1, u2);
            }
        }

        return heal;
    }


}
