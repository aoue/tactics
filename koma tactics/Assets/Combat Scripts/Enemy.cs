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

    //saved between moves (attacking stuff). Each triple is associated with a dest tile in cGrid's select_enemy_action()
    //Reset when a unit starts selection each time it's activated.
    private List<(int, List<Tile>, Tile)> moveInformationList;
    /*
    private int bestTraitIndex;
    private Tile bestAttackOrigin;
    private List<Tile> bestTileList;
    */

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

        int panicAdd = 0;
        if ((float)get_hp() / (float)get_hpMax() < panic_threshold) panicAdd = pri_panic;

        int relevanceAdd = 0;
        if (relevantTile != null)
        {
            relevanceAdd = 10 / (Math.Abs(x - relevantTile.x) + Math.Abs(y - relevantTile.y));
        }

        return pri_base + UnityEngine.Random.Range(-pri_range, pri_range) + panicAdd + relevanceAdd;
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
    [SerializeField] private bool caresAboutCover; //true if the unit thinks it's important to take cover. Adds 10* base's cover rating.
    [SerializeField] private bool caresAboutKills; //true if the unit value hitting player units with low hp percentage. (or, for elite, will kill.)

    //more target scoring:
    // +if target is low/high brk
    // +target has low/high defensive stat for this trait

    //movement + target selection
    //(necessarily done together)
    public override int score_move(int closestPlayerTile, Tile dest, int tilesAddedToZoC, Tile[,] myGrid, HashSet<Tile> visited)
    {
        //score a grid destination.
        //factors:
        int score = 0;

        // -non-controlled tiles this move adds ZoC control to (less points if foolish)
        if (caresAboutZoC)
        {
            if (unitAI == AI.FOOLISH) score += tilesAddedToZoC;
            else score += (2 * tilesAddedToZoC);
        }

        //if the unit doesn't care about keeping distance from the player,
        //i.e., the unit wants to charge straight in
        score -= closestPlayerTile;

        if (lessMoving)
        {
            score -= Math.Abs(x - dest.x) + Math.Abs(y - dest.y);
        }

        // -capturing a base is good
        if (caresAboutBases && dest is BaseTile)
        {
            score += 1;           
        }

        if (caresAboutCover)
        {
            score += (int)(dest.get_cover()*10);
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
                HashSet<Tile> origins = get_all_possible_attack_origins(get_traitList()[i], myGrid, dest);

                //for each tile that the trait could possible target; score the attack.
                foreach (Tile potential_origin in origins)
                {
                    //generate all the units that it would hit.
                    List<Tile> targetList = generate_targetList(get_traitList()[i], myGrid, potential_origin.x, potential_origin.y, visited);
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

        //Debug.Log("I'm thinking of dest " + dest.x + ", " + dest.y + " | and here's what i think of attacking: " + runningMax);

        //if any attack was found
        if ( runningMax > 0 )
        {
            //randomly pick one of the runningMaxList, and add to move information list.
            (int, List<Tile>, Tile) ans =  runningMaxList[UnityEngine.Random.Range(0, runningMaxList.Count)];
            moveInformationList.Add(ans);

            //(keep distance, but you still want to be able to attack)
            // -distance to closest player (more points if foolish) (remember: a lower value means you are closer.)
            //if we want to keep distance, then the higher/farther closestPlayerTile is the better.
            //if we do not want to keep distance, then the lower/closer closestPlayerTile is the better.
            if (keepDistance)
            {
                score += (2 * closestPlayerTile);
            }
        }
        else
        {
            //we have to add dummy to move information list, regardless, though, so the indexing can work.
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
        switch (unitAI)
        {
            case AI.FOOLISH:

                foreach (Tile targetTile in targetList)
                {
                    if (targetTile.occupied())
                    {
                        if (targetTile.get_heldUnit().get_isAlly())
                        {
                            score += 100;
                            if (caresAboutKills)
                            {
                                score = (int)(score * (2f - targetTile.get_heldUnit().get_hpPercentage()));
                            }
                        }
                        else score -= 50;
                    }
                }

                break;
            case AI.REGULAR:

                foreach (Tile targetTile in targetList)
                {
                    if (targetTile.occupied())
                    {
                        if (targetTile.get_heldUnit().get_isAlly())
                        {
                            score += 100;
                            if (caresAboutKills)
                            {
                                score = (int)(score * (2f - targetTile.get_heldUnit().get_hpPercentage()));
                            }
                        }
                        else score -= 100;
                    }
                }

                break;
            case AI.ELITE:
                
                foreach (Tile targetTile in targetList)
                {
                    //score the attack based on projected damage.
                    if (targetTile.occupied())
                    {
                        int projected_dmg = brain.calc_damage(this, targetTile.get_heldUnit(), t, targetTile, false, null, null);

                        if (projected_dmg >= targetTile.get_heldUnit().get_hp())
                        {
                            projected_dmg = (int)(projected_dmg * 1.5f);
                        }

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

    //Helper functions
    HashSet<Tile> get_all_possible_attack_origins(Trait t, Tile[,] myGrid, Tile dest)
    {
        //called when the game enters select target mode.
        //highlights the tiles that are possible targets based on the ability.
        //t, in conjunction with active player's coords, has all the information needed.
        //here, we imagine that the unit is standing on dest tile, too

        int map_x_border = myGrid.GetLength(0);
        int map_y_border = myGrid.GetLength(1);
        //Debug.Log("map x border = " + map_x_border);
        //Debug.Log("map y border = " + map_y_border);

        //for a text description of the targeting types, see the legend in Trait.cs
        HashSet<Tile> origins = new HashSet<Tile>();
        switch (t.get_targetingType())
        {
            case TargetingType.LINE:
                //above and below
                for (int i = dest.x - t.get_range(); i < dest.x + t.get_range() + 1; i++)
                {
                    if (within_border(i, dest.y, map_x_border, map_y_border) && i != dest.x)
                    {
                        if (Math.Abs(dest.x - i) >= t.get_min_range())
                        {
                            origins.Add(myGrid[i, dest.y]);
                        }
                        if (myGrid[i, dest.y].get_blocksAttacks())
                        {
                            break;
                        }                       
                    }
                }
                //left and right
                for (int j = dest.y - t.get_range(); j < dest.y + t.get_range() + 1; j++)
                {
                    if (within_border(dest.x, j, map_x_border, map_y_border) && j != dest.y)
                    {
                        if (Math.Abs(dest.y - j) >= t.get_min_range())
                        {
                            origins.Add(myGrid[dest.x, j]);
                        }
                        if (myGrid[dest.x, j].get_blocksAttacks())
                        {
                            break;
                        }
                    }
                }
                break;
            case TargetingType.SQUARE:
                for (int i = dest.x - 1; i < dest.x + 1 + 1; i++)
                {
                    for (int j = dest.y - 1; j < dest.y + 1 + 1; j++)
                    {
                        //if the tile is on the grid, and is not the unit's tile.
                        if (within_border(i, j, map_x_border, map_y_border) && !(i == dest.x && j == dest.y) && Math.Abs(dest.x - i) + Math.Abs(dest.y - j) >= t.get_min_range())
                        {
                            origins.Add(myGrid[i, j]);
                        }
                    }
                }
                break;
            case TargetingType.RADIUS:
                //borrow dfs code.
                atk_dfs(dest, origins, myGrid[dest.x, dest.y], t.get_range(), t.get_min_range(), myGrid, map_x_border, map_y_border);
                origins.Remove(myGrid[dest.x, dest.y]);
                break;
            case TargetingType.SELF:
                origins.Add(myGrid[dest.x, dest.y]);
                break;
        }
        return origins;
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
    void atk_dfs(Tile dest, HashSet<Tile> v, Tile start, int rangeLeft, int min_range, Tile[,] myGrid, int map_x_border, int map_y_border)
    {
        //here, dest is the tile the unit is imagining it is on. We have to use dest.x,y instead of real x,y
        if (Math.Abs(dest.x - start.x) + Math.Abs(dest.y - start.y) >= min_range)
        {
            v.Add(start);
        }

        if (start.get_blocksAttacks())
        {
            return;
        }

        List<Tile> adjacentTiles = new List<Tile>();
        //add tiles based on coordinates, as long as they are not out of bounds.
        if (within_border(start.x + 1, start.y, map_x_border, map_y_border)) adjacentTiles.Add(myGrid[start.x + 1, start.y]);
        if (within_border(start.x - 1, start.y, map_x_border, map_y_border)) adjacentTiles.Add(myGrid[start.x - 1, start.y]);
        if (within_border(start.x, start.y + 1, map_x_border, map_y_border)) adjacentTiles.Add(myGrid[start.x, start.y + 1]);
        if (within_border(start.x, start.y - 1, map_x_border, map_y_border)) adjacentTiles.Add(myGrid[start.x, start.y - 1]);

        foreach (Tile next in adjacentTiles)
        {
            if (rangeLeft > 0)
            {
                atk_dfs(dest, v, next, rangeLeft - 1, min_range, myGrid, map_x_border, map_y_border);
            }
        }
    }
    bool within_border(int local_x, int local_y, int map_x_border, int map_y_border)
    {
        if (local_x < map_x_border && local_x >= 0 && local_y < map_y_border && local_y >= 0) return true;
        return false;
    }

    public override (int, List<Tile>, Tile) get_action_information(int actionIndex)
    {
        return moveInformationList[actionIndex];
    }
    
}
