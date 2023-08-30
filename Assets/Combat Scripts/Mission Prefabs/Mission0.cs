using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mission0 : Mission
{
    //enemy legend:
    // 0: scarabit base
    // 1: scarabit responder
    // 2: scarabit swarmer

    //mission win/loss conditions
    public override bool is_mission_won(Unit[] pl, List<Unit> el, Tile[,] grid, int roundNumber, List<Tile> baseList)
    {
        //win if all enemies are defeated.
        //the roundNumber requirement here means the mission can't end while enemy reinforcements haven't come yet.
        if (el.Count == 0) 
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
        //tile legend: (temp)
        // 0: curtain floor
        // 1: curtain walls (impassable, blocks attacks)
        // 2: gambling machine (impassable)
        // 3: bar counter (impassable)
        // 4: doors, (reinforcement tile)

        //returns an array representing the map
        //row, depth into that row
        Tile[,] layout = new Tile[6, 6] {
            { m[0], m[0], m[0], m[0], m[0], m[0] },
            { m[1], m[1], m[1], m[1], m[0], m[0] },
            { m[0], m[0], m[0], m[1], m[0], m[0] },
            { m[0], m[0], m[0], m[1], m[0], m[0] },
            { m[0], m[0], m[1], m[0], m[0], m[0] },
            { m[0], m[0], m[0], m[0], m[0], m[0] },
            
        };
        //the mission layout will maintain this orientation in game.
        return layout;
    }
    public override int get_layout_x_dim() { return 6; } //first index of layout array, number of rows
    public override int get_layout_y_dim() { return 6; } //second index of layout array, number of columns

    //unit setup and reinforcements
    public override (int, int, int)[] get_deployment_spots()
    {
        //returns an array of unit IDs and coords representing unit starting spots.
        //unit id, row, depth into that row (if no unit with the id in reserve party, then we fail silently. all good)
        (int, int, int)[] dep_array = {
            (1, 4, 4),
            (0, 5, 3)
        };
        return dep_array;
    }
    public override (Enemy, int, int, int, int)[] get_enemy_spots()
    {
        //returns an array of units and information needed to spawn them in.
        //unit, row, depth into that row, act delay, times to level up
        (Enemy, int, int, int, int)[] dep_array = {
           
            //first room group
            (defEnemies[0], 1, 4, 0, 0), //scarabit base
            (defEnemies[0], 2, 4, 0, 0), //scarabit base
        };

        return dep_array;
    }
    public override (Enemy, int, int, int, int)[] get_enemy_reinforcements(int roundNumber)
    {
        //unit, row, depth into that row, act delay, times to level up
        return null;
        switch (roundNumber)
        {
            case 2:
                (Enemy, int, int, int, int)[] dep_array3 = {
                    (defEnemies[1], 5, 0, 1, 0),
                    (defEnemies[1], 6, 0, 1, 0)
                };
                return dep_array3;
            case 5:
                break;
                (Enemy, int, int, int, int)[] dep_array5 = {
                    (defEnemies[2], 2, 2, 1, 0),
                    (defEnemies[2], 3, 2, 0, 0),

                    (defEnemies[2], 2, 4, 1, 0),
                    (defEnemies[2], 3, 4, 0, 0),

                    (defEnemies[2], 2, 6, 1, 0),
                    (defEnemies[2], 3, 6, 0, 0)
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
        return null;
        switch (roundNumber)
        {
            case 3:
                (int, int, int)[] dep_array3 = {
                    (2, 10, 4), // yve
                    (3, 10, 3) // nai
                };
                return dep_array3;
            default:
                break;
        }
        
        return null;
    }

}