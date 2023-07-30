using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Pathfinder
{
    // used for pathfinding logic. Assistant to combat grid.

    public bool within_border(int local_x, int local_y, int map_x_border, int map_y_border)
    {
        return (local_x < map_x_border && local_x >= 0 && local_y < map_y_border && local_y >= 0);
    }

    // find manhattan paths
    // public int manhattan_closestPlayerTileToTile(Tile t, Unit[] playerUnits, Tile[,] myGrid)
    // {
    //     //return the tile occupied by a player unit that is closest to t.
    //     //(closest in manhattan distance)
    //     int runningMin = -1;
    //     Tile closest = null;
    //     for (int i = 0; i < playerUnits.Length; i++)
    //     {
    //         //calc manhattan distance
    //         if (playerUnits[i] != null)
    //         {
    //             int score = Math.Abs(t.x - myGrid[playerUnits[i].x, playerUnits[i].y].x) + Math.Abs(t.y - myGrid[playerUnits[i].x, playerUnits[i].y].y);
    //             if (runningMin == -1 || score < runningMin)
    //             {
    //                 runningMin = score;
    //             }
    //         }
            
    //     }
    //     return runningMin;
    // }

    // find actual paths (A*)
    public void reset_pathfinding(Tile[,] myGrid, int map_x, int map_y)
    {
        for (int i = 0; i < map_x; i++)
        {
            for (int j = 0; j < map_y; j++)
            {
                myGrid[i, j].reset_pathfinding();
            }
        }
    }
    public int aStar_closestPlayerTileToTile(Tile src, Tile dest, Tile[,] myGrid, int map_x, int map_y)
    {
        // t is the tile that the enemy unit is going to move to
        // dest is the tile that a player unit is on
        // myGrid is the map
        // return the real distance from t to dest
        reset_pathfinding(myGrid, map_x, map_y);

        // init closedList and add t
        List<Tile> closedList = new List<Tile>();
        closedList.Add(src);

        // init openList and add the starting position's (t's) adjacent tiles
        List<Tile> openList = new List<Tile>();
        openList.AddRange(add_adjacent_tiles(src, dest, closedList, openList, myGrid, map_x, map_y));

        bool reached = false;
        while (!reached && openList.Count > 0)
        {
            int minValue = openList.Min(t => t.tup.f);
            Tile current = openList.First(x => x.tup.f == minValue);

            if (current.tup.h == 0)
            {
                reached = true;
            }
            else
            {
                openList.AddRange(add_adjacent_tiles(current, dest, closedList, openList, myGrid, map_x, map_y));
                openList.Remove(current);
            }
            closedList.Add(current);
        }

        if (reached == false) {
            return 0;
        }
        
        PathfinderTuple cur = closedList[closedList.Count - 1].tup;
        int shortestDist = 0;
        while (cur.g != 0) 
        {
            cur = cur.prev;
            shortestDist += 1;
        }
        return shortestDist;
    }
    List<Tile> add_adjacent_tiles(Tile tile, Tile dest, List<Tile> closedList, List<Tile> openList, Tile[,] myGrid, int map_x, int map_y)
    {
        // for each adj tile, only add if it is within the borders, traversable, and not in closedList
        List<Tile> neighbours = new List<Tile>();

        int xOffset = 0;
        int yOffset = 0;
        for(int i = 0; i < 4; i++)
        {
            //order: up, right, down, left
            switch (i)
            {
                case 0:
                    xOffset = 0; yOffset = 1;
                    break;
                case 1:
                    xOffset = 1; yOffset = 0;
                    break;
                case 2:
                    xOffset = 0; yOffset = -1;
                    break;
                case 3:
                    xOffset = -1; yOffset = 0;
                    break;
            }
            if ( within_border(tile.x + xOffset, tile.y + yOffset, map_x, map_y) )
            {
                Tile tmp = myGrid[tile.x + xOffset, tile.y + yOffset];
                if ( !closedList.Contains(tmp) && tmp.get_movementCost() > 0 )
                {
                    if ( openList.Contains(tmp) )
                    {
                        // update it
                        int new_f = tile.tup.g + 1 + tmp.tup.h;
                        if ( new_f < tmp.tup.f || (new_f == tmp.tup.f && UnityEngine.Random.Range(0, 2) == 1) ) 
                        {
                            tmp.tup.prev = tile.tup;
                            tmp.tup.f = new_f;
                            tmp.tup.g = tile.tup.g + 1;
                            myGrid[tile.x + xOffset, tile.y + yOffset] = tmp;
                        }
                    }
                    else
                    {
                        // otherwise, add it as new
                        tmp.tup.prev = tile.tup;
                        tmp.tup.g = tile.tup.g + 1;
                        tmp.tup.h = get_manhattan(tmp, dest);
                        tmp.tup.f = tmp.tup.g + tmp.tup.h;
                        myGrid[tile.x + xOffset, tile.y + yOffset] = tmp;
                        neighbours.Add(myGrid[tile.x + xOffset, tile.y + yOffset]);
                    }
                }
            }
        }
        return neighbours;
    }
    private int get_manhattan(Tile start, Tile end)
    {
        //gets manhattan distance between two coords, each represented by a tuple.
        return Math.Abs(start.x - end.x) + Math.Abs(start.y - end.y);
    }

}
