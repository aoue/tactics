using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BattleBrain
{
    //calculator for combat.

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
        double coverMod;
        if (playerAttacking)
        {
            coverMod = order.order_coverMod_offense(occupied_tile.get_cover());

            if (t.get_usesPhysDefense()) def = u2.get_physd();
            else def = u2.get_magd();
        }
        else
        {
            if (order != null)
            {
                if (t.get_usesPhysDefense()) def = order.order_physd(u2.get_physd());
                else def = order.order_magd(u2.get_magd());

                coverMod = order.order_coverMod_defense(occupied_tile.get_cover());
            }
            else
            {
                if (t.get_usesPhysDefense()) def = u2.get_physd();
                else def = u2.get_magd();

                coverMod = occupied_tile.get_cover();
            }
        }

        //damage formula: dmg = roll + user's atk - target's def
        int dmg_range_roll = t.get_dmg_range()[UnityEngine.Random.Range(0, t.get_dmg_range().Length)];
        int dmg = dmg_range_roll + atk - def;

        if (order != null) dmg = order.order_damage(dmg);

        //once calc is done
        //(also, yes, order of traits here will definitely matter, because of order of operations.)
        // -run modify_dmg_dealt() for each of u1's traits
        for (int i = 0; i < u1.get_traitList().Length; i++)
        {
            if (u1.get_traitList()[i] != null)
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

        return Math.Max(1, dmg);
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


        //damage formula: dmg = roll + user's atk - target's def
        int dmg_range_roll = t.get_dmg_range()[UnityEngine.Random.Range(0, t.get_dmg_range().Length)];
        int heal = dmg_range_roll + atk;

        if (order != null) heal = order.order_heal(heal);

        //once calc is done
        //(also, yes, order of traits here will definitely matter, because of order of operations.)
        // -run modify_heal_dealt() for each of u1's traits
        for (int i = 0; i < u1.get_traitList().Length; i++)
        {
            if (u1.get_traitList()[i] != null)
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

        return Math.Max(1, heal);
    }


}
