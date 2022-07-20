using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq; //for List.Min()


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
    [SerializeField] private Button[] unit_shortcut_buttons;
    [SerializeField] private UnitInformer uInformer;
    [SerializeField] private TileInformer tInformer;
    [SerializeField] private Slider pwSlider;
    [SerializeField] private Text pwText;

    private Tile[,] myGrid;
    private State gameState;
    private Unit active_unit;
    private Trait active_ability; //the move that we are selecting targets for.

    private int map_x_border;
    private int map_y_border;

    private int pw; //power (i.e. the party's global mana pool.)
    private BattleBrain brain;
    private List<Unit> playerUnits;
    private List<Unit> enemyUnits;
    private HashSet<Tile> visited; //for tile selection and highlighting.

    void Start()
    {
        display_grid(baseMission);
        display_units(baseMission);
        cam.setup(map_x_border, map_y_border);
        brain = new BattleBrain();

        //starting power depends on the mission
        pw = baseMission.get_starting_power();
        update_pw();

        setup_selection();
    }
    void Update()
    {
        //implement:
        // 1-5 keys call traitButton_clicked(1-5)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            traitButton_clicked(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            traitButton_clicked(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            traitButton_clicked(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            traitButton_clicked(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            traitButton_clicked(4);
        }
        //go back system on right click during player turn.
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
        enemyUnits = new List<Unit>();
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
    void update_ZoC()
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
    void update_pw()
    {
        //update pw slider and pw text
        pwSlider.value = pw;
        pwText.text = "POWER " + pw;
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
    public void traitButton_clicked(int which)
    {
        //when a traitButton is clicked 
        //if during select target state:
        // -this means the player wants to change the move they are using.
        // -so, highlight and set valid tiles according to this new move's specifications. 
        //(if passive trait, then just clear highlight)

        //if we are in target select, then you can change what ability is going to be used
        if (gameState == State.SELECT_TARGET)
        {
            if (active_unit.get_traitList()[which] != null && active_unit.get_traitList()[which].get_isPassive() == false)
            {
                clear_highlights();
                active_ability = active_unit.get_traitList()[which];
                highlight_attack(active_ability);
            }
        }
        //if we are not in target select, then you can update the unit informer
        uInformer.traitButtonHover(which);       
    }

    //Tile interactivity
    public void tile_clicked(int x_pos, int y_pos, Unit heldUnit)
    {
        //validity depends on the gameState. 
        //Everytime the gameState changes, we have to go and set validity/highlights of tiles again.

        //this is the only time when you can read the moves of an enemy
        if (gameState == State.SELECT_UNIT)
        {
            uInformer.set_heldUnit(heldUnit);
        }

        switch (gameState)
        {
            case State.SELECT_UNIT:
                //Debug.Log("YOU HAVE CLICKED A UNIT TO ORDER.")
                if (!myGrid[x_pos, y_pos].isValid) return;

                //hold onto unit
                active_unit = heldUnit;

                //prepare for select movement
                clear_highlights(); //remove selection tile highlights
                highlight_tiles_mv(active_unit); //add movement tile highlights
                uInformer.set_heldUnit(heldUnit);
                gameState = State.SELECT_MOVEMENT;
                break;

            case State.SELECT_MOVEMENT:
                //Debug.Log("YOU HAVE CLICKED A TILE TO MOVE THE UNIT THERE.")
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

                //prepare for select target              
                gameState = State.SELECT_TARGET;
                clear_highlights(); //remove movement tile highlights
                //add target selection highlights
                active_ability = active_unit.get_traitList()[0];
                highlight_attack(active_ability);

                update_ZoC();

                break;

            case State.SELECT_TARGET:
                if (!myGrid[x_pos, y_pos].isValid) return;
                //Debug.Log("YOU HAVE CLICKED A TILE TO BE THE LOCATION OF A MOVE");

                //what's the process
                // -generate the list of affected targets.
                // -for each target
                //  -apply damage/heal
                //  -check if dead
                // -finally, update ZoC and drain power.

                List<Unit> affectedUnits = generate_targetList(active_ability, x_pos, y_pos);

                //the move used could be an attack or it could be a heal.
                bool anyKills = false;
                if (active_ability.get_isHeal())
                {
                    foreach (Unit target in affectedUnits)
                    {
                        int heal = brain.calc_heal(active_unit, target, active_ability);
                        target.take_heal(heal);
                    }
                }
                else
                {
                    foreach (Unit target in affectedUnits)
                    {
                        int dmg = brain.calc_damage(active_unit, target, active_ability);
                        
                        //a unit cannot kill itself. It always leaves itself with at least 1 hp.
                        //(stops stalemates, etc.)
                        target.take_dmg(dmg, active_unit == target);                       

                        //check dead:
                        //if dead, remove from playerList/enemyList depending on isAlly.
                        if (target.get_isDead())
                        {
                            anyKills = true;
                            if (target.get_isAlly())
                            {
                                //remove from player list
                                playerUnits.Remove(target);                               
                            }
                            else
                            {
                                //remove from enemy list
                                enemyUnits.Remove(target);
                            }
                            //if target is the one being shown in the unit informer; then clear unit informer.
                            if (uInformer.get_heldUnit() == target) uInformer.hide();

                            Destroy(target.gameObject);                           
                        }
                    }
                }

                pw -= active_ability.get_pwCost();
                update_pw();

                if (anyKills) update_ZoC();

                //prepare for enemy phase
                active_unit.dec_ap();
                active_unit = null;
                active_ability = null;
                uInformer.set_heldUnit(null);
                gameState = State.ENEMY;
                clear_highlights();
                
                break;
        }      
    }
    public void tile_hovered(int x_pos, int y_pos, Unit heldUnit)
    {
        //show tile information
        tInformer.fill(myGrid[x_pos, y_pos]);

        //if friendly unit; show moves with pw. else; set pw as -1
        if (heldUnit == null) return;

        if (heldUnit == active_unit)
        {           
            uInformer.fill(heldUnit, pw, true);
        }
        else
        {
            if (heldUnit.get_isAlly())
            {
                uInformer.fill(heldUnit, pw, false);
            }
            else
            {
                uInformer.fill(heldUnit, -1, false);
            }
        }

                
    }
    public void highlight_attack(Trait t)
    {
        //called when the game enters select target mode.
        //highlights the tiles that are possible targets based on the ability.
        //t, in conjunction with active player's coords, has all the information needed.

        //for a text description of the targeting types, see the legend in Trait.cs
        switch (t.get_targetingType())
        {
            case TargetingType.LINE:                
                for (int i = active_unit.x - t.get_range(); i < active_unit.x + t.get_range() + 1; i++)
                {
                    if (within_border(i, active_unit.y) && i != active_unit.x)
                    {
                        visited.Add(myGrid[i, active_unit.y]);
                    }
                }
                //north and south tiles:
                for (int j = active_unit.y - t.get_range(); j < active_unit.y + t.get_range() + 1; j++)
                {
                    if (within_border(active_unit.x, j) && j != active_unit.y)
                    {
                        visited.Add(myGrid[active_unit.x, j]);                       
                    }
                }
                //visited.Remove(myGrid[active_unit.x, active_unit.y]);
                break;
            case TargetingType.SQUARE:               
                for (int i = active_unit.x - 1; i < active_unit.x + 1 + 1; i++)
                {
                    for (int j = active_unit.y - 1; j < active_unit.y + 1 + 1; j++)
                    {
                        //if the tile is on the grid, and is not the unit's tile.
                        if (within_border(i, j) && !(i == active_unit.x && j == active_unit.y))
                        {
                            visited.Add(myGrid[i, j]);
                        }
                    }
                }
                //visited.Remove(myGrid[active_unit.x, active_unit.y]);
                break;
            case TargetingType.RADIUS:
                //borrow dfs code.               
                atk_dfs(visited, myGrid[active_unit.x, active_unit.y], t.get_range());
                visited.Remove(myGrid[active_unit.x, active_unit.y]);
                break;
            case TargetingType.SELF:
                visited.Add(myGrid[active_unit.x, active_unit.y]);
                break;
        }
        
        foreach (Tile t2 in visited)
        {
            t2.isValid = true;
            t2.highlight_atk();
        }

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

        dfs(visited, myGrid[u.x, u.y], u.get_movement(), true);

        //now, visited is comprised of all the reachable tiles.
        //for each of them, highlight the tile by changing the colour to blue.
        foreach (Tile t in visited)
        {
            if (!t.occupied())
            {
                t.isValid = true;
            }
            t.highlight_mv();
        }

    }
    void dfs(HashSet<Tile> v, Tile start, int moveLeft, bool isPlayer)
    {
        v.Add(start); //add to visited list

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

            //calc new movement cost according to unit's traits.
            List<int> mvCostList = new List<int>();
            for(int i = 0; i < active_unit.get_traitList().Length; i++)
            {
                if (active_unit.get_traitList()[i] != null && active_unit.get_traitList()[i].get_isPassive())
                {
                    mvCostList.Add(active_unit.get_traitList()[i].modify_movementCost(next));
                }
            }
            int mvCost = next.get_movementCost();
            if (mvCostList.Count > 0) { mvCost = mvCostList.Min(); }

            //if mvCost <= 0, then it is impassable. Different values mean different types.  E.g. -1: water, -2: cliffs.
            if (mvCost > 0)
            {
            //if tile is in the opponent's ZoC, then it costs all remaining movement.
                if (isPlayer && next.enemy_controlled)
                {
                    dfs(v, next, moveLeft - active_unit.get_movement(), isPlayer);
                }
                else if (!isPlayer && next.player_controlled)
                {
                    dfs(v, next, moveLeft - active_unit.get_movement(), isPlayer);
                }
                else
                {
                    dfs(v, next, moveLeft - mvCost, isPlayer);
                }
            }

        }
    }
    void atk_dfs(HashSet<Tile> v, Tile start, int rangeLeft)
    {
        v.Add(start);

        List<Tile> adjacentTiles = new List<Tile>();
        //add tiles based on coordinates, as long as they are not out of bounds.
        if (within_border(start.x + 1, start.y)) adjacentTiles.Add(myGrid[start.x + 1, start.y]);
        if (within_border(start.x - 1, start.y)) adjacentTiles.Add(myGrid[start.x - 1, start.y]);
        if (within_border(start.x, start.y + 1)) adjacentTiles.Add(myGrid[start.x, start.y + 1]);
        if (within_border(start.x, start.y - 1)) adjacentTiles.Add(myGrid[start.x, start.y - 1]);

        foreach (Tile next in adjacentTiles)
        {
            if (rangeLeft > 0)
            {
                atk_dfs(v, next, rangeLeft - 1);
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
        foreach (Tile t in visited)
        {
            t.isValid = false;
            t.remove_highlight();
        }
        visited.Clear();
    }
    public List<Unit> generate_targetList(Trait t, int x_click, int y_click)
    {
        //the player has decided on what tile they want to attack, its coordinates are (x_click, y_click)
        //Now, based on the trait's AoEType, we do that.
        //return a list of units, which will all be hit.

        //for further reading on each of the aoe types, see Trait.cs

        List<Unit> targetList = new List<Unit>();

        switch (t.get_AoEType())
        {
            case AoEType.SINGLE:
                if (myGrid[x_click, y_click].get_heldUnit() != null)
                    targetList.Add(myGrid[x_click, y_click].get_heldUnit());
                break;
            case AoEType.ALL_BETWEEN:

                //first, we need to determine if the line is vertical or horizontal.
                bool isHorinzontal;
                if (x_click == active_unit.x)
                {
                    //then the line is horizontal.
                    //next, the line could be to the right or to the left.

                    //to the right
                    if (x_click > active_unit.x)
                    {
                        for (int i = active_unit.x; i < x_click + 1; i++)
                        {
                            if (myGrid[i, active_unit.y].get_heldUnit() != null)
                                targetList.Add(myGrid[i, active_unit.y].get_heldUnit());
                        }
                    }              
                    //to the left.
                    else
                    {
                        for (int i = x_click; i < active_unit.x + 1; i++)
                        {
                            if (myGrid[i, active_unit.y].get_heldUnit() != null)
                                targetList.Add(myGrid[i, active_unit.y].get_heldUnit());
                        }
                    }
                        
                    
                }
                else
                {
                    //otherwise, the line must be vertical.
                    //next, the line could be to the top or to the bottom.
                    //to the top.
                    if (y_click > active_unit.y)
                    {
                        for (int j = active_unit.y; j < y_click + 1; j++)
                        {
                            if (myGrid[active_unit.x, j].get_heldUnit() != null)
                                targetList.Add(myGrid[active_unit.x, j].get_heldUnit());
                        }
                    }
                                           
                    //to the bottom.
                    else
                    {
                        for (int j = y_click; j < active_unit.y + 1; j++)
                        {
                            if (myGrid[active_unit.x, j].get_heldUnit() != null)
                                targetList.Add(myGrid[active_unit.x, j].get_heldUnit());
                        }                            
                    }
                        
                    
                }
                break;
            case AoEType.ALL:
                foreach (Tile t2 in visited)
                {
                    if (myGrid[t2.x, t2.y].get_heldUnit() != null)
                        targetList.Add(myGrid[t2.x, t2.y].get_heldUnit());
                }
                break;
        }

        return targetList;
    }



}
