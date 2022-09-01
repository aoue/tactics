using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mission1 : Mission
{
    //The file for mission1.
    //tile legend:
    // 0: empty/snow
    // 1: light woods
    // 2: top-down tracks 
    // 3: heavy woods
    // 4: water
    // 5: ice
    // 6: neutral base
    // 7: train (impassable to all)
    // 8: reinforce
    // 9: defend

    //enemy legend:
    // 0: light
    // 1: medium
    // 2: heavy
    // 3: beeliner(?) maybe

    //mission win/loss conditions
    public override bool is_mission_won(Unit[] pl, List<Unit> el, Tile[,] grid, int roundNumber, List<Tile> baseList)
    {
        //win if round 15
        if (roundNumber >= 15)
        {
            return true;
        }
        return false;
    }
    public override bool is_mission_lost(Unit[] pl, List<Unit> el, Tile[,] grid, int roundNumber, List<Tile> baseList)
    {
        //defeated if all player units are defeated.
        // or if defend tile is lost, but that's handled out of here.
        for (int i = 0; i < pl.Length; i++)
        {
            if (pl[i] != null)
            {
                return false;
            }
        }
        return true;
    }
    public override bool[] side_objectives_states(Unit[] pl, List<Unit> el, int roundNumber, bool anyPlayerCasualties, List<Tile> baseList)
    {
        //returns list of bools, where each bool corresponds to the state of a side objective.
        //each element corresponds to a side objective result.
        // false: objective not completed
        // true: objective completed

        bool[] retList = new bool[] { true, true };

        //Side objective: 
        // -no units are defeated
        if (anyPlayerCasualties)
        {
            retList[0] = false;
        }

        // -all bases belong to player
        for (int i = 0; i < baseList.Count; i++)
        {
            if (baseList[i].get_ownership() != BaseOwnership.PLAYER)
            {
                retList[1] = false;
                break;
            }
        }
        return retList;
    }

    //map setup
    public override Tile[,] get_layout()
    {
        //returns an array representing the map
        //row, depth into that row
        Tile[,] layout0 = new Tile[12, 12] {
            { missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[4], missionTiles[4], missionTiles[1], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[7], missionTiles[0], missionTiles[0]},
            { missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[4], missionTiles[4], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[9], missionTiles[7], missionTiles[0], missionTiles[0]},
            { missionTiles[1], missionTiles[0], missionTiles[0], missionTiles[5], missionTiles[5], missionTiles[0], missionTiles[1], missionTiles[0], missionTiles[1], missionTiles[2], missionTiles[0], missionTiles[0]},
            { missionTiles[1], missionTiles[1], missionTiles[0], missionTiles[4], missionTiles[4], missionTiles[4], missionTiles[1], missionTiles[1], missionTiles[0], missionTiles[2], missionTiles[0], missionTiles[0]},
            { missionTiles[1], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[4], missionTiles[4], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[2], missionTiles[3], missionTiles[0]},
            { missionTiles[0], missionTiles[0], missionTiles[6], missionTiles[0], missionTiles[5], missionTiles[5], missionTiles[0], missionTiles[0], missionTiles[1], missionTiles[2], missionTiles[0], missionTiles[3]},
            { missionTiles[0], missionTiles[1], missionTiles[0], missionTiles[0], missionTiles[4], missionTiles[4], missionTiles[4], missionTiles[3], missionTiles[0], missionTiles[2], missionTiles[0], missionTiles[0]},
            { missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[3], missionTiles[4], missionTiles[4], missionTiles[4], missionTiles[1], missionTiles[0], missionTiles[2], missionTiles[0], missionTiles[0]},
            { missionTiles[1], missionTiles[0], missionTiles[1], missionTiles[4], missionTiles[4], missionTiles[4], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[2], missionTiles[0], missionTiles[3]},
            { missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[4], missionTiles[4], missionTiles[4], missionTiles[1], missionTiles[1], missionTiles[0], missionTiles[2], missionTiles[1], missionTiles[0]},
            { missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[4], missionTiles[4], missionTiles[4], missionTiles[1], missionTiles[0], missionTiles[0], missionTiles[2], missionTiles[0], missionTiles[0]},
            { missionTiles[8], missionTiles[8], missionTiles[4], missionTiles[4], missionTiles[4], missionTiles[4], missionTiles[8], missionTiles[8], missionTiles[8], missionTiles[2], missionTiles[8], missionTiles[8]}
        };
        //the mission layout will maintain this orientation in game.

        return layout0;
    }
    public override int get_layout_x_dim() { return 12; }
    public override int get_layout_y_dim() { return 12; }

    //unit setup and reinforcements
    public override (int, int, int)[] get_deployment_spots()
    {
        //returns an array of unit IDs and coords representing unit starting spots.
        //unit id, row, depth into that row (if no unit with the id in reserve party, then we fail silently. all good)
        (int, int, int)[] dep_array = {
            (0, 4, 8), //mc
            (1, 3, 9), //yve
            (2, 3, 10), //nai
            (4, 0, 1), //bergen
            (5, 0, 2)  //alta
        };
        return dep_array;
    }
    public override (Enemy, int, int, int, int)[] get_enemy_spots()
    {
        //returns an array of units and information needed to spawn them in.
        //unit, row, depth into that row, act delay, times to level up
        (Enemy, int, int, int, int)[] dep_array = {
           
            //left-forward heavy
            (defEnemies[2], 5, 1, 0, 0),

            //left bottom pair
            (defEnemies[0], 10, 1, 0, 0),
            (defEnemies[1], 10, 2, 0, 0),

            //right forward group
            (defEnemies[0], 8, 8, 0, 0),
            (defEnemies[1], 8, 9, 0, 0),
            (defEnemies[2], 8, 10, 0, 0),

            //right bottom group (waiting 2)
            (defEnemies[1], 11, 8, 2, 0),
            (defEnemies[1], 11, 10, 2, 0)
        };

        return dep_array;
    }
    public virtual (Enemy, int, int, int, int)[] get_enemy_reinforcements(int roundNumber)
    {
        //unit, row, depth into that row, act delay, times to level up
        switch (roundNumber)
        {
            case 0:
                break;
            case 1:
                //(Enemy, int, int)[] dep_array = {
                //    (defEnemies[0], 4, 0, 0),
                //};
                //return dep_array;
                break;
            case 2:
                break;
            case 3:
                break;
            case 4:
                break;
            case 5:
                break;
            case 6:
                break;
            case 7:
                break;
            case 8:
                break;
            case 9:
                break;
            case 10:
                break;
            default:
                break;
        }
        return null;
    }


}
