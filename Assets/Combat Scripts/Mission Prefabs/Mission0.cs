﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mission0 : Mission
{
    //The file for mission0.
    //tile legend:
    // 0: empty/snow
    // 1: light woods
    // 2: left-right tracks 
    // 3: heavy woods
    // 4: defend tile

    //mission win/loss conditions
    public override bool is_mission_won(Unit[] pl, List<Unit> el, Tile[,] grid, int roundNumber, List<Tile> baseList)
    {
        //win if all enemies are defeated.
        //if (el.Count == 0) 
        if (el.Count <= 5) 
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
        Tile[,] layout0 = new Tile[7, 12] {
            { missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[1], missionTiles[0], missionTiles[0], missionTiles[1], missionTiles[0], missionTiles[1], missionTiles[1], missionTiles[0], missionTiles[0]},
            { missionTiles[0], missionTiles[0], missionTiles[3], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[1], missionTiles[0], missionTiles[0], missionTiles[3], missionTiles[3]},
            { missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[3], missionTiles[3], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0]},
            { missionTiles[1], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[3], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[1], missionTiles[0], missionTiles[0]},
            { missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[1], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[1]},
            { missionTiles[2], missionTiles[2], missionTiles[2], missionTiles[2], missionTiles[2], missionTiles[2], missionTiles[2], missionTiles[2], missionTiles[2], missionTiles[2], missionTiles[2], missionTiles[2]},
            { missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[1], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[1], missionTiles[0]}
            /*           
            { missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[1], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[1], missionTiles[0]},
            { missionTiles[0], missionTiles[3], missionTiles[3], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[3], missionTiles[0], missionTiles[0], missionTiles[0]},
            { missionTiles[0], missionTiles[0], missionTiles[1], missionTiles[1], missionTiles[0], missionTiles[0], missionTiles[3], missionTiles[3], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0]},
            { missionTiles[1], missionTiles[0], missionTiles[1], missionTiles[0], missionTiles[3], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[1], missionTiles[0]},
            { missionTiles[0], missionTiles[1], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[1], missionTiles[0], missionTiles[1], missionTiles[0], missionTiles[0], missionTiles[0]}
            */
        };
        //the mission layout will maintain this orientation in game.
        
        return layout0;
    }
    public override int get_layout_x_dim() { return 7; }
    public override int get_layout_y_dim() { return 12; }

    //unit setup and reinforcements
    public override (int, int, int)[] get_deployment_spots()
    {
        //returns an array of unit IDs and coords representing unit starting spots.
        //unit id, row, depth into that row (if no unit with the id in reserve party, then we fail silently. all good)
        (int, int, int)[] dep_array = {
            (0, 3, 1),           
            (1, 4, 1)//,
            //(2, 4, 0)
        };
        return dep_array;
    }
    public override (Enemy, int, int, int, int)[] get_enemy_spots()
    {
        //returns an array of units and information needed to spawn them in.
        //unit, row, depth into that row, act delay, times to level up
        (Enemy, int, int, int, int)[] dep_array = {
           
            //light pair
            (defEnemies[0], 0, 3, 1, 0),
            (defEnemies[0], 1, 4, 2, 0),

            //med pair
            (defEnemies[1], 2, 9, 3, 0),
            (defEnemies[1], 3, 9, 3, 0),

            //heavy pair
            (defEnemies[2], 4, 11, 4, 0),
            (defEnemies[2], 6, 11, 4, 0)
        };

        return dep_array;
    }
    public override (Enemy, int, int, int, int)[] get_enemy_reinforcements(int roundNumber)
    {
        //unit, row, depth into that row, act delay, times to level up
        switch (roundNumber)
        {
            case 1:
                (Enemy, int, int, int, int)[] dep_array3 = {
                    (defEnemies[0], 0, 10, 0, 1),
                    (defEnemies[0], 1, 10, 0, 2)
                };
                return dep_array3;
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
            case 1:
                (int, int, int)[] dep_array3 = {
                    //bonelord
                    (2, 6, 0)
                };
                return dep_array3;
            default:
                break;
        }
        return null;
    }

}