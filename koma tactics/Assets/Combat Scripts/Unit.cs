using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class Unit : MonoBehaviour
{
    //map
    public int x { get; set; }
    public int y { get; set; }

    [SerializeField] private int uniqueUnitID; //every player unit has a unique unit id.

    //ui
    [SerializeField] private SpriteRenderer unitSprite;
    [SerializeField] private Transform hpBar;

    //stats  
    [SerializeField] private Sprite box_portrait; //the small one.
    [SerializeField] private Sprite active_portrait;  //the large one.
    [SerializeField] private string unitName;
    [SerializeField] private bool isAlly; //true if player unit. False is not.
    [SerializeField] private int aff; //0: light, 1: medium, 2: heavy
    [SerializeField] private int movement;
    [SerializeField] private int hpMax;
    [SerializeField] private int hp;
    [SerializeField] private int brkMax; //starts at a high number. When brk reaches 0, the unit breaks.
    [SerializeField] private int brk;
    [SerializeField] private int ap;

    [SerializeField] private int controlRange; //tiles outward that their ZoC extends in each direction.
    [SerializeField] private int pwCost; //deploy cost
    [SerializeField] private int phys_atk;
    [SerializeField] private int phys_def;
    [SerializeField] private int magic_atk;
    [SerializeField] private int magic_def;

    //state
    private bool isDead;
    private bool isBroken;
    private bool deployed; //stops player from redeploying units after they die.


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
        brk = brkMax;
        ap = 0;
        unitSprite.color = new Color(27f / 255f, 27f / 255f, 27f / 255f);
    }
    public void recall(int h)
    {
        //adjust unit's stats when recalled.
        // set hp to what it was at.
        // reset brk though.
        hp = h;
        brk = brkMax;
        deployed = false;
    }
    public void set_deployed(bool d) { deployed = d; }
    public void start_of_mission()
    {
        //reset unit state at the start of a mission.
        deployed = true;
        isDead = false;
        hp = hpMax;
        brk = brkMax;
        ap = 1;
        set_hpBar();

        //setup types array
        unitTypes = new List<UnitType>();
        foreach(Trait t in traitList)
        {
            if (t != null && t.get_unitType() != UnitType.NOTHING)
            {
                unitTypes.Add(t.get_unitType());
            }
        }
    }
    public void refresh()
    {
        //called at the start of a round.
        ap = 1;
        unitSprite.color = new Color(1f, 1f, 1f);
    }
    public void take_dmg(int dmg)
    {
        //causes dmg to the unit.
        //Any dmg is dealt to both hp and brk.
        //if brk reaches 0, then the unit breaks. (ap = 0)
        hp = Mathf.Max(0, hp - dmg);

        if (!isBroken)
        {
            brk -= dmg;
            if (brk <= 0)
            {
                brk_self();
            }
        }

        if (hp == 0)
        {
            isDead = true;
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
        hpBar.localScale = new Vector3((float)hp / (float)hpMax, 1f);
    }
    public void slide_hpBar(float hpBarScale)
    {
        //slides the hpBar scale towards hpBarScale by 1 unit.
        if (hpBar.localScale.x > hpBarScale)
        {
            hpBar.localScale += new Vector3(-0.01f, 0f);
        }
        else if (hpBar.localScale.x < hpBarScale)
        {
            hpBar.localScale += new Vector3(0.01f, 0f);
        }
    }
    
    //getters
    public float get_hpPercentage() { return (float)hp / (float)hpMax; }
    public int get_uniqueUnitID() { return uniqueUnitID; }
    public Sprite get_box_p() { return box_portrait; }
    public Sprite get_active_p() { return active_portrait; }
    public string get_unitName() { return unitName; }
    public bool get_isAlly() { return isAlly; }
    public int get_aff() { return aff; }
    public int get_movement() { return movement; }
    public int get_hpMax() { return hpMax; }
    public int get_hp() { return hp; }
    public int get_brkMax() { return brkMax; }
    public int get_brk() { return brk; }
    public int get_ap() { return ap; }
    public int get_controlRange() { return controlRange; }
    public int get_pwCost() { return pwCost; }
    public int get_physa() { return phys_atk; }
    public int get_physd() { return phys_def; }
    public int get_maga() { return magic_atk; }
    public int get_magd() { return magic_def; }
    public bool get_isDeployed() { return deployed; }
    public bool get_isDead() { return isDead; }
    public bool get_isBroken() { return isBroken; }
    public Trait[] get_traitList() { return traitList; }

    //virtuals (for enemy AI)
    public virtual int calculate_priority() { return -1; }
    public virtual int score_move(int closestPlayerTile, Tile dest, int tilesAddedToZoC, Tile[,] myGrid, HashSet<Tile> visited) { return -1; }
    public virtual int score_attack(Trait t, List<Tile> targetList, BattleBrain brain) { return -1; }
    public virtual List<Tile> get_bestTileList() { return null; }
    public virtual int get_bestTraitIndex() { return -2; }
    public virtual Tile get_bestAttackOrigin() { return null; }
}
