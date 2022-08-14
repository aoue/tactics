using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mission0 : Mission
{
    //The file for mission0.

    //mission win/loss conditions
    public override bool is_mission_won(Unit[] pl, List<Unit> el, Tile[,] grid)
    {
        //win if 3 or less enemies remain
        if (el.Count <= 3) 
        {
            return true;
        }
        return false;
    }
    public override bool is_mission_lost(Unit[] pl, List<Unit> el, Tile[,] grid)
    {
        //default is all player units dead
        for (int i = 0; i < pl.Length; i++)
        {
            if (pl[i] != null)
            {
                return false;
            }
        }
        return true;
    }
    public override bool[] side_objectives_states(Unit[] pl, List<Unit> el, int roundNumber)
    {
        //returns list of ints, where each int corresponds to the state of a side objective.
        //each element corresponds to a side objective result.
        // ObjectiveState.in_progress: objective not completed.
        // false: objective not completed
        // true: objective completed

        bool[] retList = new bool[] { false };

        //Side objective: no units are defeated
        int alivePlayerUnits = 0;
        foreach(Unit u in pl)
        {
            if (u != null) alivePlayerUnits++;
        }
        if (alivePlayerUnits >= 4) retList[0] = true;

        Debug.Log("side_objectives_states yo ho ho");

        return retList;
    }

    //map setup
    public override Tile[,] get_layout()
    {
        //returns an array representing the map
        //row, depth into that row
        Tile[,] layout0 = new Tile[12, 12] {
            { missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[1], missionTiles[0], missionTiles[1], missionTiles[0], missionTiles[0], missionTiles[0]},
            { missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[1], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0]},
            { missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[1], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[1], missionTiles[0], missionTiles[0]},
            { missionTiles[1], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[1], missionTiles[1], missionTiles[0]},
            { missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[1], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0]},
            { missionTiles[2], missionTiles[2], missionTiles[2], missionTiles[2], missionTiles[2], missionTiles[2], missionTiles[2], missionTiles[2], missionTiles[2], missionTiles[2], missionTiles[2], missionTiles[2]},
            { missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[1], missionTiles[1], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0]},
            { missionTiles[0], missionTiles[1], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[1], missionTiles[0], missionTiles[0]},
            { missionTiles[1], missionTiles[1], missionTiles[1], missionTiles[0], missionTiles[1], missionTiles[0], missionTiles[0], missionTiles[1], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0]},
            { missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0]},
            { missionTiles[0], missionTiles[0], missionTiles[1], missionTiles[1], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0]},
            { missionTiles[0], missionTiles[1], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[1], missionTiles[0], missionTiles[0], missionTiles[0]}
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
        (int, int, int)[] dep_array = {
            (0, 1, 1),
            (1, 2, 1),
            (2, 3, 2),
            (3, 3, 4)
        };
        return dep_array;
    }
    public override (Enemy, int, int)[] get_enemy_spots()
    {
        //returns an array of units and coords representing unit starting spots.
        //row, depth into that row
        (Enemy, int, int)[] dep_array = {
            //close right pair
            (defEnemies[0], 2, 7),
            (defEnemies[1], 3, 8),

            //far right guy
            (defEnemies[2], 2, 11),

            //far bottom group
            (defEnemies[0], 11, 1),
            (defEnemies[2], 10, 2),
            (defEnemies[0], 11, 3),

            //far bottom right pair
            (defEnemies[0], 10, 10),
            (defEnemies[1], 9, 9)

        };

        return dep_array;
    }

}
