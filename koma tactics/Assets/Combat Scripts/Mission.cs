using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] Tile empty;
    [SerializeField] Tile lightlyWooded;

    //player auto-deployed units
    [SerializeField] Unit[] defUnits;

    //enemy auto-deployed units
    [SerializeField] Enemy[] defEnemies;

    //mission settings
    // -win cond
    // -loss cond
    [SerializeField] private int starting_power;

    public virtual Tile[,] get_layout()
    {
        //returns an array representing the map
        Tile[,] layout = new Tile[5, 5] {
            { empty, lightlyWooded, empty, empty, empty},
            { empty, empty, empty, lightlyWooded, empty},
            { lightlyWooded, empty, empty, empty, empty},
            { empty, empty, lightlyWooded, lightlyWooded, empty},
            { lightlyWooded, empty, empty, empty, empty}
        };
        //the mission layout will look like above^.

        return layout;
    }
    public virtual (Unit, int, int)[] get_deployment_spots()
    {
        //returns an array of units and coords representing unit starting spots.
        (Unit, int, int)[] dep_array = { (defUnits[0], 0, 0) };

        return dep_array;
    }
    public virtual (Enemy, int, int)[] get_enemy_spots()
    {
        //returns an array of units and coords representing unit starting spots.
        (Enemy, int, int)[] dep_array = { (defEnemies[0], 1, 2) };

        return dep_array;
    }

    //getters
    public virtual int get_layout_x_dim() { return 5; }
    public virtual int get_layout_y_dim() { return 5; }
    public int get_starting_power() { return starting_power; }

}
