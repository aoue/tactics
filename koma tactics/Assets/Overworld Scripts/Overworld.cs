using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Overworld : MonoBehaviour
{
    //game state

    //visuals
    [SerializeField] private FadeManager fader;
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
        if (Carrier.Instance.get_startingNewGame())
        {
            //NEW GAME:
            //reset carrier prefab, units, choices, etc
            Debug.Log("carrier is resetting");
            Carrier.Instance.reset();
        }
        currentPartIndex = Carrier.Instance.get_nextPartIndex();
        Debug.Log("current part index set to " + currentPartIndex + " by carrier.");

        //set current part to active. Hide all other parts.
        for(int i = 0; i < parts.Length; i++)
        {
            if (i == currentPartIndex)
            {
                parts[i].gameObject.SetActive(true);
                //Debug.Log("enabling part " + i);
            }
            else
            {
                parts[i].gameObject.SetActive(false);
                //Debug.Log("disabling part " + i);
            }
        }
        

        dayProgression = 0;
        //set the background of the part
        backgroundFrame.sprite = overworldBackgrounds.get_backgroundSprite(parts[currentPartIndex].get_backgroundIndex());
        allowInput = false;
        ranImmediate = false;

        load_part();
    }

    public void change_part(int newPartIndex)
    {
        //called by the eventholder derived class; newpart eventholder.
        for (int i = 0; i < parts.Length; i++)
        {
            if (currentPartIndex == i) parts[currentPartIndex].gameObject.SetActive(true);
            else parts[currentPartIndex].gameObject.SetActive(false);
        }
        backgroundFrame.sprite = overworldBackgrounds.get_backgroundSprite(parts[currentPartIndex].get_backgroundIndex());
        allowInput = false;
        ranImmediate = false;
        currentPartIndex = newPartIndex;
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
            //Debug.Log("loading combat: " + parts[currentPartIndex].get_combatMissionIndex());          
            Carrier.Instance.set_nextMissionIndex(parts[currentPartIndex].get_combatMissionIndex());
            StartCoroutine(pause_before_loading_combat_mission());
        }
        else
        {
            //load a normal event
            Debug.Log("loading event: " + ev.get_eventTitle());
            allowInput = false;

            evMan.begin_event(ev);
        }

        
    }

    IEnumerator pause_before_loading_combat_mission()
    {
        fader.fade_to_black_stay();
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(2);
    }

    //GETTERS
    public void set_progression(int i) { dayProgression = i; }

    //SETTERS
    public int get_progression() { return dayProgression; }

}
