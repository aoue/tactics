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
    [SerializeField] private int backgroundIndex;

    [SerializeField] private int combatMissionIndex; //the index in the mission list that the part will load.

    public void pass_event_to_overworld(EventHolder ev)
    {
        gameObject.transform.parent.GetComponent<Overworld>().load_event(ev);
    }

    public EventHolder[] get_events() { return partEvents; }
    public int get_backgroundIndex() { return backgroundIndex; }
    public int get_combatMissionIndex() { return combatMissionIndex; }
    public TextAsset get_story() { return immediate; }
}
