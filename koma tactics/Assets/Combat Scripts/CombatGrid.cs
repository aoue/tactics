using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq; //for List.Min()
using UnityEngine.SceneManagement;


enum State { SELECT_UNIT, SELECT_MOVEMENT, SELECT_TARGET, DEPLOYING, ENEMY, BETWEEN_ROUNDS };
public class CombatGrid : MonoBehaviour
{
    //responsible for instatiating and managing the combat grid and for general combat control.

    private const float pause_before_event_start = 1f; //the pause/fade duration before starting an event.
    private const float enemy_pause_before_attack = 0.35f; //the seconds the enemy unit will pause with possible origin tiles before attacking.
    private const float enemy_pause_before_movement = 0.35f; //the seconds the enemy unit will pause with highlighted movement range before moving.
    private const float movement_animation_speed = 20f; //the number of units a unit moves per second. Remember: one of our tiles is 2 units.
    private const float combat_hpBar_duration = 0.5f; //how long we will watch the unit's hpbars decrease for.
    private const float combat_hpBar_linger = 0.05f; //how long we wait for the period between hpbars finishing decreasing and then dead/broken units being removed/updated
    private const float combat_highlights_linger = 0.5f; //how long we wait for the period between dead/broken units being removed/updated and target highlights being unhighlighted
    private bool animating; //true while animating. Disables player input.

    [SerializeField] private MissionManager missionList; //all missions.
    private Mission loadedMission;

    [SerializeField] private FadeManager fader;
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private CombatAudio audio;   
    [SerializeField] private CombatDialoguer cDia;
    [SerializeField] private CameraController cam;
    [SerializeField] private Button[] unit_shortcut_buttons;
    [SerializeField] private UnitInformer uInformer;
    [SerializeField] private TileInformer tInformer;
    [SerializeField] private Slider pwSlider;
    [SerializeField] private Text pwText;
    [SerializeField] private Button nextRoundButton;
    [SerializeField] private Image[] turnPatternImages;
    [SerializeField] private Image turnPatternMarker;
    [SerializeField] private Image orderImage;
    [SerializeField] private Text orderTitleText;
    [SerializeField] private Text orderdescrText;
    [SerializeField] private Image helpDisplay;
    [SerializeField] private Image briefingDisplay;
    [SerializeField] private Text briefingWinText;
    [SerializeField] private Text briefingLossText;
    [SerializeField] private GameObject deploymentObj;
    [SerializeField] private Sprite defaultUnitShortcutSprite;
    [SerializeField] private Text roundNumberText;

    [SerializeField] private GameObject missionSummaryObj; //the object that all the mission summary stuff is on
    [SerializeField] private Text missionSummaryTitleText; //the title text for the mission. will say 'Mission Clear' or 'Mission Fail'
    [SerializeField] private Text objectivesSummaryText; //displays objective and side objective state
    [SerializeField] private Text expSummaryText; //displays objective and side objective rewards.
    [SerializeField] private Button continueRetryButton; //button that, if mission was won, loads the next part. if mission was lost, reloads the combat scene.
    [SerializeField] private Button mainMenuButton; //button that is only available if the mission was lost. can go to main menu.

    private Tile[,] myGrid;
    private State gameState;
    private bool overState = false;
    private bool playerArrived = false; //set to true if a player unit ends turn on arrival tile.
    private bool enemyArrived = false; //set to true if an enemy unit ends turn on defend tile.
    private bool anyPlayerCasualties = false; //to check if any player units were killed.

    [SerializeField] private Order defaultOrder; //the default order. No effects. So we don't have to put if not null everywhere.

    //game state
    private BaseOwnership pastBaseState; //used to revert base's state on movement cancel.
    private int past_active_unit_x; //for returning the unit to its org positin on right click
    private int past_active_unit_y;//for returning the unit to its org positin on right click
    private Order active_order; //the order assigned this round. (assigned after the first player unit finished moving.)
    private Unit active_unit; //the unit being ordered
    private Trait active_ability; //the move that we are selecting targets for.
    private Tile last_player_end_turn_tile; //the tile that the last active player unit ended its turn on. Influences enemy priority calculations.
    private bool set_order; //true on player's first move. Tells the game to set active_order to the unit's order.

    private (int, List<Tile>, Tile) enemy_active_info; //information about the move the enemy is performing.
    //Tile: dest tile | int: chosen trait index | List<Tile> affected tiles | Tile: origin tile

    private int map_x_border;
    private int map_y_border;
    private List<Tile> zocTiles; //a list of all the tiles marked by ZoC. For resetting here, rather than searching every goddamn tile.

    private int turnPatternIndex;
    private bool[] turnPattern; //list of bools representing the turn pattern. True: player's turn, False: enemy turn.
    private int enemyPW; //enemy power (i.e. the enemy's global mana pool.)
    private int pw; //party power (i.e. the party's global mana pool.)
    private BattleBrain brain;

    //round event control
    private int roundNumber; //for timing round start events and reinforcements, that kind of thing.
    private bool allowRoundEvent; //for timing round start events and reinforcements, that kind of thing.

    [SerializeField] private List<Unit> reserveParty;
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
        zocTiles = new List<Tile>();
        visited = new HashSet<Tile>();
        targetHighlightGroup = new List<Tile>();
        active_order = defaultOrder;
        allowRoundEvent = true;
        roundNumber = 0;
        roundNumberText.text = "—Start—";

        loadedMission = missionList.get_mission(Carrier.Instance.get_nextMissionIndex());
        Carrier.Instance.set_nextPartIndex(loadedMission.get_nextPartIndex());
        reserveParty = Carrier.Instance.get_reserveParty();
        Carrier.Instance.set_exp(0);

        display_grid(loadedMission);
        display_units(loadedMission);
        cam.setup(map_x_border, map_y_border);
        
        //starting power depends on the mission
        pw = loadedMission.get_starting_power();
        update_pw();

        gameState = State.BETWEEN_ROUNDS;
        update_ZoC();

        fader.fade_from_black_cheat(2f);
        next_turn();
    }
    void Update()
    {
        if (animating) return;

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
                    uInformer.set_pass(false);
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
                    if (movementHighlightTile != null)
                    {
                        movementHighlightTile.hide_target_icon();
                        movementHighlightTile = null;
                    }
                    
                    //reset ZoC, too, of course
                    update_ZoC();

                    //setup movement select
                    highlight_tiles_mv(active_unit);

                    gameState = State.SELECT_MOVEMENT;
                    break;
            }
        }
        
        //spacebar to pass attack, or start round
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Debug.Log("space bar detected. gameState = " + gameState);
            if (gameState == State.BETWEEN_ROUNDS)
            {
                //spacebar will start round
                click_start_round_button();
            }
            else if (gameState == State.SELECT_TARGET)
            {
                //spacebar will pass attack
                click_pass_button();
            }
            /*
            else if ((int)gameState < 3 && active_unit != null)
            {
                Vector3 moveHere = get_pos_from_coords(active_unit.x, active_unit.y) + new Vector3(0f, 0f, -10f);
                cam.jump_to(moveHere);
            }
            */
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
    Unit get_unit_from_id(int id)
    {
        //return the unit with the corresponding id from the reserve party.
        for (int i = 0; i < reserveParty.Count; i++)
        {
            if (reserveParty[i].get_uniqueUnitID() == id) return reserveParty[i];
        }
        return null;
    }

    //Display
    public Vector3 get_pos_from_coords(int x, int y) { return new Vector3(2 * y, 2 * (map_x_border - 1 - x), 0f); }
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
            u.start_of_mission();
        }

        //show a unit on a tile.
        for (int i = 0; i < m.get_deployment_spots().Length; i++)
        {
            int unit_id = m.get_deployment_spots()[i].Item1;

            //first, check that unit id is in the party.
            //if it isn't, then the player misses out.
            if ( get_unit_from_id(unit_id) == null ) continue;

            int x_pos = m.get_deployment_spots()[i].Item2;
            int y_pos = m.get_deployment_spots()[i].Item3;

            //Vector3 instPos = new Vector3(2 * transform_x(x_pos), 2 * transform_y(y_pos), 0f);
            Vector3 instPos = get_pos_from_coords(x_pos, y_pos);

            //cause unit to be shown.
            get_unit_from_id(unit_id).set_deployed(true);
            Unit inst_u = Instantiate( get_unit_from_id(unit_id) , instPos, transform.rotation);

            //tile.place_unit()
            myGrid[x_pos, y_pos].place_unit(inst_u);

            inst_u.x = x_pos;
            inst_u.y = y_pos;

            //place the unit into the first empty slot of playerUnits
            for(int j = 0; j < playerUnits.Length; j++)
            {
                if (playerUnits[j] == null)
                {
                    playerUnits[j] = (inst_u);
                    unit_shortcut_buttons[j].gameObject.transform.GetChild(0).gameObject.GetComponent<Text>().text = "";
                    unit_shortcut_buttons[j].GetComponent<Image>().sprite = inst_u.get_box_p();
                    break;
                }
            }           
        }

        //deploy enemies too
        enemyUnits = new List<Unit>();
        for (int i = 0; i < m.get_enemy_spots().Length; i++)
        {
            int x_pos = m.get_enemy_spots()[i].Item2;
            int y_pos = m.get_enemy_spots()[i].Item3;
            int act_delay = m.get_enemy_spots()[i].Item4;
            int times_to_level_up = m.get_enemy_spots()[i].Item5;

            //Vector3 instPos = new Vector3(2 * transform_x(x_pos), 2 * transform_y(y_pos), 0f);
            Vector3 instPos = get_pos_from_coords(x_pos, y_pos);

            //cause unit to be shown.
            Enemy inst_u = Instantiate(m.get_enemy_spots()[i].Item1, instPos, transform.rotation);

            //tile.place_unit()
            myGrid[x_pos, y_pos].place_unit(inst_u);

            inst_u.start_of_mission(); //do start of mission setup
            inst_u.set_activation_delay(act_delay);
            inst_u.x = x_pos;
            inst_u.y = y_pos;
            inst_u.level_up(times_to_level_up);
            enemyUnits.Add(inst_u);
        }

    }
    void update_ZoC()
    {
        //recalculate zone of control every time the game enters selection mode.
        //(either player or enemy)

        //first, reset all tiles zones of control.
        foreach(Tile t in zocTiles)
        {
            t.player_controlled = false;
            t.enemy_controlled = false;
            t.set_ZoC_color();
        }
        zocTiles.Clear();

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
                    zocTiles.Add(myGrid[i, u.y]);
                }
            }
            //north and south tiles:
            for (int j = u.y - u.get_controlRange(); j < u.y + u.get_controlRange() + 1; j++)
            {
                //if the tile is on the grid, set tile.playerControlled to true.
                if (within_border(u.x, j))
                {
                    myGrid[u.x, j].player_controlled = true;
                    zocTiles.Add(myGrid[u.x, j]);
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
                    zocTiles.Add(myGrid[i, u.y]);
                }
            }
            //north and south tiles:
            for (int j = u.y - u.get_controlRange(); j < u.y + u.get_controlRange() + 1; j++)
            {
                //if the tile is on the grid, set tile.playerControlled to true.
                if (within_border(u.x, j))
                {
                    myGrid[u.x, j].enemy_controlled = true;
                    zocTiles.Add(myGrid[u.x, j]);
                }
            }
        }
        foreach (Tile t in zocTiles)
        {
            t.set_ZoC_color();
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
        briefingWinText.text = "Clear—" + loadedMission.print_objectives(true, true, playerUnits, enemyUnits, roundNumber, anyPlayerCasualties, baseList);
        briefingLossText.text = "Loss—" + loadedMission.get_lossDescr();
        briefingDisplay.gameObject.SetActive(true);
        
    }
    public void unhover_briefing_button()
    {
        briefingDisplay.gameObject.SetActive(false);
    }
    public void hover_help_button()
    {
        helpDisplay.gameObject.SetActive(true);
    }
    public void unhover_help_button()
    {
        helpDisplay.gameObject.SetActive(false);
    }
    void update_order()
    {
        //updates the order bg visuals with the information in active_order.
        orderTitleText.text = "ORDER\n" + active_order.get_orderName();
        orderdescrText.text = active_order.get_orderDescr();
    }
    public void hide_informers()
    {
        uInformer.hide();
        tInformer.hide();
    }
    void spawn_reinforcements((Enemy, int, int, int, int)[] reinforcements)
    {
        Debug.Log("cgrid.spawn_reinforcements() called");
        //deploy enemies too
        for (int i = 0; i < reinforcements.Length; i++)
        {
            int x_pos = reinforcements[i].Item2;
            int y_pos = reinforcements[i].Item3;
            int act_delay = reinforcements[i].Item4;
            int times_to_level_up = reinforcements[i].Item5;

            //only spawn the unit if the tile it would be spawned on is NOT occupied.
            if (!myGrid[x_pos, y_pos].occupied())
            {
                Vector3 instPos = get_pos_from_coords(x_pos, y_pos);
                //cause unit to be shown.
                Enemy inst_u = Instantiate(reinforcements[i].Item1, instPos, transform.rotation);

                //tile.place_unit()
                myGrid[x_pos, y_pos].place_unit(inst_u);

                inst_u.start_of_mission(); //do start of mission setup

                //make sure to set their AP to 0. No same turn reinforcements.
                inst_u.x = x_pos;
                inst_u.y = y_pos;
                inst_u.dec_ap();
                inst_u.set_activation_delay(act_delay);
                inst_u.level_up(times_to_level_up);
                //level the unit up however many times
                enemyUnits.Add(inst_u);
            }
        }
        update_ZoC();
    }

    //Clear/Loss
    public void show_mission_summary(bool isClear)
    {
        //displays the mission summary screen.
        //shows:
        // -mission clear/mission fail
        // -main objective; + _ exp
        // -side objectives; + _ exp (each)
        // -continue button / retry and main menu button
        overState = isClear;
        if (overState)
        {
            missionSummaryTitleText.text = "Mission Clear";
            continueRetryButton.gameObject.transform.GetChild(0).GetComponent<Text>().text = "Continue";
            mainMenuButton.gameObject.SetActive(false);
        }
        else
        {
            missionSummaryTitleText.text = "Mission Fail";
            continueRetryButton.gameObject.transform.GetChild(0).GetComponent<Text>().text = "Retry";
            mainMenuButton.gameObject.SetActive(true);
        }

        //for the objectives list:
        // [0]: main objective. success if isClear, fail if not. (get text from mission)
        // [i]: mission has got a list of bools it returns, where each bool is the state of whether that side objective is success.
        // (there is a maximum number of side objectives allowed.)

        objectivesSummaryText.text = loadedMission.print_objectives(overState, false, playerUnits, enemyUnits, roundNumber, anyPlayerCasualties, baseList);
        expSummaryText.text = loadedMission.print_objectives_rewards(overState, playerUnits, enemyUnits, roundNumber, anyPlayerCasualties, baseList);

        expSummaryText.text += "\n\n\n—Total: " + (loadedMission.get_objectives_exp(playerUnits, enemyUnits, roundNumber, anyPlayerCasualties, baseList) + Carrier.Instance.get_exp());

        uiCanvas.enabled = true;
        missionSummaryObj.gameObject.SetActive(true);
    }
    public void mission_summary_button_pressed_1()
    {
        //the continue/retry button is pressed.
        if (overState)
        {
            //continue; load next part
            Debug.Log("continue pressed; loading overworld");

            //load up all the exp from the objectives into the carrier
            Carrier.Instance.inc_exp(loadedMission.get_objectives_exp(playerUnits, enemyUnits, roundNumber, anyPlayerCasualties, baseList));

            StartCoroutine(pause_before_loading_scene(1));
        }
        else
        {
            //retry; reload mission
            Debug.Log("retry pressed; reloading combat");
            StartCoroutine(pause_before_loading_scene(2));
        }
    }
    public void mission_summary_button_pressed_2()
    {
        //the main menu button is pressed.
        StartCoroutine(pause_before_loading_scene(0));
    }
    IEnumerator pause_before_loading_scene(int sceneIndex)
    {
        fader.fade_to_black_stay();
        yield return new WaitForSeconds(2f);
        Debug.Log("loading scene " + sceneIndex + " | carrier: nextpartindex = " + Carrier.Instance.get_nextPartIndex() + "carrier nextmissionindex = " + Carrier.Instance.get_nextMissionIndex());
        SceneManager.LoadScene(sceneIndex);
    }

    //Control
    public void post_mission_begin_dialogue()
    {
        //called right after the begin mission dialogue event has ended.
        //the player still needs the chance to deploy their units.
        uiCanvas.enabled = true;
        enable_all_unitShortcuts();
        turn_order();       
        turnPatternMarker.enabled = false;
        StartCoroutine(post_begin_mission());
    }
    IEnumerator post_begin_mission()
    {
        //need a short delay before enabling next round stuff, because
        //otherwise the proceed spacebar will double count, and the game will skip deployment.
        yield return new WaitForSeconds(0.1f);
        nextRoundButton.interactable = true;
        animating = false;
    }
    IEnumerator start_event_after_pause(int which)
    {
        //Debug.Log("cgrid.start_event_after_pause() called with which = " + which);

        cam.lock_camera();
        animating = true;
        allowRoundEvent = false;
        uiCanvas.enabled = false;
        fader.fade_to_black(pause_before_event_start);
        yield return new WaitForSeconds(pause_before_event_start);

        //spawn reinforcements, if any: (only enemy can get reinforcements)
        (Enemy, int, int, int, int)[] reinforcements = loadedMission.get_enemy_reinforcements(which);
        if (reinforcements != null)
        {
            //spawn them.

            spawn_reinforcements(reinforcements);
        }

        //start event, (well, there has to be one.)
        cDia.play_event(loadedMission.get_script(), which);
    }
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
    public void next_turn()
    {       
        //handle mission over and start (+ their event playing)
        if (loadedMission.is_mission_won(playerUnits, enemyUnits, myGrid, roundNumber, baseList) || playerArrived /*&& allowRoundEvent*/)
        {   
            StartCoroutine(start_event_after_pause(-2));
            return;
        }
        else if (loadedMission.is_mission_lost(playerUnits, enemyUnits, myGrid, roundNumber, baseList) || enemyArrived /*&& allowRoundEvent*/)
        {
            StartCoroutine(start_event_after_pause(-3));
            return;
        }

        //if it's the start of the mission
        if (roundNumber == 0 && allowRoundEvent)
        {
            //play start of mission event:         
            cam.lock_camera();
            animating = true;
            allowRoundEvent = false;
            uiCanvas.enabled = false;
            cDia.play_event(loadedMission.get_script(), 0);           
            return;
        }

        //check if end of round
        if (is_end_of_round() || gameState == State.BETWEEN_ROUNDS)
        {
            animating = false;
            gameState = State.BETWEEN_ROUNDS;

            //we're at the start of a round.
            // -add to pw

            foreach (BaseTile b in baseList)
            {
                if (b.get_ownership() == BaseOwnership.PLAYER)
                {
                    pw = Math.Min(30, pw + active_order.order_power_base(b.get_pwGeneration()));
                }
            }
            pw = Math.Min(30, active_order.order_power_flat(pw)); 

            update_pw();
            if (is_deployment_possible())
            {
                enable_all_unitShortcuts();
            }
            
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
        //check if there is a start of round event to play:
        //Debug.Log("mission.has_event() = " + loadedMission.has_event(roundNumber) + " | roundNumber = " + roundNumber + " | allowRoundEvent = " + allowRoundEvent);
        if (loadedMission.has_event(roundNumber) && allowRoundEvent)
        {
            nextRoundButton.interactable = false;
            StartCoroutine(start_event_after_pause(roundNumber));
            return;
        }
        else
        {
            uiCanvas.enabled = true;
        }

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
        
        active_order = defaultOrder;
        set_order = true;
        update_order();
        roundNumber += 1;
        roundNumberText.text = "Round " + roundNumber;
        allowRoundEvent = true;
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
    public void click_pass_button()
    {
        //click this to choose to not make an attack.
        //tidies up and then passes to next turn.
        //can be called with spacebar.
        finish_attack(false, true);

    }  

    //Enemy Turn/AI
    void start_enemy_turn()
    {
        //Debug.Log("enemy turn starting");
        cam.lock_camera();
        animating = true;
        Unit enemyChosenUnit = select_enemy_unit();
        select_enemy_action(enemyChosenUnit);        
    }
    Unit select_enemy_unit()
    {
        int chosenIndex = -2;
        int runningMax = -1;
        for(int i = 0; i < enemyUnits.Count; i++)
        {
            if (enemyUnits[i].get_ap() > 0)
            {
                int score = enemyUnits[i].calculate_priority(last_player_end_turn_tile);
                if (runningMax == -1 || score > runningMax)
                {
                    //Debug.Log("choosing index " + i + " with a score of " + score);
                    runningMax = score;
                    chosenIndex = i;
                }
            }
        }

        //Debug.Log("chosenIndex is " + chosenIndex + " with a pri score of " + runningMax);
        active_unit = enemyUnits[chosenIndex];
        active_unit.reset_selection_variables();
        return enemyUnits[chosenIndex];
    }
    IEnumerator pass_enemy_turn(Unit chosenUnit)
    {
        //used to pass the enemy's turn
        
       // cam.unlock_camera();
        Vector3 moveHere = get_pos_from_coords(active_unit.x, active_unit.y) + new Vector3(0f, 0f, -10f);
        cam.jump_to(moveHere);
        //cam.lock_camera();

        yield return new WaitForSeconds(0.2f);

        end_enemy_turn(false, true);
    }
    void select_enemy_action(Unit chosenUnit)
    {
        //selects the best destination, trait, target location for chosenUnit.
        //if the unit's activation delay is > 0, though, then do nothing.
       
        if (chosenUnit.get_act_delay() > 0)
        {
            chosenUnit.dec_act_delay();
            StartCoroutine(pass_enemy_turn(chosenUnit));
            return;
        }

        highlight_tiles_mv(chosenUnit, false);
        //so we now have visited set up as a list of all possible destination tiles.
        //find the best (destination tile, trait attack, targetlist) triple.
        int runningMax = -1;

        //dest tile, action information index
        List<(Tile, int)> bestList = new List<(Tile, int)>();
        int counter = 0;
        foreach(Tile t in visited)
        {
            if (!t.isValid) continue;
            //find position of closest player unit.
            int score = chosenUnit.score_move(closestPlayerTileToTile(t), t, e_tilesAddedToZoC(chosenUnit.x, chosenUnit.y, chosenUnit), myGrid, visited);
            //Debug.Log("scoring dest: " + t.x + ", " + t.y + " | score = " + score);

            if (runningMax == -1)
            {
                runningMax = score;
                bestList.Add((t, counter));
            }
            else if (score > runningMax)
            {
                runningMax = score;
                bestList.Clear();
                chosenUnit.clear_moveInformationList_except_last();
                counter = 0;
                bestList.Add((t, counter));
            }
            else if (score == runningMax)
            {
                bestList.Add((t, counter));                
            }
            counter++;
        }
        //randomly select from bestList
        (Tile, int) best = bestList[UnityEngine.Random.Range(0, bestList.Count)];

        //int, tilelist, tile
        enemy_active_info = active_unit.get_action_information(best.Item2);
        
        execute_enemy_movement(chosenUnit, best.Item1);
    }
    void execute_enemy_movement(Unit unit, Tile destTile)
    {
        //first, we move the unit to the destTile

        //update enemy's position on the grid
        myGrid[unit.x, unit.y].remove_unit();
        myGrid[destTile.x, destTile.y].place_unit(unit);

        cam.unlock_camera();
        Vector3 moveHere = get_pos_from_coords(active_unit.x, active_unit.y) + new Vector3(0f, 0f, -10f);
        cam.jump_to(moveHere);
        cam.lock_camera();

        active_unit.x = destTile.x;
        active_unit.y = destTile.y;

        if (destTile is DefendTile) enemyArrived = true;
        
        StartCoroutine(move_obj_on_path(unit.gameObject, destTile.path, false));
    }
    void finish_enemy_movement()
    {
        //the enemy has now reached the dest tile.
        //cleanup movement assets, capture bases, and update ZoC.
        clear_highlights();
        myGrid[active_unit.x, active_unit.y].highlight_target_mv();
        if (myGrid[active_unit.x, active_unit.y] is BaseTile)
        {
            pastBaseState = myGrid[active_unit.x, active_unit.y].get_ownership();
            myGrid[active_unit.x, active_unit.y].set_ownerShip(BaseOwnership.ENEMY);
        }
        update_ZoC();

        //perform attack
        //check if we're going to be performing an attack:
        if ( enemy_active_info.Item1 >= 0 )
        {
            active_ability = active_unit.get_traitList()[enemy_active_info.Item1];
            highlight_attack(active_ability); //adds all the potential origins to visited
            List<Unit> affectedUnits = tileList_to_targetList(enemy_active_info.Item2);
            StartCoroutine(perform_enemy_attack_1(affectedUnits));
        }
        else
        {
            end_enemy_turn(false, true);
        }
        
    }
    IEnumerator perform_enemy_attack_1(List<Unit> affectedUnits)
    {
        //wait while all the possible origin spots are shown.
        yield return new WaitForSeconds(enemy_pause_before_attack);

        //hide possible origin spots and proceeed with the actual attack.
        yield return StartCoroutine(perform_attack_animation(affectedUnits, false));       
    }
    void end_enemy_turn(bool anyKills, bool isPassed)
    {
        //the attack has just been finished.
        clear_target_highlights();
        clear_highlights();
        myGrid[active_unit.x, active_unit.y].highlight_target_mv();
        enemy_active_info = (-2, null, null);
        //dec ap, and set to grey.
        active_unit.dec_ap();

        //clear remnants of highlighting
        myGrid[active_unit.x, active_unit.y].hide_target_icon();

        active_unit = null;
        active_ability = null;
        animating = false;
        //Debug.Log("enemy turn over.");
        next_turn();
        cam.unlock_camera();
    }

    //Enemy AI helper functions
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

    //Animations
    IEnumerator move_obj_on_path(GameObject obj, List<Tile> path, bool isPlayer)
    {
        //move the object in from each path tile to the next
        //actually, we will be doing a movement animation here.       
        
        //furthermore, if it's one of the player's units moving, then the player retains camera control.
        //if it's the enemy moving though, then we'll slide the camera to the destination tile as the unit moves.

        if (!isPlayer && path.Count > 1)
        {
            yield return new WaitForSeconds(enemy_pause_before_movement);
        }

        //camera slide at the same time, but only for enemies.
        //the player shouldn't lose control of the camera during their turn.
        if (!isPlayer)
        {
            Vector3 slide_dest = get_pos_from_coords(path[path.Count - 1].x, path[path.Count - 1].y) + new Vector3(0f, 0f, -10f);
            cam.slide_to(slide_dest, path[0].x, path[0].y, true, 20f);
        }       

        for (int i = 1; i < path.Count; i++)
        {
            float elapsedTime = 0f;
            while (elapsedTime < 2 * (1 / movement_animation_speed))
            {
                Vector3 dest = get_pos_from_coords(path[i].x, path[i].y);

                obj.transform.position = Vector3.MoveTowards(obj.transform.position, dest, Time.deltaTime * movement_animation_speed);

                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
        if (isPlayer)
        {
            finish_movement();
        }
        else
        {
            finish_enemy_movement();
        }
        
        yield return null;
    }
    void finish_movement()
    {
        //moves the unit_object from start_pos to end_pos over movement_duration seconds.
        //strictly follows the path laid out by end_pos.path

        //prepare for select target
        gameState = State.SELECT_TARGET;
        uInformer.set_pass(true);
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
        animating = false;

        //update uinformer visuals
        uInformer.fill(active_unit, pw, true);
    }
    IEnumerator perform_attack_animation(List<Unit> affectedUnits, bool isPlayer)
    {
        //here's what it is to do:
        // -plays move sound (or none if null)
        // -show health bars decreasing on all the affected units
        // -when animation done: any units that have died, remove their sprites.
        // -when that's done (and attack duration time has passed), hide affectedTiles

        //do all damage calculations. 
        // -each affected unit has had its hp updated, so now we adjust their hpbars in the animation segment
        //the move used could be an attack or it could be a heal.
        bool anyKills = false;
        int totalDmg = 0;
        if (active_ability.get_isHeal())
        {
            foreach (Unit target in affectedUnits)
            {
                int heal = brain.calc_heal(active_unit, target, active_ability, active_unit.get_isAlly(), active_order);
                target.take_heal(heal);
            }
        }
        else
        {            
            if (active_unit.get_isAlly())
            {              
                foreach (Unit target in affectedUnits)
                {
                    int dmg = brain.calc_damage(active_unit, target, active_ability, myGrid[target.x, target.y], active_unit.get_isAlly(), active_order, playerUnits);
                    totalDmg += dmg;
                    target.take_dmg(dmg, active_ability.get_brkMult());
                    
                }
            }
            else
            {
                foreach (Unit target in affectedUnits)
                {
                    int dmg = brain.calc_damage(active_unit, target, active_ability, myGrid[target.x, target.y], active_unit.get_isAlly(), active_order, enemyUnits.ToArray());
                    totalDmg += dmg;
                    target.take_dmg(dmg, active_ability.get_brkMult());                  
                }
            }

            //unit's on_attack trait effect is called. 
            active_ability.on_attack(active_unit, totalDmg);
        }

        //update uinformer (show the real hp of whatever unit was there)
        uInformer.refresh();

        //play move sound
        audio.play_sound(active_ability.get_traitSound());

        //do hpbar and brkbar animations
        float elapsedTime = 0f;
        while (elapsedTime < combat_hpBar_duration)
        {
            //for all affected units:
            //move hp bars from past to current hp percentage
            foreach (Unit u in affectedUnits)
            {
                float endScale = (float)u.get_hp() / (float)u.get_hpMax();
                u.slide_hpBar(endScale);

                float brk_endScale = (float)u.get_brk() / (float)u.get_brkMax();
                u.slide_brkBar(brk_endScale);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        yield return new WaitForSeconds(combat_hpBar_linger);

        //adjust states based on break and death
        foreach(Unit target in affectedUnits)
        {
            //if dead, remove from playerList/enemyList depending on isAlly.
            if (target.get_isDead())
            {
                anyKills = true;
                if (target.get_isAlly())
                {
                    anyPlayerCasualties = true;
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
                    //get exp from defeating enemy
                    Carrier.Instance.inc_exp(target.get_exp());
                    //remove from enemy list
                    enemyUnits.Remove(target);
                }
                //if target is the one being shown in the unit informer; then clear unit informer.
                if (uInformer.get_heldUnit() == target) uInformer.hide();

                myGrid[target.x, target.y].destroy_unit();
                myGrid[target.x, target.y].remove_unit();
                Destroy(target.gameObject);
            }
            //if a player unit has broken, then update its shortcut image
            else if (target.get_isAlly() && target.get_isBroken())
            {
                //grey out unit box
                for (int i = 0; i < playerUnits.Length; i++)
                {
                    if (target == playerUnits[i])
                    {
                        unit_shortcut_buttons[i].GetComponent<Image>().color = new Color(27f / 255f, 27f / 255f, 27f / 255f);
                    }
                }
            }
        }

        //active ability's on kill trait effect called
        if (anyKills)
        {
            active_ability.on_kill(active_unit, totalDmg);
            update_ZoC();
        }               

        yield return new WaitForSeconds(combat_highlights_linger);

        if (isPlayer)
        {
            finish_attack(anyKills, false);
        }
        else
        {
            end_enemy_turn(anyKills, false);
        }
        
        yield return null;       
    }
    void finish_attack(bool anyKills, bool isPassed)
    {
        //update the active order:
        if (active_unit != null && set_order == true)
        {
            active_order = active_unit.get_unitOrder();
            update_order();
            set_order = false;
        }
               
        //remove target icons
        clear_target_highlights();
        uInformer.set_pass(false);

        //handle power (and order interaction)
        int pw_cost;
        if (!isPassed)
        {
            //order influences power cost.
            pw_cost = active_order.order_power_cost(active_ability.get_pwCost());
        }
        else
        {
            pw_cost = active_order.order_power_cost(0);           
        }
        pw -= pw_cost;
        update_pw();

        //prepare for enemy phase
        if (active_unit != null)
        {
            last_player_end_turn_tile = myGrid[active_unit.x, active_unit.y];

            //grey out unit box
            active_unit.dec_ap();
            for (int i = 0; i < playerUnits.Length; i++)
            {
                if (active_unit == playerUnits[i])
                {
                    //grey out unit button.
                    unit_shortcut_buttons[i].GetComponent<Image>().color = new Color(27f / 255f, 27f / 255f, 27f / 255f);
                    break;
                }
            }

            active_unit = null;
        }

        active_ability = null;
        uInformer.set_heldUnit(null);
        clear_highlights();
        if (movementHighlightTile != null)
        {
            movementHighlightTile.hide_target_icon();
            movementHighlightTile = null;
        }
        gameState = State.ENEMY;
        animating = false;
        next_turn();
    }

    //Tile interactivity
    int sum_path_tile_cost(List<Tile> pathTaken, Unit u)
    {
        //for a list of tiles, returns the total mvCost it would take to traverse it.
        //takes active_unit's passives into account.
        int sum = 0;
        for (int j = 0; j < pathTaken.Count; j++)
        {
            List<int> mvCostList = new List<int>();
            for (int i = 0; i < u.get_traitList().Length; i++)
            {
                if (u.get_traitList()[i] != null && u.get_traitList()[i].get_isPassive())
                {
                    mvCostList.Add(u.get_traitList()[i].modify_movementCost(pathTaken[j]));
                }
            }
            if (mvCostList.Count > 0) sum += mvCostList.Min();
            else sum += pathTaken[j].get_movementCost();
        }
        //Debug.Log("sum path tile cost = " + sum + ", u mvmt = " + active_order.order_movement(u.get_movement()));
        return Math.Min(sum, active_order.order_movement(u.get_movement()));
    }
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
            if (active_unit.get_traitList()[which] != null)
            {
                if (active_unit.get_traitList()[which].get_isPassive() || active_unit.get_traitList()[which].get_pwCost() > pw)
                {
                    //hide highlights
                    clear_highlights();
                    clear_target_highlights();
                }
                else
                {
                    //show other valid move's highlights
                    clear_highlights();
                    active_ability = active_unit.get_traitList()[which];

                    //hide target highlights
                    clear_target_highlights();
                    highlight_attack(active_ability);
                }
            }
        }
        //if we are not in target select, then you can update the unit informer
        uInformer.traitButtonHover(which);

    }
    public void tile_clicked(int x_pos, int y_pos, Unit heldUnit)
    {
        if (animating) return;
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
                targetHighlightGroup.Add(myGrid[x_pos, y_pos]);

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

                //testing path:
                /*
                Debug.Log("path taken to " + myGrid[x_pos, y_pos].x + ", " + myGrid[x_pos, y_pos].y + ":");
                for(int i = 0; i < myGrid[x_pos, y_pos].path.Count; i++)
                {
                    Debug.Log("tile " + myGrid[x_pos, y_pos].path[i].x + ", " + myGrid[x_pos, y_pos].path[i].y);
                }
                */

                //move unit to this tile
                past_active_unit_x = active_unit.x;
                past_active_unit_y = active_unit.y;
                myGrid[active_unit.x, active_unit.y].remove_unit();
                myGrid[active_unit.x, active_unit.y].hide_target_icon();

                myGrid[x_pos, y_pos].place_unit(active_unit);
                active_unit.x = x_pos;
                active_unit.y = y_pos;

                if (myGrid[x_pos, y_pos] is ArriveTile) playerArrived = true;

                //actually, perform movement animation here.
                animating = true;
                StartCoroutine(move_obj_on_path(active_unit.gameObject, myGrid[x_pos, y_pos].path, true));
                break;

            case State.SELECT_TARGET:
                //Debug.Log("YOU HAVE CLICKED A TILE TO BE THE LOCATION OF A MOVE");
                //what's the process:
                // -generate the list of affected targets.
                // -for each target
                //  -apply damage/heal
                //  -check if dead
                // -finally, update ZoC and drain power.

                //at this time:
                //visited: all tiles you can choose to be the origin of the attack
                //targetHighlightGroup: all tiles that will be hit if you choose to attack for a given origin

                List<Unit> affectedUnits = tileList_to_targetList(generate_tileList(active_ability, x_pos, y_pos)); //used to damage/heal all affected units
                clear_highlights();

                animating = true;
                if ( affectedUnits.Count > 0)
                {
                    StartCoroutine(perform_attack_animation(affectedUnits, true));
                }
                else
                {
                    finish_attack(false, true);
                }
                break;
        }      
    }
    public void tile_hovered(int x_pos, int y_pos, Unit heldUnit)
    {
        if (animating) return;

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
                //to left
                for (int i = active_unit.x - 1; i > active_unit.x - t.get_range() - 1; i--)
                {
                    if (within_border(i, active_unit.y))
                    {
                        //if the tile is valid, then add to list you can hit
                        if (Math.Abs(active_unit.x - i) >= t.get_min_range())
                        {
                            visited.Add(myGrid[i, active_unit.y]);
                        }
                        //stop exploring if the tile blocks attacks, though.
                        if (!t.get_ignores_blocking_terrain() && myGrid[i, active_unit.y].get_blocksAttacks())
                        {
                            break;
                        }
                    }
                }
                //to right
                for (int i = active_unit.x + 1; i < active_unit.x + t.get_range() + 1; i++)
                {
                    if (within_border(i, active_unit.y))
                    {
                        //if the tile is valid, then add to list you can hit
                        if (Math.Abs(active_unit.x - i) >= t.get_min_range())
                        {
                            visited.Add(myGrid[i, active_unit.y]);
                        }
                        //stop exploring if the tile blocks attacks, though.
                        if (!t.get_ignores_blocking_terrain() && myGrid[i, active_unit.y].get_blocksAttacks())
                        {
                            break;
                        }
                    }
                }
                //to top
                for (int j = active_unit.y + 1; j < active_unit.y + t.get_range() + 1; j++)
                {
                    if (within_border(active_unit.x, j))
                    {
                        if (Math.Abs(active_unit.y - j) >= t.get_min_range())
                        {
                            visited.Add(myGrid[active_unit.x, j]);
                        }
                        if (!t.get_ignores_blocking_terrain() && myGrid[active_unit.x, j].get_blocksAttacks())
                        {
                            break;
                        }
                    }
                }
                //to bottom
                for (int j = active_unit.y - 1; j > active_unit.y - t.get_range() - 1; j--)
                {
                    if (within_border(active_unit.x, j))
                    {
                        if (Math.Abs(active_unit.y - j) >= t.get_min_range())
                        {
                            visited.Add(myGrid[active_unit.x, j]);
                        }
                        if (!t.get_ignores_blocking_terrain() && myGrid[active_unit.x, j].get_blocksAttacks())
                        {
                            break;
                        }
                    }
                }
                break;
            case TargetingType.SQUARE:               
                for (int i = active_unit.x - 1; i < active_unit.x + 1 + 1; i++)
                {
                    for (int j = active_unit.y - 1; j < active_unit.y + 1 + 1; j++)
                    {
                        //if the tile is on the grid, and is not the unit's tile.
                        if (within_border(i, j) && !(i == active_unit.x && j == active_unit.y) && Math.Abs(active_unit.x - i) + Math.Abs(active_unit.y - j) >= t.get_min_range())
                        {
                            visited.Add(myGrid[i, j]);
                        }
                    }
                }
                break;
            case TargetingType.RADIUS:
                //borrow dfs code.
                atk_dfs(visited, myGrid[active_unit.x, active_unit.y], t.get_range(), t.get_min_range(), true);
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
        List<Tile> tempPath = new List<Tile>();

        if (isPlayer)
        {
            dfs(visited, myGrid[u.x, u.y], active_order.order_movement(u.get_movement()), isPlayer, u, tempPath);
        }
        else
        {
            dfs(visited, myGrid[u.x, u.y], u.get_movement(), isPlayer, u, tempPath);
        }

        

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
    void dfs(HashSet<Tile> v, Tile start, int moveLeft, bool isPlayer, Unit u, List<Tile> pathTaken)
    {
        //start.path needs to be the path at this moment in time.
        pathTaken.Add(start);
        if (start.path == null)
            start.path = new List<Tile>();
        
        start.path.Clear();
        foreach (Tile t in pathTaken)
        {
            //if (!start.path.Contains(t))
            start.path.Add(t);
        }
        
        v.Add(start); //add to visited list
        
        if (moveLeft <= 0) return;
        
        List<Tile> adjacentTiles = new List<Tile>();
        //add tiles based on coordinates, as long as they are not out of bounds.

        //check 4 adjacent tiles loop
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                //reject all non-adjacent tiles.
                if (Math.Abs(i) + Math.Abs(j) != 1) continue;

                //if on the map
                if (within_border(start.x + i, start.y + j))
                {
                    if (myGrid[start.x + i, start.y + j].path == null) myGrid[start.x + i, start.y + j].path = new List<Tile>();
                    //overwrite tile's path IF: 
                    // -it has no path yet
                    // -OR, our current path is less than the path already saved to the tile
                    // -OR, our current path is the same cost as the path already saved in cost, but is shorter in length
                    if (!v.Contains(myGrid[start.x + i, start.y + j])
                        || (sum_path_tile_cost(pathTaken, u) < sum_path_tile_cost(myGrid[start.x + i, start.y + j].path, u))
                        || (sum_path_tile_cost(pathTaken, u) == sum_path_tile_cost(myGrid[start.x + i, start.y + j].path, u) && pathTaken.Count < myGrid[start.x + i, start.y + j].path.Count)
                        )
                    {
                        adjacentTiles.Add(myGrid[start.x + i, start.y + j]);
                    }
                }
            }
        }

        //perform dfs on each accepted tile
        foreach (Tile next in adjacentTiles)
        {
            //if movement cost is less than 0, then the tile is impassable.
            //each number below 0 corresponds to some kind of impassable terrain.
            // -1: reinforcement terrain. impassable to everyone.
            // -2: water

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
            //Of course, traits can modify mvCost making certain terrain passable for a unit.

            //if (mvCost > 0) //<-- you can pay what you have left for the last tile.
            //heavy: must pay full cost for every tile.
            //light and medium: can pay what you have for last tile.
            //we check mvCost > 0 to see if tile is impassable.
            if (mvCost > 0 && ((u.get_aff() == 2 && moveLeft >= mvCost) || ( u.get_aff() <= 1 && mvCost > 0))) 
            {
                //if tile is in the opponent's ZoC, then it costs all remaining movement.
                if (isPlayer && next.enemy_controlled)
                {
                    dfs(v, next, 0, isPlayer, u, pathTaken);
                }
                else if (!isPlayer && next.player_controlled)
                {
                    dfs(v, next, 0, isPlayer, u, pathTaken);
                }
                else
                {
                    dfs(v, next, moveLeft - mvCost, isPlayer, u, pathTaken);
                }

                //hmm... maybe instead of just removing the latest element, we need to remove as many elements as it went deep.
                //No: each one gets removed in turn.
                pathTaken.RemoveAt(pathTaken.Count - 1);
            }

        }
    }
    void atk_dfs(HashSet<Tile> v, Tile start, int rangeLeft, int min_range, bool originTile)
    {
        //only add if allowed by minimum range
        //i.e. if distance between the tiles of myGrid[active_unit.x, active_unit.y] and start is >= min_range
        if ( Math.Abs(active_unit.x - start.x) + Math.Abs(active_unit.y - start.y) >= min_range )
        {
            v.Add(start);
        }

        if (!originTile && !active_ability.get_ignores_blocking_terrain() && start.get_blocksAttacks()) return;

        if (rangeLeft == 0) return;

        List<Tile> adjacentTiles = new List<Tile>();
        //add tiles based on coordinates, as long as they are not out of bounds.

        if (within_border(start.x + 1, start.y) && !v.Contains(myGrid[start.x + 1, start.y])) adjacentTiles.Add(myGrid[start.x + 1, start.y]);
        if (within_border(start.x - 1, start.y) && !v.Contains(myGrid[start.x - 1, start.y])) adjacentTiles.Add(myGrid[start.x - 1, start.y]);
        if (within_border(start.x, start.y + 1) && !v.Contains(myGrid[start.x, start.y + 1])) adjacentTiles.Add(myGrid[start.x, start.y + 1]);
        if (within_border(start.x, start.y - 1) && !v.Contains(myGrid[start.x, start.y - 1])) adjacentTiles.Add(myGrid[start.x, start.y - 1]);

        foreach (Tile next in adjacentTiles)
        {           
            atk_dfs(v, next, rangeLeft - 1, min_range, false);           
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
        if (which >= reserveParty.Count) return;
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
            if (gameState != State.ENEMY)
            {
                Vector3 moveHere = get_pos_from_coords(playerUnits[which].x, playerUnits[which].y) + new Vector3(0f, 0f, -10f);
                cam.jump_to(moveHere);
            }
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
            for (int i = 0; i < reserveParty.Count; i++)
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
            for (int i = reserveParty.Count; i < 18; i++)
            {
                deploymentObj.gameObject.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < reserveParty.Count; i++)
            {
                GameObject obj = deploymentObj.gameObject.transform.GetChild(i).gameObject;
                obj.transform.GetChild(0).GetComponent<Image>().sprite = reserveParty[i].get_box_p();
                obj.transform.GetChild(1).GetComponent<Text>().text = reserveParty[i].get_unitName() + " | " + reserveParty[i].get_pwCost() + " PW";
                obj.GetComponent<Button>().interactable = false;
                
            }
            for (int i = reserveParty.Count; i < 18; i++)
            {
                deploymentObj.gameObject.transform.GetChild(i).gameObject.SetActive(false);
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
        //i.e. true if at least one base is player owned and not occupied and not all 8 units are deployed
        foreach(BaseTile b in baseList)
        {
            if (b.get_ownership() == BaseOwnership.PLAYER && !b.occupied() && playerUnits.Contains(null))
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
        for (int i = 0; i < reserveParty.Count; i++)
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

    //Audio
    public void play_music_track(int which)
    {
        //which corresponds to an index in mission's musicList
        audio.play_music(loadedMission.get_track(which));
    }
    public void play_sound(int which)
    {
        //audio.play_music(loadedMission.get_sound(which));
    }
    public void play_typing()
    {
        //plays the typing sound
        audio.play_typingSound();
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
        //return a list of tiles, which will all be hit.

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