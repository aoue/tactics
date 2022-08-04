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
    [SerializeField] protected int nextPartIndex; //the index of the part the overworld will resume at.
    [SerializeField] protected int starting_power;
    [SerializeField] protected string win_obj_descr;
    [SerializeField] protected string loss_obj_descr;
   
    [SerializeField] protected TextAsset script;
    [SerializeField] protected int[] eventRounds; //add round number if event in for that round.
    [SerializeField] protected AudioClip[] musicList;
    [SerializeField] protected AudioClip[] soundList;

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
    public virtual (Enemy, int, int)[] get_enemy_spots()
    {
        //returns an array of units and coords representing unit starting spots.
        (Enemy, int, int)[] dep_array = {
            (defEnemies[0], 4, 4)//,
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

    //getters
    public int get_nextPartIndex() { return nextPartIndex; }
    public int get_starting_power() { return starting_power; }
    public string get_lossDescr() { return loss_obj_descr; }
    public string get_winDescr() { return win_obj_descr; }
    public TextAsset get_script() { return script; }
    public AudioClip get_track(int which) { return musicList[which]; }
    public AudioClip get_sound(int which) { return soundList[which]; }
}
