using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Enemy : Unit
{
    //derived from unit.
    //has AI.

    private int activation_delay; //the number of rounds this unit passes when activated. Also, when greater than 0, returns -100 for pri.
    
    [SerializeField] private SpriteRenderer sleepSpriteRenderer;

    [SerializeField] private int pri_base;
    [SerializeField] private int pri_range; //(range is -pri_range, pri_range)
    [SerializeField] private int pri_panic;
    [SerializeField] private float panic_threshold; //float representing a fraction of health.

    //saved between moves (attacking stuff). Each triple is associated with a dest tile in cGrid's select_enemy_action()
    //Reset when a unit starts selection each time it's activated.
    private List<(int, List<Tile>, Tile)> moveInformationList;

    public override void level_up(int toLevel)
    {
        //take an argument 'toLevel' and set the stats to preset values accordingly

    }  

    public override void clear_moveInformationList_except_last()
    {
        //removes all elements from the list except for the last one.
        if (moveInformationList == null || moveInformationList.Count < 2) return;
        else moveInformationList.RemoveRange(0, moveInformationList.Count - 1);       
    }
    public override void reset_selection_variables()
    {
        if (moveInformationList == null) moveInformationList = new List<(int, List<Tile>, Tile)>();
        else moveInformationList.Clear();
    }

    public override int calculate_priority(Tile relevantTile)
    {
        //priority process:
        // -add base
        // -add random value from pri range
        // -if hp < panic threshold, add panic pri
        // -relevance score: add points based on unit's distance to where the last active player unit ended its turn.

        if (activation_delay > 0)
        {
            return -100;
        }

        int panicAdd = 0;
        if ((float)get_hp() / (float)get_hpMax() < panic_threshold) panicAdd = pri_panic;

        int relevanceAdd = 0;
        if (relevantTile != null)
        {
            relevanceAdd = (pri_base*10) / (Math.Abs(x - relevantTile.x) + Math.Abs(y - relevantTile.y));
        }

        return pri_base + UnityEngine.Random.Range(-pri_range, pri_range) + panicAdd + relevanceAdd;
    }

    //ai parameters
    [SerializeField] private bool valuesRange; //false if the unit charges straight in. True if the unit keeps distance.
    [SerializeField] private bool valuesZOC; //true if the unit cares about ZoC score. False if doesn't care.
    [SerializeField] private bool valuesLethargy; //true if the unit prefers to move less. False if doesn't care.
    [SerializeField] private bool valuesBases; //true if the unit values capturing bases.
    [SerializeField] private bool valuesCover; //true if the unit values being on cover. Adds 10* base's cover rating.
    [SerializeField] private bool valuesKills; //true if the unit values hitting player units with low hp percentage.
    [SerializeField] private int hitAllyMod; //integer, subtracted from move score for every ally hit.

    //more target scoring:
    // +if target is low/high brk
    // +target has low/high defensive stat for this trait

    //movement + target selection
    //(necessarily done together)
    public override int score_move(int closestPlayerTile, Tile dest, int tilesAddedToZoC, Tile[,] myGrid, HashSet<Tile> visited, GridHelper gridHelper)
    {
        //score a grid destination.
        //factors:
        int score = 0;

        if (dest is DefendTile)
        {
            score += 1000;
        }

        // -non-controlled tiles this move adds ZoC control to
        if (valuesZOC)
        {
            score += tilesAddedToZoC;
        }

        //if the unit doesn't care about keeping distance from the player,
        //i.e., the unit wants to charge straight in
        /*
        if (unitAI == AI.FOOLISH)
        {
            //score -= (3 * closestPlayerTile);
            score -= closestPlayerTile;
        }*/
        score -= (closestPlayerTile);
        
        // -capturing a base is good
        if (valuesBases && dest is BaseTile)
        {
            score += 1;       
        }

        if (valuesCover)
        {
            score += (dest.get_cover());
        }

        // finally, score all possible (traits)attacks the enemy can make too and add that to the sum.
        //this loop serves to find the best trait-target tile, pair.
        int runningMax = -1;
        BattleBrain brain = new BattleBrain();

        //for each trait
        List<(int, List<Tile>, Tile)> runningMaxList = new List<(int, List<Tile>, Tile)>();
        for (int i = 0; i < get_traitList().Length; i++)
        {
            //if the trait exists and can be used to attack
            if ( get_traitList()[i] != null && !get_traitList()[i].get_isPassive() )
            {
                //calc all possible origins for this trait to pick
                HashSet<Tile> origins = gridHelper.get_all_possible_attack_origins(get_traitList()[i], myGrid, dest);

                //for each tile that the trait could possible target; score the attack.
                foreach (Tile potential_origin in origins)
                {
                    //generate all the units that it would hit.
                    List<Tile> targetList = gridHelper.generate_targetList(get_traitList()[i], myGrid, potential_origin.x, potential_origin.y, dest.x, dest.y, visited);
                    int atkScore = score_attack(get_traitList()[i], targetList, brain);
                    //Debug.Log("targetList length: " + targetList.Count + " | potential origin is " + potential_origin.x + ", " + potential_origin.y + "| score is " + atkScore);
                    
                    if (runningMax == -1)
                    {
                        runningMax = atkScore;                       
                        runningMaxList.Add((i, targetList, potential_origin));
                    }
                    else if (atkScore > runningMax)
                    {
                        runningMax = atkScore;
                        runningMaxList.Clear();
                        runningMaxList.Add((i, targetList, potential_origin));
                    }
                    else if (atkScore == runningMax)
                    {
                        runningMaxList.Add((i, targetList, potential_origin));
                    } 
                }
            }
        }

        //Debug.Log("I'm thinking of dest tile " + dest.x + ", " + dest.y + " | and here's what my score for attacking: " + runningMax);
        //if any attack was found
        if ( runningMax > 0 )
        {
            //randomly pick one of the runningMaxList, and add to move information list.
            (int, List<Tile>, Tile) ans = runningMaxList[UnityEngine.Random.Range(0, runningMaxList.Count)];
            moveInformationList.Add(ans);

            //(if true, that means the unit prefers being further away from the target.)
            //(check is in here, otherwise they would never approach the target)
            if (valuesRange)
            {
                score += (2 * closestPlayerTile);
            }

            //(if true, the unit prefers moving less)
            //(check is in here, otherwise they would never approach the target)
            if (valuesLethargy)
            {
                score -= Math.Abs(x - dest.x) + Math.Abs(y - dest.y);
            }
        }
        else
        {
            moveInformationList.Add( (-2, null, null) );
        }

        //return score (and have saved the bestTraitIndex and bestTargetList)
        //Debug.Log("running max for dest below = " + runningMax);
        return score + (runningMax);
    }   
    public override int score_attack(Trait t, List<Tile> targetList, BattleBrain brain)
    {
        //score a possible attack.
       
        //depends a lot on ai type.
        // -foolish and regular scores all attacks as 1 (player hit), or -1 (enemy hit)
        // -elite and scores all attacks based on projected damage. Also, will never attack an ally.

        //for each target
        //  calculate damage score (positive if isPlayer, negative if not)
        int score = 0;
        foreach (Tile targetTile in targetList)
        {
            if (targetTile.occupied())
            {
                if (targetTile.get_heldUnit().get_isAlly())
                {
                    score += 100;
                    if (valuesKills)
                    {
                        score = (int)(score * (4f - targetTile.get_heldUnit().get_hpPercentage()));
                    }
                }
                else 
                {
                    score += hitAllyMod;
                }
            }
        }
        return score;
    }

    //Getters
    public override (int, List<Tile>, Tile) get_action_information(int actionIndex)
    {
        return moveInformationList[actionIndex];
    }
    public override void cancel_act_delay()
    { 
        activation_delay = 0;
        sleepSpriteRenderer.enabled = false;
    }
    public void set_activation_delay(int x)
    { 
        activation_delay = x;
        if (x > 0) 
        {
            sleepSpriteRenderer.enabled = true;
            //sleepSpriteRenderer.color = new Color(0f, 0f, 0f);
        }
        else sleepSpriteRenderer.enabled = false;
    }
    public override void dec_act_delay() 
    { 
        activation_delay--; 
        if (activation_delay == 0) sleepSpriteRenderer.enabled = false;
    }  
    public override int get_act_delay() { return activation_delay; }
    public override bool get_isAlly() { return false; }
}
