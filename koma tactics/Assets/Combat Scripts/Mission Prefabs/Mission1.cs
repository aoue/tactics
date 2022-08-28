using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mission1 : Mission
{
    //The file for mission1

    //mission win/loss conditions
    public override bool is_mission_won(Unit[] pl, List<Unit> el, Tile[,] grid)
    {
        //win if all enemies are defeated.
        if (el.Count == 0)
        {
            return true;
        }
        return false;
    }
    public override bool is_mission_lost(Unit[] pl, List<Unit> el, Tile[,] grid)
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

}
