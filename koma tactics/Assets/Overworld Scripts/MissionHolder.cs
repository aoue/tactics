using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionHolder : EventHolder
{
    //an event whose purpose is to load a combat event when clicked.

    [SerializeField] private int combatMissionIndex; //the index in the mission list that the part will load.


    public override void begin_event()
    {
        //when the button is clicked. 

        //disable input
        gameObject.GetComponent<Button>().interactable = false;

        //Start the event.
        gameObject.transform.parent.GetComponent<Part>().pass_combat_to_overworld(combatMissionIndex);

        //hide yourself
        gameObject.SetActive(false);
    }


}
