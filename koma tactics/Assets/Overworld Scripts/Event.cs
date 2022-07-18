using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum eventType { RED, BLUE, GREEN }
public class Event : MonoBehaviour
{
    //an event.
    //has many child classes for more specific kinds of events.

    //event metadata
    [SerializeField] private int eventID; //day-unique identifier for this event.
    [SerializeField] private int dayProgressionNeeded; //day progression must be at least this much for the vent to be enabled.
    [SerializeField] private bool hasHaich; //controls whether a heart icon appears with the event.
    [SerializeField] private bool hasFight; //controls whether a sword crossing icon appears with the event.
    
    [SerializeField] private eventType event_type;

    //replacing these three with an Ink story that will do all these as it wishes.
    //external functions will be linked in EventManager, which will also run it.
    [SerializeField] private TextAsset inkJSONAsset;

    //post event and preview
    [SerializeField] private bool useNotifierOnEnd; //if true, we will use the notifier at the end of the event. if false, we will just return to overworld directly.
    [SerializeField] private string eventPreviewLines; //three lines shown in the event preview.
    [SerializeField] private string noteTitle; //notifier title title
    [SerializeField] private string noteBody; //notifier body
    [SerializeField] private string noteSummary; //notifier summary
    [SerializeField] private int[] portraitPreviews; //the cast preview shown in tooltip preview. size <= 4.

    //getters
    public string get_eventPreviewLines() { return eventPreviewLines; }
    public bool get_hasHaich() { return hasHaich; }
    public string get_noteTitle() { return noteTitle; }
    public int[] get_portraitPreviews() { return portraitPreviews; }
    public int get_id() { return eventID; }
    public TextAsset get_story() { return inkJSONAsset; }
    public eventType get_event_type() { return event_type; }

    public virtual bool isEventValid()
    {
        //checks whether event is valid based on History.
        //return true if it is, or false otherwise.

        //default is to return true, only override if there are conditions.
        //default is to check the event's type against the number of charges remaining:

        if (Overworld.dayProgression < dayProgressionNeeded) return false;

        return true;
    }

    public void post_event_message(Notifier notifier)
    {
        notifier.show_note(noteTitle, noteBody, noteSummary);
    }

    public void check_notifier(Notifier noty)
    {
        //called at the end of event. controls how we'll get back to the overworld.
        if (useNotifierOnEnd == true)
        {
            post_event_message(noty);
        }
        else
        {
            noty.dismiss();
        }
    }

    public virtual void post_event()
    {
        //does any game state related things for the game. always called.

    }

}
