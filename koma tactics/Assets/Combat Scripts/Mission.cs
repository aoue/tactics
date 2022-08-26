using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;
using System.Linq;

public class Mission : MonoBehaviour
{
    //mission objective notes.
    //victory:
    // -defeat all units in boss list
    // -move unit to tile
    // -survive turns
    // -reach certain PW
    // -kill x enemies (where x < total number of enemies)
    //defeat:
    // -unit killed (e.g. the train)
    // -all units killed
    // -turns pass
    // -enemy unit reaches tile

    //tiles
    [SerializeField] protected Tile[] missionTiles;
    //enemy auto-deployed units types (prafabs)
    [SerializeField] protected Enemy[] defEnemies;

    //mission settings
    // -win cond
    // -loss cond
    [SerializeField] private int nextPartIndex; //the index of the part the overworld will resume at.
    [SerializeField] private int starting_power;
    [SerializeField] private string win_obj_descr;
    [SerializeField] private string loss_obj_descr;
    [SerializeField] private int mission_clear_exp; //the exp given for completing the mission.
    [SerializeField] private string[] side_objectives_descrs; //descriptions for each side objective. parallel.
    [SerializeField] private int[] objectives_rewards; //exp reward for each objective accomplished. main is 0, 1+ is side.

    [SerializeField] private TextAsset script;
    [SerializeField] private int[] eventRounds; //add round number if event in for that round.
    [SerializeField] private AudioClip[] musicList;
    [SerializeField] private AudioClip[] soundList;

    //virtuals
    public bool has_event(int roundNumber)
    {
        //returns an int[] where every turn where there should be an event is an element.
        if (eventRounds.Contains(roundNumber)) return true;
        return false;
    }

    //mission win/loss conditions
    public virtual bool is_mission_won(Unit[] pl, List<Unit> el, Tile[,] grid)
    {
        //default is all enemy units dead
        if (el.Count == 0)
        {
            return true;
        }
        return false;
    }
    public virtual bool is_mission_lost(Unit[] pl, List<Unit> el, Tile[,] grid)
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
    
    //side objectives
    public virtual bool[] side_objectives_states(Unit[] pl, List<Unit> el, int roundNumber, bool anyPlayerCasualties)
    {
        //returns list of ints, where each int corresponds to the state of a side objective.
        //each element corresponds to a side objective result.
        // ObjectiveState.in_progress: objective not completed.
        // false: objective not completed
        // true: objective completed

        return null;
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
    public virtual (Enemy, int, int, int)[] get_enemy_spots()
    {
        //returns an array of units and coords representing unit starting spots.
        (Enemy, int, int, int)[] dep_array = {
            (defEnemies[0], 4, 4, 0)//,
            //(defEnemies[0], 3, 2),
            //(defEnemies[0], 1, 3)
        };

        return dep_array;
    }
    public virtual (Enemy, int, int)[] get_enemy_reinforcements(int roundNumber)
    {
        switch (roundNumber)
        {
            case 0:
                break;
            case 1:
                //(Enemy, int, int)[] dep_array = {
                //    (defEnemies[0], 4, 0),
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

    //objectives helpers
    public string print_objectives(bool isClear, bool isBriefing, Unit[] pl, List<Unit> el, int roundNumber, bool anyPlayerCasualties)
    {
        //returns the descr + dscription of each side objective.
        //1 per line.
        //max 4.

        //first, do main objective
        string buildStr = win_obj_descr + "\n";
        
        bool[] successStates = side_objectives_states(pl, el, roundNumber, anyPlayerCasualties);
        if (successStates == null) return buildStr;

        for (int i = 0; i < side_objectives_descrs.Length; i++)
        {
            buildStr += "\n-" + side_objectives_descrs[i];
        }

        return buildStr;
    }
    public string print_objectives_rewards(bool isClear, Unit[] pl, List<Unit> el, int roundNumber, bool anyPlayerCasualties)
    {
        string buildStr = "";

        if (isClear)
        {
            buildStr += "—Passed— (" + mission_clear_exp + " exp!)\n";
        }

        bool[] successStates = side_objectives_states(pl, el, roundNumber, anyPlayerCasualties);
        if (successStates == null) return buildStr;

        for (int i = 0; i < side_objectives_descrs.Length; i++)
        {
            buildStr += "\n";
            if (successStates[i])
            {
                buildStr += "—Passed— +" + objectives_rewards[i] + " exp!";
            }
            else
            {
                buildStr += "—Failed—";
            }
        }

        return buildStr;
    }
    public int get_objectives_exp(Unit[] pl, List<Unit> el, int roundNumber, bool anyPlayerCasualties)
    {
        int sum = mission_clear_exp;
        bool[] successStates = side_objectives_states(pl, el, roundNumber, anyPlayerCasualties);
        for (int i = 0; i < successStates.Length; i++)
        {
            if (successStates[i])
            {
                sum += objectives_rewards[i];
            }
        }
        return sum;
    }

    //getters
    public int get_nextPartIndex() { return nextPartIndex; }
    public int get_starting_power() { return starting_power; }
    public string get_lossDescr() { return loss_obj_descr; }
    public string get_winDescr() { return win_obj_descr; }
    public TextAsset get_script() { return script; }
    public AudioClip get_track(int which) { return musicList[which]; }
    public AudioClip get_sound(int which) { return soundList[which]; }
}
