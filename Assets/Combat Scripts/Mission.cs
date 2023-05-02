using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;
using System.Linq;

public class Mission : MonoBehaviour
{
    [SerializeField] protected Tile[] missionTiles;
    [SerializeField] protected Enemy[] defEnemies;

    //mission settings
    // -win cond
    // -loss cond
    [SerializeField] private int clearExp; // exp given on mission clear.
    [SerializeField] private int LTC; // the low turn count threshold.
    [SerializeField] private int nextPartIndex; //the index of the part the overworld will resume at.
    [SerializeField] private int starting_power;
    [SerializeField] private string win_obj_descr;
    [SerializeField] private string loss_obj_descr;
    [SerializeField] private string[] side_objectives_descrs; //descriptions for each side objective. parallel.
    [SerializeField] private int[] objectives_rewards; //exp reward for each objective accomplished. main is 0, 1+ is side.

    [SerializeField] private TextAsset script;
    [SerializeField] private int[] eventRounds; //add round number if event in for that round.
    [SerializeField] private AudioClip[] musicList;
    [SerializeField] private AudioClip[] soundList;

    public bool has_event(int roundNumber)
    {
        //returns an int[] where every turn where there should be an event is an element.
        if (eventRounds.Contains(roundNumber)) return true;
        return false;
    }

    //virtuals
    //mission win/loss conditions
    public virtual bool is_mission_won(Unit[] pl, List<Unit> el, Tile[,] grid, int roundNumber, List<Tile> baseList)
    {
        //default is all enemy units dead
        if (el.Count == 0)
        {
            return true;
        }
        return false;
    }
    public virtual bool is_mission_lost(Unit[] pl, List<Unit> el, Tile[,] grid, int roundNumber, List<Tile> baseList)
    {
        //default is all player units dead
        for(int i = 0; i < pl.Length; i++)
        {
            if (pl[i] != null)
            {
                return false;
            }
        }
        return true;
    }
    public virtual string print_mission_briefing()
    {
        string ret = "You should just do your best. (sample). Also, display LTC limit.";
        return ret;
    }
    
    
    
    public float get_exp_mult(Unit[] playerUnits, List<Tile> baseList, int turnsPassed)
    {
        // we calculate an exp multiplier based on factors including:
        // per alive player unit,
        // per owned base
        // low turn count (add for each round below LTC)

        // add exp from defeated enemies
        float buildMult = 1f;

        // add exp for units still alive
        //(a multiplier? add 5% for each unit still alive?)
        for(int i = 0; i < playerUnits.Length; i++)
        {
            if (playerUnits[i] != null) buildMult += 0.05f;
        }

        // add for each base owned by the player
        for(int i = 0; i < baseList.Count; i++)
        {
            if (baseList[i].get_ownership() == BaseOwnership.PLAYER) buildMult += 0.04f;
        }

        // add for each turn under LTC
        int turnDifference = turnsPassed - LTC;
        if (turnDifference > 0) buildMult += (float)(0.02 * turnDifference);

        return buildMult;
    }

    //map setup
    public virtual Tile[,] get_layout()
    {
        //returns an array representing the map
        Tile[,] layout = new Tile[5, 5] {
            { missionTiles[0], missionTiles[1], missionTiles[0], missionTiles[0], missionTiles[0]},
            { missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[1], missionTiles[0]},
            { missionTiles[1], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0]},
            { missionTiles[2], missionTiles[0], missionTiles[1], missionTiles[1], missionTiles[0]},
            { missionTiles[1], missionTiles[0], missionTiles[0], missionTiles[0], missionTiles[0]}
        };
        //the mission layout will maintain this orientation in game.
        return layout;
    }
    public virtual int get_layout_x_dim() { return 12; }
    public virtual int get_layout_y_dim() { return 12; }

    //unit setup and reinforcements
    public virtual (int, int, int)[] get_deployment_spots()
    {
        //returns an array of unit IDs and coords representing unit starting spots.
        (int, int, int)[] dep_array = {
            (1, 0, 0),
            (0, 3, 3)
        };

        return dep_array;
    }
    public virtual (Enemy, int, int, int, int)[] get_enemy_spots()
    {
        //returns an array of units and information needed to spawn them in.
        //unit, row, depth into that row, act delay, times to level up
        (Enemy, int, int, int, int)[] dep_array = {
            (defEnemies[0], 4, 4, 0, 0)//,
            //(defEnemies[0], 3, 2),
            //(defEnemies[0], 1, 3)
        };

        return dep_array;
    }
    public virtual (Enemy, int, int, int, int)[] get_enemy_reinforcements(int roundNumber)
    {
        //unit, row, depth into that row, act delay, times to level up
        return null;
    }
    public virtual (int, int, int)[] get_player_reinforcements(int roundNumber)
    {
        //unit id, row, depth into that row
        return null;
    }

    //getters
    public int get_LTC() { return LTC; }
    public int get_clearExp() { return clearExp; }
    public int get_nextPartIndex() { return nextPartIndex; }
    public int get_starting_power() { return starting_power; }
    public TextAsset get_script() { return script; }
    public AudioClip get_track(int which) { return musicList[which]; }
    public AudioClip get_sound(int which) { return soundList[which]; }
}
