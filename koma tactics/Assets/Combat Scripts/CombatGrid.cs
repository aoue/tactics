using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq; //for List.Min()


enum State { SELECT_UNIT, SELECT_MOVEMENT, SELECT_TARGET, DEPLOYING, ENEMY, BETWEEN_ROUNDS };
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
    [SerializeField] private Button nextRoundButton;
    [SerializeField] private Image[] turnPatternImages;
    [SerializeField] private Image turnPatternMarker;
    [SerializeField] private Image briefingDisplay;
    [SerializeField] private Text briefingWinText;
    [SerializeField] private Text briefingLossText;
    [SerializeField] private GameObject deploymentObj;
    [SerializeField] private Sprite defaultUnitShortcutSprite;

    private Tile[,] myGrid;
    private State gameState;

    //game state
    private BaseOwnership pastBaseState; //used to revert base's state on movement cancel.
    private int past_active_unit_x; //for returning the unit to its org positin on right click
    private int past_active_unit_y;//for returning the unit to its org positin on right click
    private Unit active_unit; //the unit being ordered
    private Trait active_ability; //the move that we are selecting targets for.

    private int map_x_border;
    private int map_y_border;

    private int turnPatternIndex;
    private bool[] turnPattern; //list of bools representing the turn pattern. True: player's turn, False: enemy turn.
    private int enemyPW; //enemy power (i.e. the enemy's global mana pool.)
    private int pw; //party power (i.e. the party's global mana pool.)
    private BattleBrain brain;

    [SerializeField] private Unit[] reserveParty;
    private Unit[] playerUnits;
    private List<Unit> enemyUnits;

    private HashSet<Tile> visited; //for tile selection and highlighting.
    private List<Tile> targetHighlightGroup; //for showing what tiles will be highlighted for an attack.
    private Tile movementHighlightTile;

    private List<Tile> baseList; //used to check all base tiles. (for power and deployment highlighting)
    

    void Start()
    {
        playerUnits = new Unit[8];
        brain = new BattleBrain();
        baseList = new List<Tile>();
        visited = new HashSet<Tile>();
        targetHighlightGroup = new List<Tile>();

        display_grid(baseMission);
        display_units(baseMission);
        cam.setup(map_x_border, map_y_border);
        

        //starting power depends on the mission
        pw = baseMission.get_starting_power();
        update_pw();

        gameState = State.BETWEEN_ROUNDS;
        update_ZoC();

        next_turn();
    }
    void Update()
    {
        //go back system on right click during player turn.
        if (Input.GetMouseButtonDown(1))
        {
            switch (gameState)
            {
                case State.DEPLOYING:
                    //if we're in deploying: return to unit select, hide deployment screen stuff
                    hide_deployment_menu();
                    disable_deployment();
                    break;
                case State.SELECT_MOVEMENT:
                    //if we're in movement select: return to unit select, cancel movement highlights
                    active_unit = null;
                    uInformer.set_recall(false);
                    clear_highlights();
                    if (movementHighlightTile != null)
                    {
                        movementHighlightTile.hide_target_icon();
                        movementHighlightTile = null;
                    }                   
                    start_player_turn();

                    gameState = State.SELECT_UNIT;
                    break;
                case State.SELECT_TARGET:
                    //if we're in target select: return unit to original position, return to movement select, cancel attack highlights and target highlights

                    //change unit pos in game
                    myGrid[active_unit.x, active_unit.y].remove_unit();
                    myGrid[active_unit.x, active_unit.y].set_ownerShip(pastBaseState);
                    active_unit.x = past_active_unit_x;
                    active_unit.y = past_active_unit_y;
                    myGrid[active_unit.x, active_unit.y].place_unit(active_unit);

                    //change unit pos graphically
                    Vector3 dest = get_pos_from_coords(active_unit.x, active_unit.y);
                    active_unit.transform.position = dest;

                    clear_highlights();
                    clear_target_highlights();
                    movementHighlightTile.hide_target_icon();
                    movementHighlightTile = null;

                    //reset ZoC, too, of course
                    update_ZoC();

                    //setup movement select
                    highlight_tiles_mv(active_unit);

                    gameState = State.SELECT_MOVEMENT;
                    break;
            }
            

        }
        
        //spacebar to recenter camera on selected unit, or start round
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Debug.Log("space bar detected. gameState = " + gameState);
            if (gameState == State.BETWEEN_ROUNDS)
            {
                click_start_round_button();
            }
            else if ((int)gameState < 3 && active_unit != null)
            {
                Vector3 moveHere = get_pos_from_coords(active_unit.x, active_unit.y) + new Vector3(0f, 0f, -10f);
                cam.jump_to(moveHere);
            }
        }

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

                if (newObj is BaseTile)
                {
                    //Debug.Log("adding tile to baselist");
                    baseList.Add(newObj);
                }
            }
        }
    }
    public void display_units(Mission m)
    {
        //does the initial deployment on units based on the mission setup.

        foreach(Unit u in reserveParty)
        {
            u.set_deployed(false);
        }

        //show a unit on a tile.
        for (int i = 0; i < m.get_deployment_spots().Length; i++)
        {
            int x_pos = m.get_deployment_spots()[i].Item2;
            int y_pos = m.get_deployment_spots()[i].Item3;

            //Vector3 instPos = new Vector3(2 * transform_x(x_pos), 2 * transform_y(y_pos), 0f);
            Vector3 instPos = get_pos_from_coords(x_pos, y_pos);

            //cause unit to be shown.
            m.get_deployment_spots()[i].Item1.set_deployed(true);
            Unit inst_u = Instantiate(m.get_deployment_spots()[i].Item1, instPos, transform.rotation);

            //tile.place_unit()
            myGrid[x_pos, y_pos].place_unit(inst_u);

            inst_u.start_of_mission(); //do start of mission setup

            inst_u.x = x_pos;
            inst_u.y = y_pos;
            playerUnits[i] = (inst_u);

            //setup the shortcut button:
            // -hide text
            // -put unit's box image into the slot.
            unit_shortcut_buttons[i].gameObject.transform.GetChild(0).gameObject.GetComponent<Text>().text = "";
            unit_shortcut_buttons[i].GetComponent<Image>().sprite = inst_u.get_box_p();
        }

        //deploy enemies too
        enemyUnits = new List<Unit>();
        for (int i = 0; i < m.get_enemy_spots().Length; i++)
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
            if (u == null) continue;
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
    void set_turnPattern_display()
    {
        //called at start of round to set turn display based on turnPattern.
        //has 5 orbs. (max)
        //colours them blue or red based player/enemy.
        //sets highlight to 1st orb.

        for (int i = 0; i < turnPattern.Length; i++)
        {
            if (turnPattern[i])
            {
                //set color to player color (blue)
                turnPatternImages[i].color = Color.blue;
            }
            else
            {
                //set color to enemy color (red)
                turnPatternImages[i].color = Color.red;
            }
            turnPatternImages[i].enabled = true;
        }
        for (int i = turnPattern.Length; i < turnPatternImages.Length; i++)
        {
            turnPatternImages[i].enabled = false;
        }
    }
    public void hover_briefing_button()
    {
        //called when player hovers briefing button.
        //gets information from mission to display:
        // -win objective
        // -lose objective
        briefingWinText.text = baseMission.get_winDescr();
        briefingLossText.text = baseMission.get_lossDescr();
        briefingDisplay.gameObject.SetActive(true);
        
    }
    public void unhover_briefing_button()
    {
        briefingDisplay.gameObject.SetActive(false);
    }

    //Control
    public void start_player_turn()
    {
        //go through all the party units.
        //for each one, if the unit has ap > 0
        //then set its tile to isValid so we can select them.
        foreach (Unit u in playerUnits)
        {
            if (u == null) continue;
            if (u.get_ap() > 0)
            {
                myGrid[u.x, u.y].highlight_deploy();
                myGrid[u.x, u.y].isValid = true;
                visited.Add(myGrid[u.x, u.y]);
            }           
        }
    }  
    void next_turn()
    {
        //check if end of round
        //Debug.Log("is_end_round =" + is_end_of_round());
        
        if (is_end_of_round() || gameState == State.BETWEEN_ROUNDS)
        {
            gameState = State.BETWEEN_ROUNDS;
            //we're at the start of a round.
            // -add to pw
            foreach(BaseTile b in baseList)
            {
                if (b.get_ownership() == BaseOwnership.PLAYER)
                {
                    pw = Mathf.Min(pw + b.get_pwGeneration(), 30);
                }
            }
            update_pw();
            enable_all_unitShortcuts();
            // -calc turn pattern
            turn_order();
            // -wait for player to click round start button.
            nextRoundButton.interactable = true;
            //Debug.Log("new round: waiting for player to hit start round button");
            turnPatternMarker.enabled = false;
        }
        else
        {
            disable_empty_unitShortcuts();
            if (turnPatternIndex + 1 == turnPattern.Length)
            {
                //loop back around
                //Debug.Log("looping back around yes");
                turnPatternIndex = -1;
            }
            
            //proceed to next turn
            turnPatternIndex += 1;

            //set position of turn pattern marker
            turnPatternMarker.gameObject.transform.position = turnPatternImages[turnPatternIndex].transform.position;

            //Debug.Log("turn=" + turnPattern[turnPatternIndex]);
            //if player turn
            if ( turnPattern[turnPatternIndex] )
            {
                //if no player actions possible; 
                if (player_actions_possible())
                {
                    gameState = State.SELECT_UNIT;
                    start_player_turn();
                }
                                
                else next_turn();
            }
            //else: enemy turn
            else
            {
                if (enemy_actions_possible())
                {
                    gameState = State.ENEMY;
                    start_enemy_turn();
                }
                             
                else next_turn();
            }

            //update turn pattern display(highlight the next orb.)
        }
    }
    void turn_order()
    {
        //so, we're going to be pulling from berwick saga's excellent turn system
        //basically, at the start of a round, we calculate the turn pattern for the entire round.
        //it's a pattern of 2-5 turns, that loop.
        //Note: the first two moves of the pattern must always be: player turn -> enemy turn
        //for design, always put off the doubling of turns to the last part of the pattern.

        //turns are distributed based on odds, and there are 5 different odds.
        // player:enemy
        // 2:1 (player turn -> enemy turn -> player turn)
        // 3:2 (player turn -> enemy turn -> player turn -> enemy turn -> player turn)
        // 1:1 (player turn -> enemy turn -> player turn -> enemy turn)
        // 2:3 (player turn -> enemy turn -> player turn -> enemy turn -> enemy turn)
        // 1:2 (player turn -> enemy turn -> enemy turn)

        int numOfDeployedPlayerUnits = 0;
        for(int i = 0; i < playerUnits.Length; i++)
        {
            if (playerUnits[i] != null)
            {
                numOfDeployedPlayerUnits += 1;
            }
        }

        float unitRatio = ((float)numOfDeployedPlayerUnits) / ((float)enemyUnits.Count);
        //Debug.Log("new unit ratio = " + unitRatio);
        float[] ratioList = new float[5] { 2f, 1.5f, 1f, 0.66f, 0.5f};

        //calculate running min with ratio list.
        float closestRatio = -1f;
        int which = -1;
        for(int i = 0; i < ratioList.Length; i++)
        {
            float difference = Mathf.Abs(unitRatio - ratioList[i]);

            if (closestRatio == -1f || difference < closestRatio)
            {
                closestRatio = difference;
                which = i;
            }
        }
        //Debug.Log("closest ration = " + ratioList[which]);

        switch (which)
        {
            case 0:
                turnPattern = new bool[3] { true, false, true };
                break;
            case 1:
                turnPattern = new bool[5] { true, false, true, false, true };
                break;
            case 2:
                turnPattern = new bool[2] { true, false };
                break;
            case 3:
                turnPattern = new bool[5] { true, false, true, false, false };
                break;
            case 4:
                turnPattern = new bool[3] { true, false, false };
                break;
            default:
                Debug.Log("Error: turn pattern ratio not matched.");
                break;
        }       
        set_turnPattern_display();
    }    
    public void click_start_round_button()
    {
        if (gameState == State.DEPLOYING) return;
        nextRoundButton.interactable = false;
        gameState = State.SELECT_UNIT;
        turnPatternIndex = -1;

        //refresh ap of all units
        for (int i = 0; i < playerUnits.Length; i++)
        {
            if (playerUnits[i] != null)
            {
                unit_shortcut_buttons[i].GetComponent<Image>().color = new Color(1f, 1f, 1f);
                playerUnits[i].refresh();
            }
            
        }

        foreach (Unit u in enemyUnits)
        {
            u.refresh();
        }
        turnPatternMarker.enabled = true;
        next_turn();
    }
    bool is_end_of_round()
    {
        //return true if end of round (all units have ap = 0)
        //return false otherwise
        return !player_actions_possible() && !enemy_actions_possible();    
    }
    bool player_actions_possible()
    {
        foreach (Unit u in playerUnits)
        {
            if (u == null) continue;
            if (u.get_ap() > 0) return true;
        }
        return false;
    }
    bool enemy_actions_possible()
    {
        foreach (Unit u in enemyUnits)
        {
            if (u.get_ap() > 0) return true;
        }
        return false;
    }

    //Enemy Turn/AI
    void start_enemy_turn()
    {
        //for the moment, do nothing.
        //Debug.Log("enemy turn passing");

        //set closest enemy ap to 0
        foreach (Unit u in enemyUnits)
        {
            if (u.get_ap() > 0)
            {
                u.dec_ap();
                break;
            }
        }

        Unit enemyChosenUnit = select_enemy_unit();
        select_enemy_action(enemyChosenUnit);
        //next_turn();
    }
    Unit select_enemy_unit()
    {
        int chosenIndex = -2;
        int runningMax = -1;
        for(int i = 0; i < enemyUnits.Count; i++)
        {
            if (enemyUnits[i].get_ap() > 0)
            {
                int score = enemyUnits[i].calculate_priority();
                if (runningMax == -1 || score > runningMax)
                {
                    runningMax = score;
                    chosenIndex = i;
                }
            }
        }

        Debug.Log("chosenIndex is " + chosenIndex + " with a pri score of " + runningMax);
        return enemyUnits[chosenIndex];
    }
    void select_enemy_action(Unit chosenUnit)
    {
        //generate all the possible movement destinations for the unit.
        //for each position:
        //  if the manhattan distance between the unit and the closest player unit < range
        //      for each active trait that the unit has:
        //          generate targetlist (can be 1 unit, can be more.) 
        //          score targetlist; save it. (hitting allies is negative. Elites will never do it, regardless.)

        //highlight all possible movement locations
        //myGrid[chosenUnit.x, chosenUnit.y].highlight_target_mv();
        highlight_tiles_mv(chosenUnit, false);

        //so we now have visited set up as a list of all possible destination tiles.
        //find the best (destination tile, trait attack, targetlist) triple.
        int runningMax = -1;
        (Tile, int, List<Unit>) best = (null, -2, null);
        
        foreach(Tile t in visited)
        {
            //find position of closest player unit.
            int score = chosenUnit.score_move(closestPlayerTileToTile(t), t, e_tilesAddedToZoC(chosenUnit.x, chosenUnit.y, chosenUnit), myGrid, visited);

            Debug.Log("scoring dest: " + t.x + ", " + t.y + " | score = " + score);

            if (runningMax == -1 || score > runningMax)
            {
                runningMax = score;
                best = (t, chosenUnit.get_bestTraitIndex(), chosenUnit.get_bestTargetList());
            }         
        }
        //at the end of this, we should have:
        // -destination tile chosen
        // -trait chosen
        // -targetlist in hand
        Tile temp = best.Item1;
        Debug.Log("chosen tile = " + temp.x + ", " + temp.y);

    }
    int closestPlayerTileToTile(Tile t)
    {
        //return the tile occupied by a player unit that is closest to t.
        //(closest in manhattan distance)

        int runningMin = -1;
        //Tile closest = null;
        for (int i = 0; i < playerUnits.Length; i++)
        {
            //calc manhattan distance
            if (playerUnits[i] != null)
            {
                int score = Math.Abs(t.x - myGrid[playerUnits[i].x, playerUnits[i].y].x) + Math.Abs(t.y - myGrid[playerUnits[i].x, playerUnits[i].y].y);
                //Debug.Log("score = " + score);
                if (runningMin == -1 || score < runningMin)
                {
                    runningMin = score;
                    //closest = myGrid[playerUnits[i].x, playerUnits[i].y];
                }
            }
            
        }
        //return closest;
        return runningMin;
    }
    int e_tilesAddedToZoC(int x, int y, Unit u)
    {
        //returns the number of tiles a move to this tile t, by a unit u, would add enemy's ZoC.
        //(note: add, meaning we only count tiles that are not already part of the ZoC)
        int score = 0;
        //east and west tiles:
        for (int i = x - u.get_controlRange(); i < x + u.get_controlRange() + 1; i++)
        {
            //if the tile is on the grid, set tile.playerControlled to true.
            if (within_border(i, u.y))
            {
                score += 1;
            }
        }
        //north and south tiles:
        for (int j = y - u.get_controlRange(); j < y + u.get_controlRange() + 1; j++)
        {
            //if the tile is on the grid, set tile.playerControlled to true.
            if (within_border(x, j))
            {
                score += 1;
            }
        }
        return score;
    }

    //Tile interactivity
    public void traitButton_clicked(int which)
    {
        //when a traitButton is clicked 
        //if during select target state:
        // -this means the player wants to change the move they are using.
        // -so, highlight and set valid tiles according to this new move's specifications. 
        //(if passive trait, then just clear highlight)

        //if we are in target select, then you can change what ability is going to be used
        //(but only if you're hovering over the active unit)
        if (gameState == State.SELECT_TARGET && active_unit == uInformer.get_heldUnit())
        {
            //if trait exists and is active
            if (active_unit.get_traitList()[which] != null && !active_unit.get_traitList()[which].get_isPassive())
            {
                clear_highlights();
                active_ability = active_unit.get_traitList()[which];

                //hide target highlights
                clear_target_highlights();

                highlight_attack(active_ability);
            }

        }
        //if we are not in target select, then you can update the unit informer
        uInformer.traitButtonHover(which);

    }
    public void tile_clicked(int x_pos, int y_pos, Unit heldUnit)
    {
        //validity depends on the gameState. 
        //Everytime the gameState changes, we have to go and set validity/highlights of tiles again.

        //this is the only time when you can read the moves of an enemy
        if (gameState == State.SELECT_UNIT)
        {
            uInformer.set_heldUnit(heldUnit);
        }
        if (!myGrid[x_pos, y_pos].isValid) return;
        switch (gameState)
        {
            case State.DEPLOYING:
                //Debug.Log("YOU HAVE CLICKED A TILE TO DEPLOY.");
                active_unit.set_deployed(true);
                Vector3 instPos = get_pos_from_coords(x_pos, y_pos);
                Unit inst_u = Instantiate(active_unit, instPos, transform.rotation);
                active_unit = null;

                myGrid[x_pos, y_pos].place_unit(inst_u);
                inst_u.start_of_mission(); //do start of mission setup
                inst_u.x = x_pos;
                inst_u.y = y_pos;

                //shove the unit in the first open spot in playerunits
                for (int i = 0; i < playerUnits.Length; i++)
                {
                    if (playerUnits[i] == null)
                    {
                        playerUnits[i] = inst_u;
                        unit_shortcut_buttons[i].gameObject.transform.GetChild(0).gameObject.GetComponent<Text>().text = "";
                        unit_shortcut_buttons[i].GetComponent<Image>().sprite = inst_u.get_box_p();
                        break;
                    }
                }               
                gameState = State.BETWEEN_ROUNDS;
                disable_deployment();
                update_ZoC();
                break;

            case State.SELECT_UNIT:
                //Debug.Log("YOU HAVE CLICKED A UNIT TO ORDER.")
                
                //hold onto unit
                active_unit = heldUnit;

                //prepare for select movement
                clear_highlights(); //remove selection tile highlights
                highlight_tiles_mv(active_unit); //add movement tile highlights
                uInformer.set_heldUnit(heldUnit);
                gameState = State.SELECT_MOVEMENT;

                //if the unit starts movement on a base:
                // then enable recall button
                if (myGrid[active_unit.x, active_unit.y] is BaseTile)
                {
                    //only allow recall button if there would still be at least one other player unit deployed.
                    for (int i = 0; i < playerUnits.Length; i++)
                    {
                        if ( playerUnits[i] != null && playerUnits[i] != active_unit)
                        {
                            uInformer.set_recall(true);
                        }
                    }                   
                }              
                break;

            case State.SELECT_MOVEMENT:
                //Debug.Log("YOU HAVE CLICKED A TILE TO MOVE THE UNIT THERE.")
                uInformer.set_recall(false);

                //move unit to this tile
                past_active_unit_x = active_unit.x;
                past_active_unit_y = active_unit.y;
                myGrid[active_unit.x, active_unit.y].remove_unit();
                myGrid[active_unit.x, active_unit.y].hide_target_icon();

                myGrid[x_pos, y_pos].place_unit(active_unit);
                active_unit.x = x_pos;
                active_unit.y = y_pos;

                //modify unit's position/new vector for unit
                Vector3 dest = get_pos_from_coords(x_pos, y_pos);
                active_unit.transform.position = dest;

                //prepare for select target              
                gameState = State.SELECT_TARGET;
                clear_highlights(); //remove movement tile highlights
                //add target selection highlights
                active_ability = active_unit.get_traitList()[0];
                highlight_attack(active_ability);

                //update tile ownership (if applicable)
                if (myGrid[active_unit.x, active_unit.y] is BaseTile)
                {
                    pastBaseState = myGrid[active_unit.x, active_unit.y].get_ownership();
                    myGrid[active_unit.x, active_unit.y].set_ownerShip(BaseOwnership.PLAYER);                                 
                }
                update_ZoC();
                break;

            case State.SELECT_TARGET:
                //Debug.Log("YOU HAVE CLICKED A TILE TO BE THE LOCATION OF A MOVE");
                //what's the process:
                // -generate the list of affected targets.
                // -for each target
                //  -apply damage/heal
                //  -check if dead
                // -finally, update ZoC and drain power.

                List<Unit> affectedUnits = tileList_to_targetList(generate_tileList(active_ability, x_pos, y_pos));

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
                        int dmg = brain.calc_damage(active_unit, target, active_ability, myGrid[target.x, target.y]);

                        //a unit cannot kill itself. It always leaves itself with at least 1 hp.
                        //(stops stalemates, etc.)
                        //target.take_dmg(dmg, active_unit == target);      
                        //No, a unit can kill itself. No stalemate, because we check for player win first.
                        //Also, come on, blowing yourself up is hilarious. You need to be able to do that.
                        target.take_dmg(dmg);                       

                        if (target.get_isAlly() && target.get_isBroken())
                        {
                            //grey out unit box
                            for(int i = 0; i < playerUnits.Length; i++)
                            {
                                if (target == playerUnits[i])
                                {
                                    unit_shortcut_buttons[i].GetComponent<Image>().color = new Color(27f / 255f, 27f / 255f, 27f / 255f);
                                }
                            }
                        }

                        //check dead:
                        //if dead, remove from playerList/enemyList depending on isAlly.
                        if (target.get_isDead())
                        {
                            anyKills = true;
                            if (target.get_isAlly())
                            {
                                //remove from player list
                                for (int i = 0; i < playerUnits.Length; i++)
                                {
                                    if (target == playerUnits[i])
                                    {
                                        playerUnits[i] = null;

                                        //remove from unit box shortcut list too
                                        unit_shortcut_buttons[i].GetComponent<Image>().color = new Color(1f, 1f, 1f);
                                        unit_shortcut_buttons[i].GetComponent<Image>().sprite = defaultUnitShortcutSprite;
                                        unit_shortcut_buttons[i].gameObject.transform.GetChild(0).gameObject.GetComponent<Text>().text = "Deploy\nUnit";
                                        break;
                                    }
                                }                                
                            }
                            else
                            {
                                //remove from enemy list
                                enemyUnits.Remove(target);
                            }
                            //if target is the one being shown in the unit informer; then clear unit informer.
                            if (uInformer.get_heldUnit() == target) uInformer.hide();

                            myGrid[target.x, target.y].remove_unit();
                            Destroy(target.gameObject);                           
                        }
                    }
                }

                //remove target icons
                clear_target_highlights();

                pw -= active_ability.get_pwCost();
                update_pw();

                if (anyKills) update_ZoC();

                //prepare for enemy phase
                if (active_unit != null)
                {
                    //grey out unit box
                    active_unit.dec_ap();
                    for (int i = 0; i < playerUnits.Length; i++)
                    {
                        if (active_unit == playerUnits[i])
                        {
                            //grey out unit button.
                            unit_shortcut_buttons[i].GetComponent<Image>().color = new Color(27f / 255f, 27f / 255f, 27f / 255f);
                        }
                    }
                    
                    active_unit = null;
                }
                
                active_ability = null;
                uInformer.set_heldUnit(null);
                clear_highlights();
                movementHighlightTile.hide_target_icon();
                movementHighlightTile = null;
                gameState = State.ENEMY;

                //check win/loss
                //we check player victory first; yes, it's biased.
                if (baseMission.is_mission_won(playerUnits, enemyUnits, myGrid))
                {
                    Debug.Log("Mission is won.");
                }
                else if (baseMission.is_mission_lost(playerUnits, enemyUnits, myGrid))
                {
                    Debug.Log("Mission is lost.");
                }
                else
                {
                    next_turn();
                }
                
                break;
        }      
    }
    public void tile_hovered(int x_pos, int y_pos, Unit heldUnit)
    {
        //if we are in select target, then mousing over a tile should
        //display what the AoE area would end up looking like. 
        if (gameState == State.SELECT_TARGET)
        {
            highlight_targets(x_pos, y_pos, active_ability);
        }
        else if (gameState == State.SELECT_MOVEMENT)
        {
            //highlight hovered tile during movement
            
            if (movementHighlightTile != null)
            {
                movementHighlightTile.hide_target_icon();
            }
            movementHighlightTile = myGrid[x_pos, y_pos];
            movementHighlightTile.highlight_target_mv();
        }
            
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
    void highlight_targets(int x_pos, int y_pos, Trait ability)
    {
        //highlights what square will be highlighted by a move's Ao.    
        //first; clear it.
        foreach (Tile t in targetHighlightGroup)
        {
            //hide target icon
            t.hide_target_icon();
        }
        targetHighlightGroup.Clear();

        //(if origin tile is valid, then highlight based on that.)
        if (myGrid[x_pos, y_pos].isValid)
        {
            //gather all tiles based on active_ability and position
            //and highlight them
            targetHighlightGroup = generate_tileList(ability, x_pos, y_pos);

            foreach (Tile t in targetHighlightGroup)
            {
                t.highlight_target(!ability.get_isHeal());
            }
        }

        
    }
    void highlight_attack(Trait t)
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
            t2.highlight_atk(!t.get_isHeal());
        }

    }
    void highlight_tiles_mv(Unit u, bool isPlayer = true)
    {
        //highlight tiles for movement, and also set tile's isValid to true, based on:
        //a unit. (position, movement, and flight/etc)

        //keep all highlighted tiles in a list, so we can easily unhighlight them later.

        //efficient way to highlight all times?
        //have to do it tile by tile, expanding the network, to account for movement penalties.        

        //starting from the 4 adjacent tiles, expand outwards, adding every reachable tile to a set.
        myGrid[u.x, u.y].isValid = true;

        dfs(visited, myGrid[u.x, u.y], u.get_movement(), isPlayer, u);

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
    void dfs(HashSet<Tile> v, Tile start, int moveLeft, bool isPlayer, Unit u)
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
            for(int i = 0; i < u.get_traitList().Length; i++)
            {
                if (u.get_traitList()[i] != null && u.get_traitList()[i].get_isPassive())
                {
                    mvCostList.Add(u.get_traitList()[i].modify_movementCost(next));
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
                    dfs(v, next, moveLeft - u.get_movement(), isPlayer, u);
                }
                else if (!isPlayer && next.player_controlled)
                {
                    dfs(v, next, moveLeft - u.get_movement(), isPlayer, u);
                }
                else
                {
                    dfs(v, next, moveLeft - mvCost, isPlayer, u);
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
    void clear_highlights()
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
    void clear_target_highlights()
    {
        foreach (Tile t in targetHighlightGroup)
        {
            t.hide_target_icon();
        }
        targetHighlightGroup.Clear();
    }

    //Deployment and unit shortcuts
    public void hover_reserveUnit_slot(int which)
    {
        //fills uInformer on hover.
        if (reserveParty[which] != null)
        {
            uInformer.fill(reserveParty[which], pw, false);
        }
    }
    public void hover_unit_shortcut(int which)
    {
        //fills uInformer on hover.
        if (playerUnits[which] != null)
        {
            uInformer.fill(playerUnits[which], pw, false);
        }       
    }
    public void click_unit_shortcut(int which)
    {
        //(only allowed during unit selecting)
        //if (gameState != State.SELECT_UNIT) return;

        //which: 0 to 7. Corresponds to unit's index in player units list.

        if (playerUnits[which] != null)
        {
            //then jump
            Vector3 moveHere = get_pos_from_coords(playerUnits[which].x, playerUnits[which].y) + new Vector3(0f, 0f, -10f);
            cam.jump_to(moveHere);
        }
        else
        {
            //if we're inbetween rounds, then clicking an empty slot opens the deployment menu.
            if (gameState == State.BETWEEN_ROUNDS)
            {
                //Debug.Log("Yes; opening deployment menu (the unit list)");
                show_deployment_menu();
            }
        } 
    }
    public void click_deployment_unitSlot(int which)
    {
        //which: 0-17
        //at this point, we have already done checks;
        //if we're here, we know the unit is not already deployed and can be .
        active_unit = reserveParty[which];
        enable_deployment();      
    }
    void show_deployment_menu()
    {
        //show the deployment menu
        gameState = State.DEPLOYING;
        nextRoundButton.interactable = false;
        if (is_deployment_possible())
        {
            //for each the units in the reserveParty           
            // -if not already deployed
            // -if our pw > unit's pw deploy cost
            for (int i = 0; i < reserveParty.Length; i++)
            {
                GameObject obj = deploymentObj.gameObject.transform.GetChild(i).gameObject;
                obj.transform.GetChild(0).GetComponent<Image>().sprite = reserveParty[i].get_box_p();
                obj.transform.GetChild(1).GetComponent<Text>().text = reserveParty[i].get_unitName() + " | " + reserveParty[i].get_pwCost() + " PW";

                if (!reserveParty[i].get_isDeployed() && reserveParty[i].get_pwCost() <= pw)
                {
                    //then allow button to be clickable                    
                    obj.GetComponent<Button>().interactable = true;

                }
                else
                {
                    obj.GetComponent<Button>().interactable = false;
                }
            }
            for (int i = reserveParty.Length; i < 18; i++)
            {
                //deploymentObj.gameObject.transform.GetChild(i).GetComponent<Button>().interactable = false;
                deploymentObj.gameObject.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < 18; i++)
            {
                deploymentObj.gameObject.transform.GetChild(i).GetComponent<Button>().interactable = false;
            }
        }       
        deploymentObj.SetActive(true);
    }
    public void hide_deployment_menu()
    {
        //used to close without deploying a thing.
        deploymentObj.SetActive(false);
        gameState = State.BETWEEN_ROUNDS;
        nextRoundButton.interactable = true;
    }  
    void enable_deployment()
    {
        //highlights / validates bases.
        //only possible to call if deployment is possible.
        foreach(BaseTile b in baseList)
        {
            if (b.get_ownership() == BaseOwnership.PLAYER)
            {
                b.isValid = true;
                b.highlight_deploy();
                visited.Add(b);
            }
        }
        deploymentObj.SetActive(false);
    }
    void disable_deployment()
    {
        foreach (Tile t in visited)
        {
            t.isValid = false;
            t.remove_highlight();
        }
        nextRoundButton.interactable = true;

        //we've just successfully deployed a unit;
        //this changes the unit ratio, so we recalc it and update turn pattern display too.
        turn_order();
        set_turnPattern_display();
    }
    bool is_deployment_possible()
    {
        //returns true if the player has any bases they can deploy from.
        //i.e. true if at least one base is player owned and not occupied.
        foreach(BaseTile b in baseList)
        {
            if (b.get_ownership() == BaseOwnership.PLAYER && !b.occupied())
            {
                //Debug.Log("deployment is possible");
                return true;
            }
        }
        //Debug.Log("deployment is not possible");
        return false;
    }
    void enable_all_unitShortcuts()
    {
        for (int i = 0; i < unit_shortcut_buttons.Length; i++)
        {
            unit_shortcut_buttons[i].interactable = true;            
        }
    }
    void disable_empty_unitShortcuts()
    {
        for(int i = 0; i < unit_shortcut_buttons.Length; i++)
        {
            if (playerUnits[i] == null)
            {
                unit_shortcut_buttons[i].interactable = false;
            }
            else
            {
                unit_shortcut_buttons[i].interactable = true;
            }
        }
    }
    public void click_recall_button()
    {
        //at this point, authentication is already done.
        //When this function is called; undeploy the active unit.
        //set their home prefab state to undeployed.

        //similar to dying.

        //remove from player list
        for (int i = 0; i < playerUnits.Length; i++)
        {
            if (active_unit == playerUnits[i])
            {
                playerUnits[i] = null;

                //remove from unit box shortcut list too
                unit_shortcut_buttons[i].GetComponent<Image>().color = new Color(1f, 1f, 1f);
                unit_shortcut_buttons[i].GetComponent<Image>().sprite = defaultUnitShortcutSprite;
                unit_shortcut_buttons[i].gameObject.transform.GetChild(0).gameObject.GetComponent<Text>().text = "Deploy\nUnit";
                break;
            }
        }
        //set the prefab's state
        for (int i = 0; i < reserveParty.Length; i++)
        {
            if (active_unit.get_uniqueUnitID() == reserveParty[i].get_uniqueUnitID())
            {
                //carry all stats over, etc.
                reserveParty[i].recall(active_unit.get_hp());
                break;
            }
        }

        //if target is the one being shown in the unit informer; then clear unit informer.
        if (uInformer.get_heldUnit() == active_unit) uInformer.hide();

        //remove gameobject from grid, both logically and visually
        myGrid[active_unit.x, active_unit.y].remove_unit();
        active_unit.gameObject.SetActive(false);

        //finally, end the player turn.
        active_unit = null;
        uInformer.set_recall(false);
        clear_highlights();
        if (movementHighlightTile != null)
        {
            movementHighlightTile.hide_target_icon();
            movementHighlightTile = null;
        }
        gameState = State.ENEMY;

        next_turn();
    }

    List<Unit> tileList_to_targetList(List<Tile> tileList)
    {
        //remove all tiles with null unit.
        List<Unit> targetList = new List<Unit>();

        foreach (Tile t in tileList)
        {
            if (t.occupied())
            {
                targetList.Add(t.get_heldUnit());
            }
        }
        return targetList;
    }
    List<Tile> generate_tileList(Trait t, int x_click, int y_click)
    {
        //the player has decided on what tile they want to attack, its coordinates are (x_click, y_click)
        //Now, based on the trait's AoEType, we do that.
        //return a list of units, which will all be hit.

        //for further reading on each of the aoe types, see Trait.cs

        List<Tile> targetList = new List<Tile>();

        switch (t.get_AoEType())
        {
            case AoEType.SINGLE:               
                targetList.Add(myGrid[x_click, y_click]);
                break;
            case AoEType.ALL_BETWEEN:
                //first, we need to determine if the line is vertical or horizontal.
                if (y_click == active_unit.y)
                {
                    //then the line is horizontal.
                    //next, the line could be to the right or to the left.
                    if (x_click > active_unit.x)
                    {
                        for (int i = active_unit.x + 1; i < x_click + 1; i++)
                        {
                            targetList.Add(myGrid[i, active_unit.y]);
                        }
                    }              
                    else
                    {
                        for (int i = x_click; i < active_unit.x; i++)
                        {
                            targetList.Add(myGrid[i, active_unit.y]);
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
                        for (int j = active_unit.y + 1; j < y_click + 1; j++)
                        {
                            targetList.Add(myGrid[active_unit.x, j]);
                        }
                    }                                          
                    else
                    {
                        for (int j = y_click; j < active_unit.y; j++)
                        {
                            targetList.Add(myGrid[active_unit.x, j]);
                        }                            
                    }
                        
                    
                }
                break;
            case AoEType.ALL:
                foreach (Tile t2 in visited)
                {
                    targetList.Add(myGrid[t2.x, t2.y]);
                }
                break;
        }

        return targetList;
    }


}
