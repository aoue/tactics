using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayByPlay : MonoBehaviour
{
    //a play by play window somewhere which displays:
    //what elements to display this information with?
    // when unit has been selected: [UNIT] activated
    // when dest tile has been clicked and movement completed: Moved to [TILE TYPE]
    // when a trait is highlighted, show damage roll: [range] + [ATK/HAC] - DEF/ICE <-not the value of an enemy, just select the corresponding value.
    // when target tile has been clicked, update finally: Finished.

    // also the enemy unit does this too.

    [SerializeField] private Text titleText; // display the current action that the player has to do next: SELECT, MOVE, PICK TARGET, COMPLETED
    [SerializeField] private Text displayText; //displays all the information given out by the play by play.

    public void fill(State gameState, Unit u, Trait t)
    {
        //fill up to a certain point depending on game state.
        //enum State { SELECT_UNIT, SELECT_MOVEMENT, SELECT_TARGET, ENEMY, BETWEEN_ROUNDS }

        switch(gameState)
        {
            case State.BETWEEN_ROUNDS:
                titleText.text = "START_ROUND";
                displayText.text = "";
                break;
            case State.ENEMY:
                titleText.text = "ENEMY PHASE";
                displayText.text = "";
                break;
            case State.SELECT_UNIT:
                titleText.text = "SELECT_UNIT";
                displayText.text = "";
                break;
            case State.SELECT_MOVEMENT:
                titleText.text = "SELECT_MOVEMENT";
                displayText.text = "";
                break;
            case State.SELECT_TARGET:
                titleText.text = "SELECT_TARGET";
                displayText.text = "";
                break;
        }
    }

}
