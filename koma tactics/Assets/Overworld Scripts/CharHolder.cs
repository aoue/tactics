using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharHolder : MonoBehaviour
{
    // holds all events for a particular character.
    //called at the start of a day. it keeps track of progression and will check if the next
    //event in sequence has had its requirements cleared. if it has, then it will show the event.
    //otherwise, it does nothing.
    //a single char's events cannot be done do days in a row, there is a minimum of a one day break.

    [SerializeField] private EventHolder[] charEventsList;
    public bool didEventYesterday { get; set; }
    public int progress { get; set; }

    public void prepare(Overworld theWorld)
    {
        if (didEventYesterday == true)
        {
            didEventYesterday = false;
            return;
        }

        if (progress > charEventsList.Length) return;

        //don't check all events, only the next one as marked by progress.
        //add that one to active events too.
        theWorld.add_active_event(charEventsList[progress], charEventsList[progress].get_id());

        //Debug.Log("preparing day, progress = " + progress);
        if (charEventsList[progress].validate_event() == true)
        {
            charEventsList[progress].setup_event();
        }
        else
        {
            charEventsList[progress].gameObject.SetActive(false);
        }
    }

    public void banish_day()
    {
        //called the day after this day has passed.
        //hides all its days that may or may not be gone.

        if ( charEventsList[progress] != null)
        {
            charEventsList[progress].gameObject.SetActive(false);
        }

    }


}
