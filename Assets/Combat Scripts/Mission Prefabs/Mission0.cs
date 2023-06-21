using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mission0 : Mission
{
    //The file for mission0.
    //tile legend:
    // 0: curtain floor
    // 1: curtain walls (impassable, blocks attacks)
    // 2: gambling machine (impassable)
    // 2: bar counter (impassable)
    // 3: outside doors, (reinforcement tile)
    // 4: doors to storage room, (reinforcement tile)
    

    //enemy legend:
    // 0: scarabit base
    // 1: scarabit responder
    // 2: scarabit swarmer

    //mission win/loss conditions
    public override bool is_mission_won(Unit[] pl, List<Unit> el, Tile[,] grid, int roundNumber, List<Tile> baseList)
    {
        //win if all enemies are defeated.
        //the roundNumber requirement here means the mission can't end while enemy reinforcements haven't come yet.
        if (roundNumber > 5 && el.Count == 0) 
        {
            return true;
        }
        return false;
    }
    public override bool is_mission_lost(Unit[] pl, List<Unit> el, Tile[,] grid, int roundNumber, List<Tile> baseList)
    {
        //defeated if all player units are defeated.
        for (int i = 0; i < pl.Length; i++)
        {
            if (pl[i] != null)
            {
                return false;
            }
        }
        return true;
    }
    public override string print_mission_briefing()
    {
        string ret = "Line 1\nLine 2\n\nBonuses\n-Allies alive\n-Bases owned\n-Low turn count";
        return ret;
    }

    //map setup
    public override Tile[,] get_layout()
    {
        //returns an array representing the map
        //row, depth into that row
        Tile[,] layout = new Tile[11, 9] {
            { null, missionTiles[1], missionTiles[1], missionTiles[1], missionTiles[1], missionTiles[1], missionTiles[1], missionTiles[1], null },
            { missionTiles[1], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[1] },
            { missionTiles[1], missionTiles[0], missionTiles[2], missionTiles[0], missionTiles[2], missionTiles[0], missionTiles[2], missionTiles[0], missionTiles[1] },
            { missionTiles[1], missionTiles[0], missionTiles[2], missionTiles[0], missionTiles[2], missionTiles[0], missionTiles[2], missionTiles[0], missionTiles[1] },
            { missionTiles[1], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[1] },
            { missionTiles[1], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[1] },
            { missionTiles[0], missionTiles[0], missionTiles[2], missionTiles[0], missionTiles[2], missionTiles[0], missionTiles[2], missionTiles[0], missionTiles[1] },
            { missionTiles[0], missionTiles[0], missionTiles[2], missionTiles[0], missionTiles[2], missionTiles[0], missionTiles[2], missionTiles[0], missionTiles[1] },
            { missionTiles[1], missionTiles[2], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[1] },
            { missionTiles[1], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[1] },
            { null, missionTiles[1], missionTiles[1], missionTiles[1], missionTiles[0], missionTiles[0], missionTiles[1], missionTiles[1], null }
            
        };
        //the mission layout will maintain this orientation in game.
        return layout;
    }
    public override int get_layout_x_dim() { return 11; } //first index of layout array, number of rows
    public override int get_layout_y_dim() { return 9; } //second index of layout array, number of columns

    //unit setup and reinforcements
    public override (int, int, int)[] get_deployment_spots()
    {
        //returns an array of unit IDs and coords representing unit starting spots.
        //unit id, row, depth into that row (if no unit with the id in reserve party, then we fail silently. all good)
        (int, int, int)[] dep_array = {
            (0, 5, 5),
            (1, 4, 4)
        };
        return dep_array;
    }
    public override (Enemy, int, int, int, int)[] get_enemy_spots()
    {
        //returns an array of units and information needed to spawn them in.
        //unit, row, depth into that row, act delay, times to level up
        (Enemy, int, int, int, int)[] dep_array = {
           
            //first room group
            (defEnemies[0], 1, 1, 0, 0), //scarabit base
            (defEnemies[0], 1, 3, 0, 0), //scarabit base
            (defEnemies[0], 2, 7, 0, 0), //scarabit base
            (defEnemies[0], 4, 1, 0, 0), //scarabit base
            (defEnemies[0], 9, 2, 0, 0), //scarabit responder
            (defEnemies[0], 8, 5, 0, 0) //scarabit responder
        };

        return dep_array;
    }
    public override (Enemy, int, int, int, int)[] get_enemy_reinforcements(int roundNumber)
    {
        //unit, row, depth into that row, act delay, times to level up
        switch (roundNumber)
        {
            case 2:
                (Enemy, int, int, int, int)[] dep_array3 = {
                    (defEnemies[2], 10, 4, 0, 0),
                    (defEnemies[2], 10, 5, 0, 0)
                };
                return dep_array3;
            case 5:
                (Enemy, int, int, int, int)[] dep_array5 = {
                    (defEnemies[1], 6, 0, 0, 0),
                    (defEnemies[1], 7, 0, 0, 0),
                    (defEnemies[2], 0, 2, 0, 0),
                    (defEnemies[2], 0, 4, 0, 0),
                    (defEnemies[2], 0, 6, 0, 0),
                    (defEnemies[2], 0, 3, 0, 0),
                    (defEnemies[2], 0, 5, 0, 0)
                };
                return dep_array5;
            default:
                break;
        }
        
        return null;
    }
    public override (int, int, int)[] get_player_reinforcements(int roundNumber)
    {
        //unit id, row, depth into that row
        switch (roundNumber)
        {
            case 3:
                (int, int, int)[] dep_array3 = {
                    (2, 10, 5), // yve
                    (3, 10, 4) // nai
                };
                return dep_array3;
            default:
                break;
        }
        
        return null;
    }

}