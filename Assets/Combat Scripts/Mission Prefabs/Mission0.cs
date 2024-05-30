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
        // 0: snow floor
        // 1: building walls (impassable, blocks attacks)
        // 2: rocks (cover)
        // 3: arrive tile (objective)
        // 4: reinforcement tile (impassable; for spawns only)
        //      ^looks like a hole in the wall, where they crawl out from

        Tile[,] layout = new Tile[13, 12] {
            { m[0], m[0], m[1], m[0], m[0], m[0], m[0], m[0], m[0], m[0], m[1], m[1] },
            { m[0], m[0], m[0], m[0], m[0], m[0], m[0], m[0], m[0], m[0], m[0], m[1] },
            { m[0], m[0], m[0], m[1], m[0], m[0], m[0], m[0], m[1], m[0], m[0], m[1] },
            { m[0], m[0], m[0], m[1], m[1], m[0], m[0], m[0], m[1], m[0], m[0], m[0] },
            { m[0], m[0], m[0], m[0], m[1], m[0], m[0], m[0], m[1], m[0], m[0], m[0] },
            { m[0], m[0], m[0], m[0], m[0], m[0], m[0], m[0], m[0], m[1], m[0], m[0] },
            { m[0], m[0], m[0], m[0], m[0], m[0], m[0], m[0], m[0], m[1], m[0], m[0] },
            { m[1], m[0], m[0], m[1], m[0], m[0], m[0], m[0], m[0], m[1], m[0], m[0] },
            { m[1], m[1], m[1], m[0], m[1], m[1], m[0], m[0], m[0], m[0], m[0], m[0] },
            { m[1], m[0], m[0], m[0], m[0], m[0], m[0], m[0], m[0], m[0], m[1], m[0] },
            { m[0], m[0], m[0], m[0], m[0], m[0], m[0], m[0], m[0], m[0], m[0], m[1] },
            { m[1], m[0], m[0], m[0], m[0], m[0], m[0], m[0], m[0], m[1], m[1], m[1] },
            { m[1], m[1], m[1], m[1], m[0], m[0], m[0], m[1], m[1], m[1], m[1], m[1] }
        };
        //the mission layout will maintain this orientation in game.
        return layout;
    }
    public override int get_layout_x_dim() { return 13; } //first index of layout array, number of rows
    public override int get_layout_y_dim() { return 12; } //second index of layout array, number of columns

    //unit setup and reinforcements
    public override (int, int, int)[] get_deployment_spots()
    {
        //returns an array of unit IDs and coords representing unit starting spots.
        //unit id, row, depth into that row (if no unit with the id in reserve party, then we fail silently. all good)
        (int, int, int)[] dep_array = {
            (0, 10, 2),  // anse
            (1, 9, 2),  // friday
            (2, 11, 2),  // yve
            (3, 10, 1),  // nai
        };
        return dep_array;
    }
    public override (Enemy, int, int, int, int)[] get_enemy_spots()
    {
        //returns an array of units and information needed to spawn them in.

        //unit, row, depth into that row, act delay, times to level up
        (Enemy, int, int, int, int)[] dep_array = {
           
            //close-bottom group
            (defEnemies[0], 9, 6, 0, 0), //scarabit base
            (defEnemies[0], 10, 7, 0, 0), //scarabit base
            (defEnemies[0], 11, 8, 0, 0), //scarabit base

            //mid-center group (sleep 2)
            (defEnemies[0], 4, 7, 2, 0), //scarabit base
            (defEnemies[0], 3, 6, 2, 0), //scarabit base
            (defEnemies[0], 4, 5, 2, 0), //scarabit base

            //right-building group (sleep 4)
            (defEnemies[0], 4, 11, 4, 0), //scarabit base
            (defEnemies[0], 5, 10, 4, 0), //scarabit base

            //left-building group (sleep 6)
            (defEnemies[0], 1, 1, 6, 0), //scarabit base
            (defEnemies[0], 2, 1, 6, 0), //scarabit base
            (defEnemies[0], 1, 0, 6, 0), //scarabit base
            (defEnemies[0], 2, 2, 6, 0), //scarabit base
        };

        return dep_array;
    }
    public override (Enemy, int, int, int, int)[] get_enemy_reinforcements(int roundNumber)
    {
        //unit, row, depth into that row, act delay, times to level up
        
        // switch (roundNumber)
        // {
        //     case 2:
        //         (Enemy, int, int, int, int)[] dep_array3 = {
        //             (defEnemies[1], 5, 0, 1, 0),
        //             (defEnemies[1], 6, 0, 1, 0)
        //         };
        //         return dep_array3;
        //     case 5:
        //         break;
        //         (Enemy, int, int, int, int)[] dep_array5 = {
        //             (defEnemies[2], 2, 2, 1, 0),
        //             (defEnemies[2], 3, 2, 0, 0),

        //             (defEnemies[2], 2, 4, 1, 0),
        //             (defEnemies[2], 3, 4, 0, 0),

        //             (defEnemies[2], 2, 6, 1, 0),
        //             (defEnemies[2], 3, 6, 0, 0)
        //         };
        //         return dep_array5;
        //     default:
        //         break;
        // }
        
        return null;
    }
    public override (int, int, int)[] get_player_reinforcements(int roundNumber)
    {
        //unit id, row, depth into that row
        // switch (roundNumber)
        // {
        //     case 1:
        //         (int, int, int)[] dep_array1 = {
        //             (1, 12, 5), // friday
        //         };
        //         return dep_array1;
        //     default:
        //         break;
        // }
        
        return null;
    }

}