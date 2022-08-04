using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BattleBrain
{
    //calculator for combat.

    //light, medium, heavy
    //the trait's affinity vs. the defender's affinity
    //light does 2x to heavy, and takes 0.5x
    //medium does 1.5x to light, and takes 0.75x
    //heavy does 1x to medium, and takes 1x

    //light good against heavy in both off and def
    //medium good against light in both off and def, but not crazily
    //heavy good against nothing; relies on pure stats.
    float[,] affinityMultArray = new float[3, 3]
    {
        {1.0f, 0.75f, 2.0f },
        {1.25f, 1.0f, 1.0f },
        {0.5f, 1.0f, 1.0f }
    };

    public int calc_damage(Unit u1, Unit u2, Trait t, Tile occupied_tile, bool playerAttacking, Order order, Unit[] u1_allies)
    {
        //use attacker's phys a or mag a?
        int atk;
        if (playerAttacking)
        {
            if (order != null)
            {
                if (t.get_usesPhysAttack()) atk = order.order_physa(u1.get_physa());
                else atk = order.order_maga(u1.get_maga());
            }
            else
            {
                if (t.get_usesPhysAttack()) atk = u1.get_physa();
                else atk = u1.get_maga();
            }            
        }
        else
        {
            if (t.get_usesPhysAttack()) atk = u1.get_physa();
            else atk = u1.get_maga();
        }

        int def;
        float coverMod;
        if (playerAttacking)
        {
            coverMod = order.order_coverMod_offense(1f - occupied_tile.get_cover());

            if (t.get_usesPhysDefense()) def = u2.get_physd();
            else def = u2.get_magd();
        }
        else
        {
            coverMod = order.order_coverMod_defense(1f - occupied_tile.get_cover());

            if (order != null)
            {
                if (t.get_usesPhysDefense()) def = order.order_physd(u2.get_physd());
                else def = order.order_magd(u2.get_magd());
            }
            else
            {
                if (t.get_usesPhysDefense()) def = u2.get_physd();
                else def = u2.get_magd();
            }
        }
        
        //integer damage formula:
        //int dmg = Mathf.Max(1, (int)(((atk + t.get_power()) - def) * coverMod));

        //multiplicative damage formula:
        int dmg = Mathf.Max(1, (int)((t.get_power() / 2 + (atk * (t.get_power() + (u1.get_level() * 2)) / def) * coverMod * UnityEngine.Random.Range(0.85f, 1f))));
        dmg = order.order_damage(dmg);

        //once calc is done
        //(also, yes, order of traits here will definitely matter, because of order of operations.)
        // -run modify_dmg_dealt() for each of u1's traits
        for (int i = 0; i < u1.get_traitList().Length; i++)
        {
            if (u1.get_traitList()[i] != null && u1.get_traitList()[i].get_isPassive())
            {
                dmg = u1.get_traitList()[i].modify_dmg_dealt(dmg, u1, u2, u1_allies);
            }
        }

        // -run modify_dmg_taken() for each of u2's traits
        for (int i = 0; i < u2.get_traitList().Length; i++)
        {
            if (u2.get_traitList()[i] != null && u2.get_traitList()[i].get_isPassive())
            {
                dmg = u2.get_traitList()[i].modify_dmg_received(dmg, u1, u2, u1_allies);
            }
        }

        //finally, apply affinity triangle multiplier
        dmg = Math.Max(1, (int)(affinityMultArray[t.get_aff(), u2.get_aff()] * dmg));

        return dmg;
    }
    public int calc_heal(Unit u1, Unit u2, Trait t, bool playerAttacking, Order order)
    {
        int atk;

        if (playerAttacking)
        {
            if (order != null)
            {
                if (t.get_usesPhysAttack()) atk = order.order_physa(u1.get_physa());
                else atk = order.order_maga(u1.get_maga());
            }
            else
            {
                if (t.get_usesPhysAttack()) atk = u1.get_physa();
                else atk = u1.get_maga();
            }
        }
        else
        {
            if (t.get_usesPhysAttack()) atk = u1.get_physa();
            else atk = u1.get_maga();
        }

        int heal = Mathf.Max(1, (int)((atk + t.get_power() + (u1.get_level() * 2)) / 2 * UnityEngine.Random.Range(0.85f, 1f)));
        if (order != null) heal = order.order_heal(heal);

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
