using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    //map
    public int x { get; set; }
    public int y { get; set; }

    //stats
    [SerializeField] private Sprite box_portrait; //the small one.
    [SerializeField] private Sprite active_portrait;  //the large one.
    [SerializeField] private string unitName;
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

    //movement
    public virtual int calc_movementCost(Tile t)
    {
        //Now, a tile may have a movecost, but the unit may have traits 
        //that lower that cost. Such as aquatic or flight, etc.
        return t.get_movementCost();
    }
    public void dec_ap() { ap -= 1; }

    //adjust unit status
    public void start_of_mission()
    {
        //reset unit state at the start of a mission.
        hp = hpMax;
        brk = brkMax;
        ap = 1;
    }
    public void take_dmg(int dmg)
    {
        //causes dmg to the unit.
        //Any dmg is dealt to both hp and brk.
        //if brk reaches 0, then the unit breaks. (ap = 0)
        hp -= dmg;
        brk -= dmg;

        if (brk <= 0)
        {
            brk = brkMax;
            ap = 0;
        }
        //update hpText
    }

    //getters
    public Sprite get_box_p() { return box_portrait; }
    public Sprite get_active_p() { return active_portrait; }
    public string get_unitName() { return unitName; }
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

}
