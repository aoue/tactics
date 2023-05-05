using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GridHelper
{
    //holds grid logic functions that are used in multiple places.
    //Naturally, they should be centralized, so they are centralized here.

    public bool within_border(int local_x, int local_y, int map_x_border, int map_y_border)
    {
        return (local_x < map_x_border && local_x >= 0 && local_y < map_y_border && local_y >= 0);
    }

    public void atk_dfs(Tile dest, HashSet<Tile> v, Tile start, int rangeLeft, int min_range, Tile[,] myGrid, int map_x_border, int map_y_border, Trait t, bool originTile)
    {
        //here, dest is the tile the unit is imagining it is on. We have to use dest.x,y instead of real x,y
        if (Math.Abs(dest.x - start.x) + Math.Abs(dest.y - start.y) >= min_range && start.get_canBeTargeted())
        {
            v.Add(start);
        }

        if (!originTile && !t.get_ignores_blocking_terrain() && (start.get_blocksAttacks() || !start.get_canBeTargeted())) return;

        if (rangeLeft == 0) return;

        List<Tile> adjacentTiles = new List<Tile>();
        //add tiles based on coordinates, as long as they are not out of bounds.
        if (within_border(start.x + 1, start.y, map_x_border, map_y_border) && myGrid[start.x + 1, start.y] != null) adjacentTiles.Add(myGrid[start.x + 1, start.y]);
        if (within_border(start.x - 1, start.y, map_x_border, map_y_border) && myGrid[start.x - 1, start.y] != null) adjacentTiles.Add(myGrid[start.x - 1, start.y]);
        if (within_border(start.x, start.y + 1, map_x_border, map_y_border) && myGrid[start.x, start.y + 1] != null) adjacentTiles.Add(myGrid[start.x, start.y + 1]);
        if (within_border(start.x, start.y - 1, map_x_border, map_y_border) && myGrid[start.x, start.y - 1] != null) adjacentTiles.Add(myGrid[start.x, start.y - 1]);

        foreach (Tile next in adjacentTiles)
        {
            atk_dfs(dest, v, next, rangeLeft - 1, min_range, myGrid, map_x_border, map_y_border, t, false);          
        }
    }

    public HashSet<Tile> get_all_possible_attack_origins(Trait t, Tile[,] myGrid, Tile dest)
    {
        //corresponding function is CombatGrid.cs is 'void highlight_attack(Trait t)'
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
                //to left
                for (int i = dest.x - 1; i > dest.x - t.get_range() - 1; i--)
                {
                    if (within_border(i, dest.y, map_x_border, map_y_border) && myGrid[i, dest.y] != null && myGrid[i, dest.y].get_canBeTargeted())
                    {
                        //if the tile is valid, then add to list you can hit
                        if (Math.Abs(dest.x - i) >= t.get_min_range())
                        {
                            origins.Add(myGrid[i, dest.y]);
                        }
                        //stop exploring if the tile blocks attacks, though.
                        if (!t.get_ignores_blocking_terrain() && myGrid[i, dest.y].get_blocksAttacks())
                        {
                            break;
                        }
                    } else break;
                }
                //to right
                for (int i = dest.x + 1; i < dest.x + t.get_range() + 1; i++)
                {
                    if (within_border(i, dest.y, map_x_border, map_y_border) && myGrid[i, dest.y] != null && myGrid[i, dest.y].get_canBeTargeted())
                    {
                        //if the tile is valid, then add to list you can hit
                        if (Math.Abs(dest.x - i) >= t.get_min_range())
                        {
                            origins.Add(myGrid[i, dest.y]);
                        }
                        //stop exploring if the tile blocks attacks, though.
                        if (!t.get_ignores_blocking_terrain() && myGrid[i, dest.y].get_blocksAttacks())
                        {
                            break;
                        }
                    } else break;
                }
                //to top
                for (int j = dest.y + 1; j < dest.y + t.get_range() + 1; j++)
                {
                    if (within_border(dest.x, j, map_x_border, map_y_border) && myGrid[dest.x, j] != null && myGrid[dest.x, j].get_canBeTargeted())
                    {
                        if (Math.Abs(dest.y - j) >= t.get_min_range())
                        {
                            origins.Add(myGrid[dest.x, j]);
                        }
                        if (!t.get_ignores_blocking_terrain() && myGrid[dest.x, j].get_blocksAttacks())
                        {
                            break;
                        }
                    } else break;
                }
                //to bottom
                for (int j = dest.y - 1; j > dest.y - t.get_range() - 1; j--)
                {
                    if (within_border(dest.x, j, map_x_border, map_y_border) && myGrid[dest.x, j] != null && myGrid[dest.x, j].get_canBeTargeted())
                    {
                        if (Math.Abs(dest.y - j) >= t.get_min_range())
                        {
                            origins.Add(myGrid[dest.x, j]);
                        }
                        if (!t.get_ignores_blocking_terrain() && myGrid[dest.x, j].get_blocksAttacks())
                        {
                            break;
                        }
                    } else break;
                }
                break;
            case TargetingType.SQUARE:
                for (int i = dest.x - 1; i < dest.x + 1 + 1; i++)
                {
                    for (int j = dest.y - 1; j < dest.y + 1 + 1; j++)
                    {
                        //if the tile is on the grid, and is not the unit's tile.
                        if (myGrid[i, j] != null && within_border(i, j, map_x_border, map_y_border) && myGrid[i, j].get_canBeTargeted() && !(i == dest.x && j == dest.y) && Math.Abs(dest.x - i) + Math.Abs(dest.y - j) >= t.get_min_range() )
                        {
                            origins.Add(myGrid[i, j]);
                        }
                    }
                }
                break;
            case TargetingType.RADIUS:
                //borrow dfs code.
                atk_dfs(dest, origins, myGrid[dest.x, dest.y], t.get_range(), t.get_min_range(), myGrid, map_x_border, map_y_border, t, true);
                origins.Remove(myGrid[dest.x, dest.y]);
                break;
            case TargetingType.SELF:
                origins.Add(myGrid[dest.x, dest.y]);
                break;
        }
        return origins;
    }
    
    public List<Tile> generate_targetList(Trait t, Tile[,] myGrid, int x_click, int y_click, int x_unit, int y_unit, HashSet<Tile> visited)
    {
        //helper function for traits and generating targetlists. For scoring attacks, etc.

        int map_x_border = myGrid.GetLength(0);
        int map_y_border = myGrid.GetLength(1);

        List<Tile> targetList = new List<Tile>();

        switch (t.get_AoEType())
        {
            case AoEType.SINGLE:
                targetList.Add(myGrid[x_click, y_click]);
                break;
            case AoEType.ALL_BETWEEN:
                //first, we need to determine if the line is vertical or horizontal.
                if (y_click == y_unit)
                {
                    //then the line is horizontal.
                    //next, the line could be to the right or to the left.
                    if (x_click > x_unit)
                    {
                        for (int i = x_unit + 1; i < x_click + 1; i++)
                        {
                            if (myGrid[i, y_unit] != null) targetList.Add(myGrid[i, y_unit]);
                        }
                    }
                    else
                    {
                        for (int i = x_click; i < x_unit; i++)
                        {
                            if (myGrid[i, y_unit] != null) targetList.Add(myGrid[i, y_unit]);
                        }
                    }
                }
                else
                {
                    //otherwise, the line must be vertical.
                    //next, the line could be to the top or to the bottom.
                    //to the top.
                    if (y_click > y_unit)
                    {
                        for (int j = y_unit + 1; j < y_click + 1; j++)
                        {
                            if (myGrid[x_unit, j] != null) targetList.Add(myGrid[x_unit, j]);
                        }
                    }
                    else
                    {
                        for (int j = y_click; j < y_unit; j++)
                        {
                            if (myGrid[x_unit, j] != null) targetList.Add(myGrid[x_unit, j]);
                        }
                    }
                }
                break;
            case AoEType.ALL:
                foreach (Tile t2 in visited)
                {
                    if (t2 != null) targetList.Add(myGrid[t2.x, t2.y]);
                }
                break;
            case AoEType.ADJACENT_FOUR:
                // this one hits the target tile plus the four tiles next to it.
                // (Have to check that each tile is within the map borders here)
                targetList.Add(myGrid[x_click, y_click]);
                if (within_border(x_click - 1, y_click, map_x_border, map_y_border) && myGrid[x_click - 1, y_click] != null) targetList.Add(myGrid[x_click - 1, y_click]);
                if (within_border(x_click + 1, y_click, map_x_border, map_y_border) && myGrid[x_click + 1, y_click] != null) targetList.Add(myGrid[x_click + 1, y_click]);
                if (within_border(x_click, y_click - 1, map_x_border, map_y_border) && myGrid[x_click, y_click - 1] != null) targetList.Add(myGrid[x_click, y_click - 1]);
                if (within_border(x_click, y_click + 1, map_x_border, map_y_border) && myGrid[x_click, y_click + 1] != null) targetList.Add(myGrid[x_click, y_click + 1]);
                break;
            case AoEType.WAVE_3:
                // this one hits the target tile and the two tiles adj to it that are not on the same row as the user
                targetList.Add(myGrid[x_click, y_click]);
                //first, we need to determine if the line is vertical or horizontal.
                if (y_click == y_unit)
                {
                    //then the line is horizontal.
                    //which means we add the tiles that are on the top and bottom
                    if (within_border(x_click, y_click - 1, map_x_border, map_y_border) && myGrid[x_click, y_click - 1] != null) targetList.Add(myGrid[x_click, y_click - 1]);
                    if (within_border(x_click, y_click + 1, map_x_border, map_y_border) && myGrid[x_click, y_click + 1] != null) targetList.Add(myGrid[x_click, y_click + 1]);    
                }
                else
                {
                    //otherwise, the line must be vertical.
                    //which means we add the tiles that are on the left and right
                    if (within_border(x_click - 1, y_click, map_x_border, map_y_border) && myGrid[x_click - 1, y_click] != null) targetList.Add(myGrid[x_click - 1, y_click]);
                    if (within_border(x_click + 1, y_click, map_x_border, map_y_border) && myGrid[x_click + 1, y_click] != null) targetList.Add(myGrid[x_click + 1, y_click]); 
                }
                break;
        }
        return targetList;
    }

}
