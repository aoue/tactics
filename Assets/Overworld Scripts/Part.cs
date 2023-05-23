using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Part : MonoBehaviour
{
    //manages a day/similar section of the overworld.

    //has a list of all possible events for that day
    //and an index to the overworld background manager to tell us what background to display

    [SerializeField] private TextAsset immediate; //the script for the immediate. Plays immediately on part start.
    [SerializeField] private EventHolder[] partEvents;
    [SerializeField] private int messagerID; // the id used to get information for the messager in overworld. -1 for nothing.
    [SerializeField] private int backgroundIndex;
    [SerializeField] private int musicIndex; //the index in overworld audio of the music track to play. -1 to stop.
    [SerializeField] private string dateString; //the date string. displayed in top left.

    public void pass_event_to_overworld(EventHolder ev)
    {
        gameObject.transform.parent.GetComponent<Overworld>().load_event(ev);
    }
    public void pass_combat_to_overworld(int missionID)
    {
        gameObject.transform.parent.GetComponent<Overworld>().load_combat(missionID);
    }
    public void pass_part_to_overworld(int partID)
    {
        gameObject.transform.parent.GetComponent<Overworld>().switch_part(partID);
    }

    public EventHolder[] get_events() { return partEvents; }
    public int get_backgroundIndex() { return backgroundIndex; }
    public int get_musicIndex() { return musicIndex; }
    public TextAsset get_story() { return immediate; }
    public string get_dateString() { return dateString; }
    public int get_messagerID() { return messagerID; }
}
