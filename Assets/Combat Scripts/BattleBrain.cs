using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class BattleBrain
{
    //calculator for combat.

    
    public int calc_damage(Unit u1, Unit u2, Trait t, Tile occupied_tile, bool playerAttacking, Order order, Unit[] u1_allies)
    {
        //use attacker's phys a or mag a?
        int atk;
        if (playerAttacking && order != null)
        {
            if (t.get_usesPhysAttack()) atk = order.order_physa(u1.get_physa());
            else atk = order.order_maga(u1.get_maga());
        }
        else
        {
            if (t.get_usesPhysAttack()) atk = u1.get_physa();
            else atk = u1.get_maga();
        }

        int def;
        if (!playerAttacking && order != null)
        {
            // so, player unit is on defense
            // order defenses            
            if (t.get_usesPhysDefense()) def = order.order_physd(u2.get_physd());
            else def = order.order_magd(u2.get_magd());
        }
        else
        {
            if (t.get_usesPhysDefense()) def = u2.get_physd();
            else def = u2.get_magd();
        }
        if (u2.get_isBroken()) def = 0;

        int coverMod;
        if (playerAttacking) coverMod = order.order_coverMod_offense(occupied_tile.get_cover());
        else coverMod = order.order_coverMod_defense(occupied_tile.get_cover());

        //create damage range
        int[] dmg_range = t.get_dmg_range();
        dmg_range = t.modify_dmg_range(dmg_range, u1, u2);

        //damage formula: dmg = roll + user's atk - target's def
        //if the target is broken, then set their defense to 0
        int dmg_range_roll = dmg_range[UnityEngine.Random.Range(0, dmg_range.Length)];
        int dmg = dmg_range_roll + atk - def - coverMod;
        dmg = t.modify_dmg_dealt(dmg, u1, u2, u1_allies);
        
        //each of user's passives interacts with dmg dealt
        for (int i = 0; i < u1.get_traitList().Length; i++)
        {
            if (u1.get_traitList()[i] != null && u1.get_traitList()[i].get_isPassive())
            {
                dmg = u1.get_traitList()[i].modify_dmg_dealt(dmg, u1, u2, u1_allies);
            }
        }

        //each of target's passives interacts with dmg taken
        for (int i = 0; i < u2.get_traitList().Length; i++)
        {
            if (u2.get_traitList()[i] != null && u2.get_traitList()[i].get_isPassive())
            {
                dmg = u2.get_traitList()[i].modify_dmg_received(dmg, u1, u2, u1_allies);
            }
        }

        // finally, if the player is attacking, then apply dmg order.
        if (order != null) 
        {
            if (playerAttacking) dmg = order.order_damage_dealt(dmg);
            else dmg = order.order_damage_received(dmg);
        }
        
        return Math.Max(1, dmg);
    }
    public int calc_heal(Unit u1, Unit u2, Trait t, bool playerAttacking, Order order)
    {
        int atk;
        if (playerAttacking && order != null)
        {
            if (t.get_usesPhysAttack()) atk = order.order_physa(u1.get_physa());
            else atk = order.order_maga(u1.get_maga());
        }
        else
        {
            if (t.get_usesPhysAttack()) atk = u1.get_physa();
            else atk = u1.get_maga();
        }

        int[] dmg_range = t.get_dmg_range();
        dmg_range = t.modify_dmg_range(dmg_range, u1, u2);

        //damage formula: dmg = roll + user's atk - target's def
        int dmg_range_roll = dmg_range[UnityEngine.Random.Range(0, dmg_range.Length)];
        int heal = dmg_range_roll + atk;
        heal = t.modify_heal_dealt(heal, u1, u2);

        // run user's passives
        for (int i = 0; i < u1.get_traitList().Length; i++)
        {
            if (u1.get_traitList()[i] != null && u1.get_traitList()[i].get_isPassive())
            {
                heal = u1.get_traitList()[i].modify_heal_dealt(heal, u1, u2);
            }
        }

        // run target's passives
        for (int i = 0; i < u2.get_traitList().Length; i++)
        {
            if (u2.get_traitList()[i] != null && u2.get_traitList()[i].get_isPassive())
            {
                heal = u2.get_traitList()[i].modify_heal_received(heal, u1, u2);
            }
        }

        if (order != null) 
        {
            if (playerAttacking) heal = order.order_heal_dealt(heal);
            else heal = order.order_heal_received(heal);
        }
        
        return Math.Max(1, heal);
    }

    public string calc_damage_range_str(Unit u1, Unit u2, Trait t, Tile occupied_tile, bool playerAttacking, Order order, Unit[] u1_allies)
    {
        //use attacker's phys a or mag a?
        int atk;
        if (playerAttacking && order != null)
        {
            if (t.get_usesPhysAttack()) atk = order.order_physa(u1.get_physa());
            else atk = order.order_maga(u1.get_maga());
        }
        else
        {
            if (t.get_usesPhysAttack()) atk = u1.get_physa();
            else atk = u1.get_maga();
        }

        int def;
        if (!playerAttacking && order != null)
        {
            // so, player unit is on defense
            // order defenses            
            if (t.get_usesPhysDefense()) def = order.order_physd(u2.get_physd());
            else def = order.order_magd(u2.get_magd());
        }
        else
        {
            if (t.get_usesPhysDefense()) def = u2.get_physd();
            else def = u2.get_magd();
        }
        if (u2.get_isBroken()) def = 0;

        int coverMod;
        if (playerAttacking) coverMod = order.order_coverMod_offense(occupied_tile.get_cover());
        else coverMod = order.order_coverMod_defense(occupied_tile.get_cover());

        
        int[] dmg_range = t.get_dmg_range();
        dmg_range = t.modify_dmg_range(dmg_range, u1, u2);

        //damage formula: dmg = roll + user's atk - target's def
        //if the target is broken, then set their defense to 0
        int dmg_high = dmg_range.Max() + atk - def - coverMod;
        int dmg_low = dmg_range.Min() + atk - def - coverMod;
        dmg_high = t.modify_dmg_dealt(dmg_high, u1, u2, u1_allies);
        dmg_low = t.modify_dmg_dealt(dmg_low, u1, u2, u1_allies);
        
        //each of user's passives interacts with dmg dealt
        for (int i = 0; i < u1.get_traitList().Length; i++)
        {
            if (u1.get_traitList()[i] != null && u1.get_traitList()[i].get_isPassive())
            {
                dmg_high = u1.get_traitList()[i].modify_dmg_dealt(dmg_high, u1, u2, u1_allies);
                dmg_low = u1.get_traitList()[i].modify_dmg_dealt(dmg_low, u1, u2, u1_allies);
            }
        }

        //each of target's passives interacts with dmg taken
        for (int i = 0; i < u2.get_traitList().Length; i++)
        {
            if (u2.get_traitList()[i] != null && u2.get_traitList()[i].get_isPassive())
            {
                dmg_high = u2.get_traitList()[i].modify_dmg_received(dmg_high, u1, u2, u1_allies);
                dmg_low = u2.get_traitList()[i].modify_dmg_received(dmg_low, u1, u2, u1_allies);
            }
        }

        // finally, if the player is attacking, then apply dmg order.
        if (order != null) 
        {
            if (playerAttacking)
            {
                dmg_high = order.order_damage_dealt(dmg_high);
                dmg_low = order.order_damage_dealt(dmg_low);
            }
            else 
            {
                dmg_high = order.order_damage_received(dmg_high);
                dmg_low = order.order_damage_received(dmg_low);
            }
        }
        
        dmg_high = Math.Max(1, dmg_high);
        dmg_low = Math.Max(1, dmg_low);
        return dmg_low.ToString() + "-" + dmg_high.ToString();
    }

    public string calc_heal_range_str(Unit u1, Unit u2, Trait t, bool playerAttacking, Order order)
    {
        int atk;
        if (playerAttacking && order != null)
        {
            if (t.get_usesPhysAttack()) atk = order.order_physa(u1.get_physa());
            else atk = order.order_maga(u1.get_maga());
        }
        else
        {
            if (t.get_usesPhysAttack()) atk = u1.get_physa();
            else atk = u1.get_maga();
        }

        int[] dmg_range = t.get_dmg_range();
        dmg_range = t.modify_dmg_range(dmg_range, u1, u2);

        //damage formula: dmg = roll + user's atk - target's def
        int heal_high = dmg_range.Max() + atk;
        int heal_low = dmg_range.Min() + atk;
        heal_high = t.modify_heal_dealt(heal_high, u1, u2);
        heal_low = t.modify_heal_dealt(heal_low, u1, u2);

        // run user's passives
        for (int i = 0; i < u1.get_traitList().Length; i++)
        {
            if (u1.get_traitList()[i] != null && u1.get_traitList()[i].get_isPassive())
            {
                heal_high = u1.get_traitList()[i].modify_heal_dealt(heal_high, u1, u2);
                heal_low = u1.get_traitList()[i].modify_heal_dealt(heal_low, u1, u2);
            }
        }

        // run target's passives
        for (int i = 0; i < u2.get_traitList().Length; i++)
        {
            if (u2.get_traitList()[i] != null && u2.get_traitList()[i].get_isPassive())
            {
                heal_high = u2.get_traitList()[i].modify_heal_received(heal_high, u1, u2);
                heal_low = u2.get_traitList()[i].modify_heal_received(heal_low, u1, u2);
            }
        }

        if (order != null) 
        {
            if (playerAttacking) 
            {
                heal_high = order.order_heal_dealt(heal_high);
                heal_low = order.order_heal_dealt(heal_low);
            }
            else 
            {
                heal_high = order.order_heal_received(heal_high);
                heal_low = order.order_heal_received(heal_low);
            }
            
        }
        
        return heal_low.ToString() + "-" + heal_high.ToString();
    }

}
