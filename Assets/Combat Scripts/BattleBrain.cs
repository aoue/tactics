using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class BattleBrain
{
    //calculator for combat.
    public int calc_damage(Unit u1, Unit u2, Trait t, double actual_roll, Tile occupied_tile, bool playerAttacking, Order order, Unit[] u1_allies)
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

        double coverMult;
        if (playerAttacking) coverMult = order.order_coverMult_offense(occupied_tile.get_coverMult());
        else coverMult = order.order_coverMult_defense(occupied_tile.get_coverMult());

        //damage formula: dmg = roll * (user's atk - target's def)
        //if the target is broken, then set their defense to 0
        int dmg = (int)((atk - def) * actual_roll * coverMult);
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
    public int calc_heal(Unit u1, Unit u2, Trait t, double actual_roll, bool playerAttacking, Order order)
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
        //heal formula: dmg = roll + user's atk
        int heal = (int)(actual_roll * atk);
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

        double coverMult;
        if (playerAttacking) coverMult = order.order_coverMult_offense(occupied_tile.get_coverMult());
        else coverMult = order.order_coverMult_defense(occupied_tile.get_coverMult());

        double[] rolls = t.get_rolls();
        rolls = t.modify_rolls(rolls, u1);

        //damage formula: dmg = roll + user's atk - target's def
        List<int> damage_results = new List<int>();
        foreach (double roll in rolls)
        {
            int damage_result = (int)((((roll * atk)) - def) * coverMult);
            damage_result = t.modify_dmg_dealt(damage_result, u1, u2, u1_allies);
            damage_results.Add(damage_result);
        }

        //each of user's passives interacts with dmg dealt
        for (int i = 0; i < u1.get_traitList().Length; i++)
        {
            if (u1.get_traitList()[i] != null && u1.get_traitList()[i].get_isPassive())
            {
                for(int j = 0; j < damage_results.Count; j++)
                {
                    damage_results[j] = u1.get_traitList()[i].modify_dmg_dealt(damage_results[j], u1, u2, u1_allies);
                }
            }
        }

        //each of target's passives interacts with dmg taken
        for (int i = 0; i < u2.get_traitList().Length; i++)
        {
            if (u2.get_traitList()[i] != null && u2.get_traitList()[i].get_isPassive())
            {
                for(int j = 0; j < damage_results.Count; j++)
                {
                    damage_results[j] = u2.get_traitList()[i].modify_dmg_received(damage_results[j], u1, u2, u1_allies);
                }
            }
        }

        // finally, if the player is attacking, then apply dmg order.
        if (order != null) 
        {
            if (playerAttacking)
            {
                for(int i = 0; i < damage_results.Count; i++)
                {
                    damage_results[i] = order.order_damage_dealt(damage_results[i]);
                }
            }
            else 
            {
                for(int i = 0; i < damage_results.Count; i++)
                {
                    damage_results[i] = order.order_damage_received(damage_results[i]);
                }
            }
        }
        
        string return_string = "";
        for(int i = 0; i < damage_results.Count; i++)
        {
            damage_results[i] = Math.Max(1, damage_results[i]);
            return_string += damage_results[i] + ",";
        }
        return return_string.Substring(0, return_string.Length - 1);
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

        double[] rolls = t.get_rolls();
        rolls = t.modify_rolls(rolls, u1);

        // int heal_high = (int)(rolls.Max() + atk);
        // int heal_low = (int)(rolls.Min() + atk);
        // heal_high = t.modify_heal_dealt(heal_high, u1, u2);
        // heal_low = t.modify_heal_dealt(heal_low, u1, u2);

        //damage formula: dmg = roll + user's atk - target's def
        List<int> heal_results = new List<int>();
        foreach (double roll in rolls)
        {
            int heal_result = (int)(roll * atk);
            heal_result = t.modify_heal_dealt(heal_result, u1, u2);
            heal_results.Add(heal_result);
        }

        // run user's passives
        for (int i = 0; i < u1.get_traitList().Length; i++)
        {
            if (u1.get_traitList()[i] != null && u1.get_traitList()[i].get_isPassive())
            {
                // heal_high = u1.get_traitList()[i].modify_heal_dealt(heal_high, u1, u2);
                // heal_low = u1.get_traitList()[i].modify_heal_dealt(heal_low, u1, u2);
                for(int j = 0; j < heal_results.Count; j++)
                {
                    heal_results[j] = u1.get_traitList()[i].modify_heal_dealt(heal_results[j], u1, u2);
                }
            }
        }

        // run target's passives
        for (int i = 0; i < u2.get_traitList().Length; i++)
        {
            if (u2.get_traitList()[i] != null && u2.get_traitList()[i].get_isPassive())
            {
                // heal_high = u2.get_traitList()[i].modify_heal_received(heal_high, u1, u2);
                // heal_low = u2.get_traitList()[i].modify_heal_received(heal_low, u1, u2);
                for(int j = 0; j < heal_results.Count; j++)
                {
                    heal_results[j] = u2.get_traitList()[i].modify_heal_received(heal_results[j], u1, u2);
                }
            }
        }

        if (order != null) 
        {
            if (playerAttacking) 
            {
                // heal_high = order.order_heal_dealt(heal_high);
                // heal_low = order.order_heal_dealt(heal_low);
                for(int j = 0; j < heal_results.Count; j++)
                {
                    heal_results[j] = order.order_heal_dealt(heal_results[j]);
                }
            }
            else 
            {
                // heal_high = order.order_heal_received(heal_high);
                // heal_low = order.order_heal_received(heal_low);
                for(int j = 0; j < heal_results.Count; j++)
                {
                    heal_results[j] = order.order_heal_received(heal_results[j]);
                }
            }
            
        }

        string return_string = "";
        for(int i = 0; i < heal_results.Count; i++)
        {
            heal_results[i] = Math.Max(1, heal_results[i]);
            return_string += heal_results[i] + ",";
        }
        return return_string.Substring(0, return_string.Length - 1);

        // return heal_low.ToString() + "-" + heal_high.ToString();
    }

}
