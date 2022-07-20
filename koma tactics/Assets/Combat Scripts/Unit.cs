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

    //ui
    [SerializeField] private TextMeshPro hpText;

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


    //traitList[0] is locked, and is the unit's default attack ability. VITALLY IMPORTANT.
    [SerializeField] private Trait[] traitList; //ability/trait list. Passives and non-passives, together.
    private List<UnitType> unitTypes; //given by traits. Things like flying, aquatic, etc... Only influences bonus dmg taken, e.g. anti-air does bonus vs. flying units.


    public void dec_ap() { ap -= 1; }

    //adjust unit status
    public void start_of_mission()
    {
        //reset unit state at the start of a mission.
        isDead = false;
        hp = hpMax;
        brk = brkMax;
        ap = 1;
        update_hpText();

        //setup types array
        unitTypes = new List<UnitType>();
        foreach(Trait t in traitList)
        {
            if (t != null && t.get_unitType() != null)
            {
                unitTypes.Add(t.get_unitType());
            }
        }
    }
    public void take_dmg(int dmg, bool hittingYourself)
    {
        //causes dmg to the unit.
        //Any dmg is dealt to both hp and brk.
        //if brk reaches 0, then the unit breaks. (ap = 0)
        if (hittingYourself) hp = Mathf.Max(1, hp - dmg);
        else hp = Mathf.Max(0, hp - dmg);

        brk -= dmg;

        if (brk <= 0)
        {
            brk = brkMax;
            ap = 0;
        }
        if (hp == 0)
        {
            isDead = true;
        }
        else
        {
            update_hpText();
        }
        
    }
    public void take_heal(int heal)
    {
        hp = Mathf.Min(hpMax, hp + heal);
        update_hpText();
    }
    void update_hpText()
    {
        hpText.text = hp + "HP";
    }

    //getters
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
    public bool get_isDead() { return isDead; }
    public Trait[] get_traitList() { return traitList; }

}
