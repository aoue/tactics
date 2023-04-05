using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelTreeMove : MonoBehaviour
{
    // Holds information for the level tree of each unit.
    // each move button in the level tree will have one of these.
    // necessary information:
    // -button, for being toggled
    // -attached move, that can be equipped
    // -cost, to learn the move
    // -stat increases, that occur when the move is learned

    [SerializeField] private Trait attachedMove; // we borrow the move's name and descr.
    [SerializeField] private int learnExpCost;

    // increases the following stats
    [SerializeField] private int hp_inc; // hpmax
    [SerializeField] private int brk_inc; // brkmax
    [SerializeField] private int pa_inc; // phys atk
    [SerializeField] private float pd_inc; // phys def
    [SerializeField] private int ma_inc; // mag atk
    [SerializeField] private float md_inc; // mag def
    
    // called when the move is learned, permanently increasing unit stats.
    public void learn_increases(Unit u)
    {
        // takes in the unit, and increases their stats in a way
        u.set_hpMax(u.get_hpMax() + hp_inc);
        u.set_brkMax(u.get_brkMax() + brk_inc);
        u.set_physa(u.get_physa() + pa_inc);
        u.set_physd(u.get_physd() + pd_inc);
        u.set_maga(u.get_maga() + ma_inc);
        u.set_magd(u.get_magd() + md_inc);
    }
    public string get_increases_string()
    {
        // returns a six line string that details the increases to each stat.
        return "+" + hp_inc
            + "\n+" + hp_inc
            + "\n+" + pa_inc
            + "\n+" + (pd_inc * 100f)
            + "%\n+" + ma_inc
            + "\n+" + (md_inc * 100f) + "%";
    }

    public Trait get_attached_move(){ return attachedMove; }
    public int get_learnExpCost() { return learnExpCost; }
}
