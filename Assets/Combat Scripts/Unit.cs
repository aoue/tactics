using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public enum moveLearnState { UNKNOWN, CAN_LEARN, LEARNED, CANNOT_LEARN }
public class Unit : MonoBehaviour
{
    //map
    public int x { get; set; }
    public int y { get; set; }

    //ui
    [SerializeField] private SpriteRenderer unitSprite;
    [SerializeField] private Transform hpBar; //marks hp / hpmax percentage, in red
    [SerializeField] private Transform brkBar; //marks brk / brkmax percentage, in yellow

    //stats  
    [SerializeField] private Sprite box_portrait; //the small one.
    [SerializeField] private Sprite active_portrait; //the large one.
    [SerializeField] private string unitName;
    [SerializeField] private int level;
    [SerializeField] private int exp; //for player units; it's the unspent exp they have. For enemy units, it's the exp they drop on kill.
    [SerializeField] private int aff; //0: light, 1: medium, 2: heavy
    [SerializeField] private int movement;
    [SerializeField] private int hpMax;
    private int hp;
    [SerializeField] private int brkMax; //starts at a high number. When brk reaches 0, the unit breaks.
    private int brk;
    private int ap;

    [SerializeField] private int controlRangeBase; //tiles outward that their ZoC extends in each direction. Real value. 
    private int controlRange; //tiles outward that their ZoC extends in each direction. Set to 0 when unit broken.
    [SerializeField] private int phys_atk;
    [SerializeField] private int phys_def;
    [SerializeField] private int magic_atk;
    [SerializeField] private int magic_def;

    //state
    private bool hasMoved; //true if the unit has moved (like, over tiles) this turn, false otherwise.
    private bool isDead;
    private bool isBroken;
    private bool lockHpSlider; //stops hp sliders from going back and forth.
    private bool lockBrkSlider; //stops hp sliders from going back and forth.

    [SerializeField] private Order unitOrder;

    private moveLearnState[] learnedList; // holds the state of all moves, whether they're learned or not.

    //traitList[0] is locked, and is the unit's default attack ability. VITALLY IMPORTANT.
    [SerializeField] private Trait[] traitList; //ability/trait list. Passives and non-passives, together.
    private List<UnitType> unitTypes; //given by traits. Things like flying, aquatic, etc... Only influences bonus dmg taken, e.g. anti-air does bonus vs. flying units.

    //adjust unit status
    public void dec_ap()
    {
        //Debug.Log("unit ap = 0");
        ap = 0;

        //grey out actual sprite
        unitSprite.color = new Color(27f / 255f, 27f / 255f, 27f / 255f);
    }
    void brk_self()
    {
        //called when the unit breaks.
        isBroken = true;      
        ap = 0;
        controlRange = 0;
        unitSprite.color = new Color(27f / 255f, 27f / 255f, 27f / 255f);
    }
    public void start_of_mission()
    {
        //reset unit state at the start of a mission.
        unlock_sliders();
        isDead = false;
        hp = hpMax;
        brk = brkMax;
        controlRange = controlRangeBase;
        ap = 1;
        set_hpBar();
        set_brkBar();

        //setup types array
        unitTypes = new List<UnitType>();
        foreach(Trait t in traitList)
        {
            if (t != null)
            {
                //add to unit's trait list
                if (t.get_unitType() != UnitType.NOTHING)
                {
                    unitTypes.Add(t.get_unitType());
                }
            }
        }
    }
    public void refresh(bool onBase)
    {
        //called at the start of a round.
        //restore brk if broken
        //restore 10% of maxhp if unit is on a base.
        hasMoved = false;
        ap = 1;
        if (brk == 0)
        {
            isBroken = false;
            brk = brkMax;
            controlRange = controlRangeBase;
            set_brkBar();
            unlock_sliders();
        }
        if (onBase)
        {
            take_heal((int)(hpMax * 0.1));
        }
        set_hpBar();
        unitSprite.color = new Color(1f, 1f, 1f);
    }
    public void set_hasMoved(bool value)
    {
        hasMoved = value;
    }
    public void take_dmg(int dmg, int brkOffset)
    {
        //causes dmg to the unit.
        //Any dmg is dealt to both hp and brk.
        //if brk reaches 0, then the unit breaks. (ap = 0)
        cancel_act_delay();
        hp = Mathf.Max(0, hp - dmg);       

        if (!isBroken)
        {
            brk = Math.Max(0, brk - (dmg + brkOffset));
            if (brk == 0)
            {
                brk_self();
            }
        }

        if (hp == 0)
        {
            foreach (Trait t in traitList)
            {
                if (t != null)
                {
                    t.on_own_death(this);
                }              
            }

            if (hp == 0)
            {
                isDead = true;
            }               
        }
    }
    public void take_heal(int heal)
    {
        hp = Mathf.Min(hpMax, hp + heal);
    }
    void set_hpBar()
    {
        //updates the hpbar:
        //set the scale of the sprite to hp / hpmax
        hpBar.localScale = new Vector3((float)hp / (float)hpMax, 0.4f);
    }
    void set_brkBar()
    {
        //updates the hpbar:
        //set the scale of the sprite to hp / hpmax
        brkBar.localScale = new Vector3((float)brk / (float)brkMax, 0.4f);
    }
    public void unlock_sliders()
    {
        lockHpSlider = false;
        lockBrkSlider = false;
    }
    public void slide_hpBar(float hpBarScale)
    {
        if (lockHpSlider) return;

        //slides the hpBar scale towards hpBarScale by 1 unit.    
        if (hpBar.localScale.x > hpBarScale)
        {
            hpBar.localScale += new Vector3(-0.01f, 0f);
        }
        else
        {
            hpBar.localScale += new Vector3(0.01f, 0f);
        }

        if (Math.Abs(hpBar.localScale.x - hpBarScale) < 0.001f)
        {
            lockHpSlider = true;
        }
    }
    public void slide_brkBar(float brkBarScale)
    {
        if (lockBrkSlider) return;

        //slides the hpBar scale towards hpBarScale by 1 unit.      
        if (brkBar.localScale.x > brkBarScale)
        {
            brkBar.localScale += new Vector3(-0.01f, 0f);
        }
        else
        {
            brkBar.localScale += new Vector3(0.01f, 0f);
        }

        if (Math.Abs(brkBar.localScale.x - brkBarScale) < 0.001f)
        {
            lockBrkSlider = true;
        }
    }

    //getters
    public float get_hpPercentage() { return (float)hp / (float)hpMax; }
    public Sprite get_box_p() { return box_portrait; }
    public Sprite get_active_p() { return active_portrait; }
    public string get_unitName() { return unitName; }
    public virtual bool get_isAlly() { return true; }
    public int get_aff() { return aff; }
    public int get_movement() { return movement; }
    public int get_hpMax() { return hpMax; }
    public int get_hp() { return hp; }
    public int get_brkMax() { return brkMax; }
    public int get_brk() { return brk; }
    public int get_ap() { return ap; }
    public int get_controlRange() { return controlRange; }
    public int get_level() { return level; }
    public int get_exp() { return exp; }
    public int get_physa() { return phys_atk; }
    public int get_physd() { return phys_def; }
    public int get_maga() { return magic_atk; }
    public int get_magd() { return magic_def; }
    public bool get_hasMoved() {return hasMoved; }
    public bool get_isDead() { return isDead; }
    public bool get_isBroken() { return isBroken; }
    public Trait[] get_traitList() { return traitList; }
    public Order get_unitOrder() { return unitOrder; }
    public moveLearnState[] get_learnedList() { return learnedList; }
    public List<UnitType> get_unitTypes() { return unitTypes; }

    //setters, for leveling up
    public void set_exp(int x) { exp = x; }
    public void set_hpMax(int x) { hpMax = x; }
    public void set_brkMax(int x) { brkMax = x; }
    public void set_physa(int x) { phys_atk = x; }
    public void set_physd(int x) { phys_def = x; }
    public void set_maga(int x) { magic_atk = x; }
    public void set_magd(int x) { magic_def = x; }
    public void init_learnList() { learnedList = new moveLearnState[24]; }

    //virtuals (for enemy AI)
    public virtual void clear_moveInformationList_except_last() { }
    public virtual void reset_selection_variables() { }
    public virtual int calculate_priority(Tile relevantTile) { return -1; }
    public virtual int score_move(int closestPlayerTile, Tile dest, int tilesAddedToZoC, Tile[,] myGrid, HashSet<Tile> visited, GridHelper gridHelper) { return -1; }
    public virtual int score_attack(Trait t, List<Tile> targetList, BattleBrain brain) { return -1; }
    public virtual (int, List<Tile>, Tile) get_action_information(int actionIndex) { return (-1, null, null); }
    public virtual void cancel_act_delay() { }
    public virtual int get_act_delay() { return 0; }
    public virtual void dec_act_delay() { }
    public virtual void level_up(int times) { }
    public virtual void set_sleepEffect() { }

}
