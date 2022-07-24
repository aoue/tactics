using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum AI { FOOLISH, REGULAR, ELITE };
public class Enemy : Unit
{
    //derived from unit.
    //has AI.

    
    [SerializeField] private int pri_base;
    [SerializeField] private int pri_range; //(range is -pri_range, pri_range)
    [SerializeField] private int pri_panic;
    [SerializeField] private float panic_threshold; //float representing a fraction of health.
    
    //priority process:
    // -add base
    // -add random value from pri range
    // -if hp < panic threshold, add panic pri
    // -relevance score: add points based on unit's distance to where the last active player unit ended its turn.
    public override int calculate_priority()
    {
        int panicAdd = 0;
        if ((float)get_hp() / (float)get_hpMax() < panic_threshold) panicAdd = pri_panic;

        return pri_base + UnityEngine.Random.Range(-pri_range, pri_range) + panicAdd;
    }


    //3 different AIs: (each unit has its own.)
    // -foolish: advance with all units; do not compare attack scores, just attack closest.
    // -regular: advance rationally; do not compare attack scores, just attack closest.
    // -elite: advance rationally; compare attacks and pick best.
    [SerializeField] private AI unitAI;
    [SerializeField] private bool keepDistance; //false if the unit charges straight in. True if the unit keeps distance.
    [SerializeField] private bool caresAboutZoC; //true if the unit cares about ZoC score. False if doesn't care.
    [SerializeField] private bool lessMoving; //true if the unit prefers to move less. False if doesn't care.
    [SerializeField] private bool caresAboutBases; //true if the unit thinks it's important to capture a base. False if doesn't care.

    //movement + target selection
    //(necessarily done together)
    public override int score_move(int closestPlayerTile, Tile dest, int tilesAddedToZoC, Tile[,] myGrid, HashSet<Tile> visited)
    {
        bestTargetList = new List<Unit>();
        bestTraitIndex = -2;

        //score a grid destination.
        //factors:
        int score = 0;

        // -distance to closest player (more points if foolish) (remember: a lower value means you are closer.)
        //if we want to keep distance, then the higher closestPlayerTile is the better.
        //if we do not want to keep distance, then the lower closestPlayerTile is the better.
        if (keepDistance)
        {
            score += closestPlayerTile;
        }
        else
        {
            if (unitAI == AI.FOOLISH) score -= (2 * closestPlayerTile);
            else score -= closestPlayerTile;
        }

        // -non-controlled tiles this move adds ZoC control to (less points if foolish)
        if (caresAboutZoC)
        {
            if (unitAI == AI.FOOLISH) score += tilesAddedToZoC;
            else score += (2 * tilesAddedToZoC);
        }

        // -moving less is good.
        //calculate the manhattan distance between this tile and your starting position:
        if (lessMoving)
        {
            score -= Math.Abs(x - dest.x) + Math.Abs(y - dest.y);
        }

        // -capturing a base is good
        if (caresAboutBases)
        {
            if (dest is BaseTile)
            {
                //capturing bases is pretty important.
                score += 10;
            }
        }

        // finally, score all possible (traits)attacks the enemy can make too and add that to the sum.
        int runningMax = -1;
        BattleBrain brain = new BattleBrain();
        for (int i = 0; i < get_traitList().Length; i++)
        {
            if (get_traitList()[i] != null && !get_traitList()[i].get_isPassive() )
            {
                List<Tile> targetList = generate_targetList(get_traitList()[i], myGrid,  dest.x, dest.y, visited);

                int atkScore = score_attack(get_traitList()[i], targetList, brain);

                if (runningMax == -1)
                {
                    runningMax = atkScore;
                    bestTraitIndex = i;
                    bestTargetList = tileList_to_unitList(targetList);
                }
                else if (runningMax == score)
                {
                    if (UnityEngine.Random.Range(0, 2) == 0)
                    {
                        bestTraitIndex = i;
                        bestTargetList = tileList_to_unitList(targetList);
                    }
                }
                else if (runningMax < score)
                {
                    runningMax = atkScore;
                    bestTraitIndex = i;
                    bestTargetList = tileList_to_unitList(targetList);
                }
            }
        }

        //of course, there's the option of not attacking. 
        if (runningMax < 0)
        {
            bestTraitIndex = -1;
            bestTargetList = null;
        }


        //return score (but have saved the bestTraitIndex and bestTargetList)
        return score + (100*runningMax);
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
        switch (unitAI)
        {
            case AI.FOOLISH:

                foreach (Tile targetTile in targetList)
                {
                    if (targetTile.occupied())
                    {
                        if (targetTile.get_heldUnit().get_isAlly()) score += 1;
                        else score -= 1;
                    }
                }

                break;
            case AI.REGULAR:

                foreach (Tile targetTile in targetList)
                {
                    if (targetTile.occupied())
                    {
                        if (targetTile.get_heldUnit().get_isAlly()) score += 1;
                        else score -= 1;
                    }
                }

                break;
            case AI.ELITE:
                
                foreach (Tile targetTile in targetList)
                {
                    //score the attack based on projected damage.
                    if (targetTile.occupied())
                    {
                        int projected_dmg = brain.calc_damage(this, targetTile.get_heldUnit(), t, targetTile);
                        if (targetTile.get_heldUnit().get_isAlly())
                        {
                            score += projected_dmg;
                        }
                        else
                        {
                            score -= projected_dmg;
                        }
                    }
                }
                break;
        }

        return score;
    }
    List<Tile> generate_targetList(Trait t, Tile[,] myGrid, int x_click, int y_click, HashSet<Tile> visited)
    {
        //helper function for traits and generating targetlists. For scoring attacks, etc.

        List<Tile> targetList = new List<Tile>();

        switch (t.get_AoEType())
        {
            case AoEType.SINGLE:
                targetList.Add(myGrid[x_click, y_click]);
                break;
            case AoEType.ALL_BETWEEN:
                //first, we need to determine if the line is vertical or horizontal.
                if (y_click == y)
                {
                    //then the line is horizontal.
                    //next, the line could be to the right or to the left.
                    if (x_click > x)
                    {
                        for (int i = x + 1; i < x_click + 1; i++)
                        {
                            targetList.Add(myGrid[i, y]);
                        }
                    }
                    else
                    {
                        for (int i = x_click; i < x; i++)
                        {
                            targetList.Add(myGrid[i, y]);
                        }
                    }
                }
                else
                {
                    //otherwise, the line must be vertical.
                    //next, the line could be to the top or to the bottom.
                    //to the top.
                    if (y_click > y)
                    {
                        for (int j = y + 1; j < y_click + 1; j++)
                        {
                            targetList.Add(myGrid[x, j]);
                        }
                    }
                    else
                    {
                        for (int j = y_click; j < y; j++)
                        {
                            targetList.Add(myGrid[x, j]);
                        }
                    }
                }
                break;
            case AoEType.ALL:
                foreach (Tile t2 in visited)
                {
                    targetList.Add(myGrid[t2.x, t2.y]);
                }
                break;
        }

        return targetList;
    }
    List<Unit> tileList_to_unitList(List<Tile> tileList)
    {
        //remove all tiles with null unit.
        List<Unit> targetList = new List<Unit>();

        foreach (Tile t in tileList)
        {
            if (t.occupied())
            {
                targetList.Add(t.get_heldUnit());
            }
        }
        return targetList;
    }

    private List<Unit> bestTargetList;
    private int bestTraitIndex; 

    public override List<Unit> get_bestTargetList() { return bestTargetList; }
    public override int get_bestTraitIndex() { return bestTraitIndex; }

}
