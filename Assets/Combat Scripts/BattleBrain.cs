using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BattleBrain
{
    //calculator for combat.

    //(light, medium, heavy)
    //the attacker's affinity vs. the defender's affinity
    //medium to 1x to light.
    //heavy does 0.5x to light
    float[,] affinityMultArray = new float[3, 3]
    {
        {1.0f, 1.0f, 1.0f },
        {1.0f, 1.0f, 1.0f },
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

        double def;
        double coverMod;
        if (playerAttacking)
        {
            coverMod = order.order_coverMod_offense(1f - occupied_tile.get_cover());

            if (t.get_usesPhysDefense()) def = u2.get_physd();
            else def = u2.get_magd();
        }
        else
        {
            if (order != null)
            {
                if (t.get_usesPhysDefense()) def = order.order_physd(u2.get_physd());
                else def = order.order_magd(u2.get_magd());

                coverMod = order.order_coverMod_defense(1f - occupied_tile.get_cover());
            }
            else
            {
                if (t.get_usesPhysDefense()) def = u2.get_physd();
                else def = u2.get_magd();

                coverMod = 1f - occupied_tile.get_cover();
            }
        }

        //integer damage formula:
        //Debug.Log("performing attack calc. u1 aff =" + u1.get_aff() + " | u2 aff =" + u2.get_aff() + "| aff mult = " + affinityMultArray[u1.get_aff(), u2.get_aff()]);
        //int dmg = Mathf.Max(1, (int)((u1.get_level() + atk + t.get_power() - def) * coverMod * affinityMultArray[u1.get_aff(), u2.get_aff()] * UnityEngine.Random.Range(t.get_min_dmg_range(), t.get_max_dmg_range())));

        //multiplicative damage formula:
        //int dmg = Mathf.Max(1, (int)((2 * atk * (t.get_power() + (u1.get_level() * 2)) / (1.5f * def)) * coverMod * UnityEngine.Random.Range(t.get_min_dmg_range(), t.get_max_dmg_range()) * affinityMultArray[u1.get_aff(), u2.get_aff()]));

        //integer + defense mult damage formula:
        int dmg = Mathf.Max(1, (int)((u1.get_level() + atk) * def * coverMod * affinityMultArray[u1.get_aff(), u2.get_aff()] * UnityEngine.Random.Range(t.get_min_dmg_range(), t.get_max_dmg_range())));

        if (order != null) dmg = order.order_damage(dmg);

        //once calc is done
        //(also, yes, order of traits here will definitely matter, because of order of operations.)
        // -run modify_dmg_dealt() for each of u1's traits
        for (int i = 0; i < u1.get_traitList().Length; i++)
        {
            if (u1.get_traitList()[i] != null /*&& u1.get_traitList()[i].get_isPassive()*/)
            {
                dmg = u1.get_traitList()[i].modify_dmg_dealt(dmg, u1, u2, u1_allies);
            }
        }

        // -run modify_dmg_taken() for each of u2's traits
        if (order != null)
        {
            for (int i = 0; i < u2.get_traitList().Length; i++)
            {
                if (u2.get_traitList()[i] != null && u2.get_traitList()[i].get_isPassive())
                {
                    dmg = u2.get_traitList()[i].modify_dmg_received(dmg, u1, u2, u1_allies);
                }
            }
        }
        

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


        //integer based formula
        int heal = Mathf.Max(1, (int)((u1.get_level() + atk) * UnityEngine.Random.Range(t.get_min_dmg_range(), t.get_max_dmg_range())));

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
