using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NextPartEvent : EventHolder
{
    //an event whose purpose is to load the specified part when clicked.
    //it's the only other way to get to a new part without going through a combat mission.

    [SerializeField] private int nextPartIndex;

    public override void begin_event()
    {
        //when the button is clicked. 

        //disable input
        gameObject.GetComponent<Button>().interactable = false;

        //Start the event.
        gameObject.transform.parent.GetComponent<Part>().pass_part_to_overworld(nextPartIndex);

        //hide yourself
        gameObject.SetActive(false);
    }



}
