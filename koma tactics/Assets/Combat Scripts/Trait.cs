﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AoEType { SINGLE, ALL_BETWEEN, ALL, ADJACENT_FOUR }
public enum TargetingType { LINE, SQUARE, RADIUS, SELF }
public enum UnitType { NOTHING, CREATURE }
public class Trait : MonoBehaviour
{
    //passive and abilities.
    //used in combat scene.

    //A trait can not be both Passive and Active.
    //if ISPASSIVE is true, the unit will call the virtual functions, but will refuse to target
    //if ISPASSIVE is false, the unit will not call the virtual functions, but will target.
    //NO DOUBLE DIPPING. (seriously, it's not even possible.)
    //(this is also good design; seperating choices makes the decisions more meaningful.)

    //AoE Type legend:
    // -single: only the square clicked
    // -all between: for line only; all tiles between origin and dest tile.
    // -all: every single tile that is within range.

    //Targeting Type legend:
    // -line: valids the (4, 8, 12, etc...) tiles on the same axes as the player, in *range* tiles in all 4 directions.
    // -square: valids the 8 tiles forming a solid circle around the player.
    // -self: only self is a valid tiles
    // -radius: valids all tiles in *range* tiles of the origin.

    //Also, traits add to the unit's type.
    // -these can modify damage dealt. E.g. anti-air weapons do bonus against flying type units.
    //types legend:
    // see enum.

    //passive traits can:
    // -change movement reduction
    // -change dmg amount dealt
    // -change dmg amount received
    // -change heal amount received
    //active traits:
    // -can be used as an attack for targeting, dmg, etc.

    [SerializeField] private AudioClip traitSound; //the sound that plays when the trait is used. Or null for nothing.
    [SerializeField] private string traitName; //the name of the trait. Unique.
    [SerializeField] private string descr; //a 1-line (at the most) descr of the trait.
    [SerializeField] private UnitType unitType; //adds this unit type to the player unit.
    [SerializeField] private bool isPassive; //if true, the trait cannot be used as a move to target enemies. If false, it can.

    //active stuff
    [SerializeField] private TargetingType targeting; //determines how tiles are validated for targeting.
    [SerializeField] private AoEType aoe; //determines how many tiles are actually hit by an attack.
    [SerializeField] private bool ignores_blocking_terrain; //if true, then the move is not affected by tiles that block attacks
    [SerializeField] private int minimum_range; //used for validating tiles when attacking. Can only hit a tile where manhattan distance is >= x tiles away/
    [SerializeField] private int affinity; //used when attacking.
    [SerializeField] private int range; //determines how far the attack can reach
    [SerializeField] private int power; //determines, along with the attacker's and defender's stats, the damage dealt.
    [SerializeField] private float min_dmg_range; //floor of random dmg distribution range.
    [SerializeField] private float max_dmg_range; //ceiling of random dmg distribution range.
    [SerializeField] private float brkMult; //determines the amount of brk dmg dealt.
    [SerializeField] private int pwCost; //the power cost to use the move.
    [SerializeField] private bool usesPhysAttack; //on true, use attacker's phys attack for dmg calc. On false, use attacker's mag attack.
    [SerializeField] private bool usesPhysDefense; //on true, use target's phys def for dmg calc. On false, use target's mag def.
    [SerializeField] private bool isHeal;
    [SerializeField] private bool usesTrigger; //if true, then can only be used once per deployment.

    //state
    //set to true in the relevant function, if applicable. Set to false in unit.start_of_mission()
    //used to make sure one-time traits are only triggered once.
    protected bool triggered = false; 

    //for abilities, but on their use.
    //that is, only the active trait's on_attack() will be called when the unit attacks.
    public virtual void on_attack(Unit self, int totalDmg)
    {
        //the totalDmg is all the damage the unit dealt, in case it hit multiple targets.
    }
    public virtual void on_kill(Unit self, int totalDmg)
    {
        //the totalDmg is all the damage the unit dealt, in case it hit multiple targets.
    }

    //for strictly passive traits 
    public virtual void on_own_death(Unit self)
    {
        //examples:
        // -one time thing; return unit to x% health; triggered = true;
        // -every time thing; x% chance to survive at 1 hp

        
    }
    public virtual int modify_dmg_dealt(int dmg, Unit self, Unit enemy, Unit[] self_allies)
    {
        //will probably need the grid and playerList.

        //if allies are close/far
        //if the enemy is of a certain unitType
        //if the enemy is of a certain affinity
        //etc

        return dmg;
    }
    public virtual int modify_dmg_received(int dmg, Unit self, Unit enemy, Unit[] self_allies)
    {
        //will probably need the grid too.

        return dmg;
    }
    public virtual int modify_heal_dealt(int heal, Unit self, Unit enemy)
    {
        //will probably need the grid too.

        return heal;
    }
    public virtual int modify_heal_received(int heal, Unit self, Unit enemy)
    {
        //will probably need the grid too.

        return heal;
    }
    public virtual int modify_movementCost(Tile t)
    {
        //e.g. check tile type.
        //if it's a certain type, then do whatever.

        //e.g. flight makes all costs = 1
        //amphibious makes water tiles cost 1, whereas normally they cost -1 (impassable.)

        return t.get_movementCost();
    }

    
    public string get_traitDescr()
    {
        //generate the string based on move information.
        //then add the descr on to the end.
        //You have 3 lines. (1 of which is descr)

        string buildStr = "";
        //relevant information:
        // -what unit type it adds (if any). (can only add 1.)
        // -passive / ability
        // -range
        // -power + isHeal
        // -pwCost
        // -physA/magA vs. physD/magD

        if (isPassive)
        {
            buildStr += "Passive";

            if (usesTrigger)
            {
                
                if (triggered)
                {
                    buildStr += " (already used)";
                }
                else
                {
                    buildStr += " (not yet used)";
                }
            }

        }
        else
        {
            switch (affinity)
            {
                case 0:
                    buildStr += "Light ";
                    break;
                case 1:
                    buildStr += "Medium ";
                    break;
                case 2:
                    buildStr += "Heavy ";
                    break;
                default:
                    break;
            }
            buildStr += "Attack — ";

            if (isHeal) buildStr += " Support";
            else buildStr += " Offense";

            if (usesPhysAttack) buildStr += "| Machine Atk vs. ";           
            else buildStr += "| EAC Atk vs. ";
            
            if (usesPhysDefense) buildStr += " Machine Def";
            else buildStr += " EAC Def";

            buildStr += "\n" + power + " Force | " + pwCost + " cost.";
        }

        //each has its own sep text and image
        // -targetingType (image)
        // -AoEType (image)

        return buildStr + "\n<i>" + descr + "</i>";
    }

    public AudioClip get_traitSound() { return traitSound; }
    public string get_traitName() { return traitName; }
    public bool get_isPassive() { return isPassive; }
    public TargetingType get_targetingType() { return targeting; }
    public AoEType get_AoEType() { return aoe; }
    public int get_min_range() { return minimum_range; }
    public int get_aff() { return affinity; }
    public int get_range() { return range; }
    public int get_minimum_range() { return minimum_range; }
    public int get_power() { return power; }
    public float get_min_dmg_range() { return min_dmg_range; }
    public float get_max_dmg_range() { return max_dmg_range; }
    public float get_brkMult() { return brkMult; }
    public int get_pwCost() { return pwCost; }
    public bool get_usesPhysAttack() { return usesPhysAttack; }
    public bool get_usesPhysDefense() { return usesPhysDefense; }
    public bool get_isHeal() { return isHeal; }
    public UnitType get_unitType() { return unitType; }
    public void reset_triggered() { triggered = false; }
    public bool get_ignores_blocking_terrain() { return ignores_blocking_terrain; }

}
