using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


enum State { SELECT_UNIT, SELECT_MOVEMENT, SELECT_TARGET, DEPLOYING, ENEMY };
public class CombatGrid : MonoBehaviour
{
    //responsible for instatiating and managing the combat grid
    //and for general combat control.

    //turn order:
    //player:
    // - select unit
    // - select tile to move unit
    // - do attack/ability
    // - turn ends
    //enemy:
    // - ...

    [SerializeField] private Mission baseMission;
    [SerializeField] private CameraController cam;
    [SerializeField] private Text pwText;
    [SerializeField] private Image active_unit_portrait; //for players only.
    [SerializeField] private Button[] unit_shortcut_buttons;
    [SerializeField] private UnitInformer uInformer;
    [SerializeField] private TileInformer tInformer;

    private Tile[,] myGrid;
    private State gameState;
    private Unit active_unit;

    private int map_x_border;
    private int map_y_border;

    private List<Unit> playerUnits;
    private List<Enemy> enemyUnits;
    //private List<> enemy units
    private HashSet<Tile> visited; //for tile selection and highlighting.

    void Start()
    {
        display_grid(baseMission);
        display_units(baseMission);
        cam.setup(map_x_border, map_y_border);
        

        setup_selection();
    }

    //Display
    private Vector3 get_pos_from_coords(int x, int y) { return new Vector3(2 * y, 2 * (map_x_border - 1 - x), 0f); }
    public void display_grid(Mission m)
    {
        //sets up the level based on m.
        myGrid = new Tile[m.get_layout_x_dim(), m.get_layout_y_dim()];
        map_x_border = m.get_layout_x_dim();
        map_y_border = m.get_layout_y_dim();

        //for every tile in the layout
        for (int i = 0; i < m.get_layout_x_dim(); i++)
        {
            for (int j = 0; j < m.get_layout_y_dim(); j++)
            {
                //instantiate the tile in a position based on i and j.
                //Vector3 instPos = new Vector3(2 * transform_x(i), 2 * transform_y(j), 0f);
                Vector3 instPos = get_pos_from_coords(i, j);
                
                Tile newObj = Instantiate(m.get_layout()[i, j], instPos, transform.rotation, transform);
                newObj.set_coords(i, j);

                myGrid[i, j] = newObj;
            }
        }
    }
    public void display_units(Mission m)
    {
        //does the initial deployment on units based on the mission setup.

        //show a unit on a tile.
        playerUnits = new List<Unit>();
        for (int i = 0; i < m.get_deployment_spots().Length; i++)
        {
            int x_pos = m.get_deployment_spots()[i].Item2;
            int y_pos = m.get_deployment_spots()[i].Item3;

            //Vector3 instPos = new Vector3(2 * transform_x(x_pos), 2 * transform_y(y_pos), 0f);
            Vector3 instPos = get_pos_from_coords(x_pos, y_pos);

            //cause unit to be shown.
            Unit inst_u = Instantiate(m.get_deployment_spots()[i].Item1, instPos, transform.rotation);

            //tile.place_unit()
            myGrid[x_pos, y_pos].place_unit(inst_u);

            inst_u.start_of_mission(); //do start of mission setup

            inst_u.x = x_pos;
            inst_u.y = y_pos;
            playerUnits.Add(inst_u);

            //setup the shortcut button:
            // -hide text
            // -put unit's box image into the slot.
            unit_shortcut_buttons[i].gameObject.transform.GetChild(0).gameObject.GetComponent<Text>().text = "";
            unit_shortcut_buttons[i].GetComponent<Image>().sprite = inst_u.get_box_p();
        }

        //deploy enemies too
        enemyUnits = new List<Enemy>();
        for (int i = 0; i < m.get_deployment_spots().Length; i++)
        {
            int x_pos = m.get_enemy_spots()[i].Item2;
            int y_pos = m.get_enemy_spots()[i].Item3;

            //Vector3 instPos = new Vector3(2 * transform_x(x_pos), 2 * transform_y(y_pos), 0f);
            Vector3 instPos = get_pos_from_coords(x_pos, y_pos);

            //cause unit to be shown.
            Enemy inst_u = Instantiate(m.get_enemy_spots()[i].Item1, instPos, transform.rotation);

            //tile.place_unit()
            myGrid[x_pos, y_pos].place_unit(inst_u);

            inst_u.start_of_mission(); //do start of mission setup

            inst_u.x = x_pos;
            inst_u.y = y_pos;
            enemyUnits.Add(inst_u);
        }

    }
    public void update_ZoC()
    {
        //recalculate zone of control every time the game enters selection mode.
        //(either player or enemy)

        //first, reset all tiles zones of control.
        for (int i = 0; i < map_x_border; i++)
        {
            for (int j = 0; j < map_y_border; j++)
            {
                myGrid[i, j].player_controlled = false;
                myGrid[i, j].enemy_controlled = false;
                myGrid[i, j].set_ZoC_color();
            }
        }

        //then, for all player units, mark all tiles.
        //and highlight them.
        foreach(Unit u in playerUnits)
        {
            //east and west tiles:
            for(int i = u.x - u.get_controlRange(); i < u.x + u.get_controlRange() + 1; i++)
            {
                //if the tile is on the grid, set tile.playerControlled to true.
                if ( within_border(i, u.y) )
                {
                    myGrid[i, u.y].player_controlled = true;
                    myGrid[i, u.y].set_ZoC_color();
                }
            }
            //north and south tiles:
            for (int j = u.y - u.get_controlRange(); j < u.y + u.get_controlRange() + 1; j++)
            {
                //if the tile is on the grid, set tile.playerControlled to true.
                if (within_border(u.x, j))
                {
                    myGrid[u.x, j].player_controlled = true;
                    myGrid[u.x, j].set_ZoC_color();
                }
            }
        }

        
        foreach(Enemy u in enemyUnits)
        {
            //east and west tiles:
            for (int i = u.x - u.get_controlRange(); i < u.x + u.get_controlRange() + 1; i++)
            {
                //if the tile is on the grid, set tile.playerControlled to true.
                if (within_border(i, u.y))
                {
                    myGrid[i, u.y].enemy_controlled = true;
                    myGrid[i, u.y].set_ZoC_color();
                }
            }
            //north and south tiles:
            for (int j = u.y - u.get_controlRange(); j < u.y + u.get_controlRange() + 1; j++)
            {
                //if the tile is on the grid, set tile.playerControlled to true.
                if (within_border(u.x, j))
                {
                    myGrid[u.x, j].enemy_controlled = true;
                    myGrid[u.x, j].set_ZoC_color();
                }
            }
        }        
    }

    //Control
    public void setup_selection()
    {
        update_ZoC();
        gameState = State.SELECT_UNIT;        
        if (visited == null) visited = new HashSet<Tile>();
        //go through all the party units.
        //for each one, if the unit has ap > 0
        //then set its tile to isValid so we can select them.
        foreach (Unit u in playerUnits)
        {
            if (u.get_ap() > 0)
            {
                myGrid[u.x, u.y].highlight_mv();
                myGrid[u.x, u.y].isValid = true;
                visited.Add(myGrid[u.x, u.y]);
            }
            
        }

    }
    public void click_unit_shortcut(int which)
    {
        //(only allowed during unit selecting)
        if (gameState != State.SELECT_UNIT) return;

        //which: 0 to 7. Corresponds to unit's index in player units list.

        //if the slot is filled (i.e. if which player units length)
        if (which < playerUnits.Count)
        {
            //then jump camera to that unit
            //Vector3 moveHere = new Vector3(2 * transform_x(playerUnits[which].x), 2 * transform_y(playerUnits[which].y), -10f);
            Vector3 moveHere = get_pos_from_coords(playerUnits[which].x, playerUnits[which].y) + new Vector3(0f, 0f, -10f);
            cam.jump_to(moveHere);
        }
        

        //otherwise, open deploy menu.
        //set gameState to DEPLOYING
    }

    //Tile interactivity
    public void tile_clicked(int x_pos, int y_pos, Unit heldUnit)
    {       
        //validity depends on the gameState. 
        //Everytime the gameState changes, we have to go and set validity/highlights of tiles again.
        
        switch (gameState)
        {
            case State.SELECT_UNIT:
                //YOU HAVE CLICKED A UNIT TO ORDER.
                if (!myGrid[x_pos, y_pos].isValid) return;

                //hold onto unit
                active_unit = heldUnit;

                //set state to select movement
                clear_highlights(); //remove selection tile highlights
                highlight_tiles_mv(active_unit); //add movement tile highlights
                gameState = State.SELECT_MOVEMENT;
                break;

            case State.SELECT_MOVEMENT:
                //YOU HAVE CLICKED A TILE TO MOVE THE UNIT THERE.
                if (!myGrid[x_pos, y_pos].isValid) return;

                //move unit to this tile
                myGrid[active_unit.x, active_unit.y].remove_unit();

                myGrid[x_pos, y_pos].place_unit(active_unit);
                active_unit.x = x_pos;
                active_unit.y = y_pos;

                //modify unit's position/new vector for unit
                //Vector3 dest = new Vector3(2 * transform_x(x_pos), 2 * transform_y(y_pos), 0f);
                Vector3 dest = get_pos_from_coords(x_pos, y_pos);
                active_unit.transform.position = dest;

                //set state to select target
                //gameState = State.SELECT_TARGET;
                gameState = State.SELECT_TARGET;
                clear_highlights(); //remove movement tile highlights
                //add target selection highlights

                update_ZoC();

                break;

            case State.SELECT_TARGET:
                if (!myGrid[x_pos, y_pos].isValid) return;
                //YOU HAVE CLICKED A TILE TO BE THE LOCATION OF A MOVE.

                //resolve attack

                //set state to enemy turn

                //if any kills; update_ZoC();

                active_unit.dec_ap();
                active_unit = null;
                break;
        }      
    }
    public void tile_hovered(int x_pos, int y_pos, Unit heldUnit)
    {
        //show tile information
        tInformer.fill(myGrid[x_pos, y_pos]);

        //show hovered unit information (if any)
        uInformer.fill(heldUnit);
    }
    public void highlight_tiles_mv(Unit u)
    {
        //highlight tiles for movement, and also set tile's isValid to true, based on:
        //a unit. (position, movement, and flight/etc)

        //keep all highlighted tiles in a list, so we can easily unhighlight them later.

        //efficient way to highlight all times?
        //have to do it tile by tile, expanding the network, to account for movement penalties.        

        //starting from the 4 adjacent tiles, expand outwards, adding every reachable tile to a set.

        visited = new HashSet<Tile>();
        myGrid[u.x, u.y].isValid = true;
        visited.Add(myGrid[u.x, u.y]);

        dfs(visited, myGrid[u.x, u.y], u.get_movement(), true);

        //now, visited is comprised of all the reachable tiles.
        //for each of them, highlight the tile by changing the colour to blue.
        foreach (Tile t in visited)
        {
            t.highlight_mv();
        }

    }
    void dfs(HashSet<Tile> visited, Tile start, int moveLeft, bool isPlayer)
    {
        //subtract current tile's movement cost
        //moveLeft = moveLeft - active_unit.calc_movementCost(start);           
        if (!start.occupied())
        {
            start.isValid = true;
            visited.Add(start); //add to visited list
        }

        if (moveLeft <= 0) return;

        //add all adjacent tiles to the
        //for each tile adjacent to start, DFS(start)

        List<Tile> adjacentTiles = new List<Tile>();
        //add tiles based on coordinates, as long as they are not out of bounds.
        if (within_border(start.x + 1, start.y)) adjacentTiles.Add(myGrid[start.x + 1, start.y]);
        if (within_border(start.x - 1, start.y)) adjacentTiles.Add(myGrid[start.x - 1, start.y]);
        if (within_border(start.x, start.y + 1)) adjacentTiles.Add(myGrid[start.x, start.y + 1]);
        if (within_border(start.x, start.y - 1)) adjacentTiles.Add(myGrid[start.x, start.y - 1]);

        foreach (Tile next in adjacentTiles)
        {
            //if movement cost is -1, the tile is impassable. Only continue if not.
            if (next.get_movementCost() != -1)
            {
                //if tile is in the opponent's ZoC, then it costs all remaining movement.
                if (isPlayer && next.enemy_controlled)
                {
                    dfs(visited, next, moveLeft - active_unit.get_movement(), isPlayer);
                }
                else if (!isPlayer && next.player_controlled)
                {
                    dfs(visited, next, moveLeft - active_unit.get_movement(), isPlayer);
                }
                else
                {
                    dfs(visited, next, moveLeft - active_unit.calc_movementCost(start), isPlayer);
                }
            }

        }
    }
    bool within_border(int x, int y)
    {
        //dfs helper
        //returns true if the coords given are inside the map border. False otherwise.
        if (x < map_x_border && x >= 0 && y < map_y_border && y >= 0) return true;
        return false;
    }

    public void clear_highlights()
    {
        //unhighlights every tile in the highlighted tiles light
        //also sets isValid to false. Also sets isVisited to false.
        //and clears the list.
        foreach(Tile t in visited)
        {
            t.isValid = false;
            t.remove_highlight();
        }
        visited.Clear();
    }

    
}
