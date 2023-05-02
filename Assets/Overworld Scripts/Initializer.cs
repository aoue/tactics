using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Initializer : MonoBehaviour
{
    //used to set things up on newgame.
    //

    [SerializeField] private Trait[] anseStarting;
    [SerializeField] private Trait[] fridayStarting;

    public void reset_units()
    {
        //resets all units to their new game states.
        reset_anse();
        reset_friday();
    }

    // INDIVIDUAL UNIT RESETS
    // (starting list logic: starting list must have 3 moves, followed by 2 null)
    // in starting list, the non-null moves are LEARNED
    // the next two moves are CAN_LEARN
    // all the rest of the moves are UNKNOWN
    void reset_anse()
    {
        Unit u = Carrier.Instance.get_allUnitList()[0];
        u.set_hpMax(100);
        u.set_brkMax(55);
        u.set_physa(30);
        u.set_physd(1.0);
        u.set_maga(30);
        u.set_magd(1.0);
        u.set_exp(0);
        u.init_learnList();
        for(int i = 0; i < u.get_traitList().Length; i++)
        {
            u.get_traitList()[i] = anseStarting[i];
            if (i > 0 && i < 3) u.get_learnedList()[i-1] = moveLearnState.LEARNED;
            else if (i >= 3) u.get_learnedList()[i-1] = moveLearnState.CAN_LEARN;
        }
        for(int i = u.get_traitList().Length; i < u.get_learnedList().Length; i++)
        {
            u.get_learnedList()[i] = moveLearnState.UNKNOWN;
        }
    }

    void reset_friday()
    {
        Unit u = Carrier.Instance.get_allUnitList()[1];
        u.set_hpMax(90);
        u.set_brkMax(50);
        u.set_physa(40);
        u.set_physd(1.0);
        u.set_maga(5);
        u.set_magd(1.0);
        u.set_exp(0);
        u.init_learnList();
        for(int i = 0; i < u.get_traitList().Length; i++)
        {
            u.get_traitList()[i] = fridayStarting[i];
            if (i > 0 && i < 3) u.get_learnedList()[i-1] = moveLearnState.LEARNED;
            else if (i >= 3) u.get_learnedList()[i-1] = moveLearnState.CAN_LEARN;
        }
        for(int i = u.get_traitList().Length; i < u.get_learnedList().Length; i++)
        {
            u.get_learnedList()[i] = moveLearnState.UNKNOWN;
        }
    }


}
