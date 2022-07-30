using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Overworld : MonoBehaviour
{
    //visuals
    [SerializeField] private BackgroundManager overworldBackgrounds;
    [SerializeField] private Image backgroundFrame;
    [SerializeField] private EventManager evMan;

    
    [SerializeField] private Part[] parts; //all their children are event holders.

    //day logic
    private int currentPartIndex;
    private int dayProgression;
    private bool allowInput;
    private bool ranImmediate;

    //transporting data between scenes - we store the data in a prefab.


    void Start()
    {
        //when we load, we check the part we are set at, and load that part in the game.
        currentPartIndex = 0; //dummy; set to 0 for now

        dayProgression = 0;
        //set the background of the part
        backgroundFrame.sprite = overworldBackgrounds.get_backgroundSprite(parts[currentPartIndex].get_backgroundIndex());
        allowInput = false;
        ranImmediate = false;

        load_part();
    }

    public void load_part()
    {
        //will be called when the part is first loaded, but also after each event has completed.
        //(to account for new day progression.)

        //for all the eventholders of this part:
        // -validate each eventholder
        //if valid:
        // -display it
        //else:
        // -hide it
        foreach (EventHolder eh in parts[currentPartIndex].get_events())
        {
            if (eh.validate_progression(dayProgression) && eh.validate_event())
            {
                eh.setup_event();
            }
            else
            {
                eh.disable_event();
            }
        }

        if (!ranImmediate && parts[currentPartIndex].get_story() != null)
        {
            //load part's immediate
            ranImmediate = true;
            evMan.begin_immediate(parts[currentPartIndex].get_story());
        }
        else
        {
            allowInput = true;
        }
    }

    public void load_event(EventHolder ev)
    {
        allowInput = false;

        if (ev.get_loadMission())
        {
            //load a combat mission
            Debug.Log("loading combat: " + parts[currentPartIndex].get_combatMissionIndex());
            SceneManager.LoadScene(2);
        }
        else
        {
            //load a normal event
            Debug.Log("loading event: " + ev.get_eventTitle());
            allowInput = false;

            evMan.begin_event(ev);
        }

        
    }


    //GETTERS
    public void set_progression(int i) { dayProgression = i; }

    //SETTERS
    public int get_progression() { return dayProgression; }

}
