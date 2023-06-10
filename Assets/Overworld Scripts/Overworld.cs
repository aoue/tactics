using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Overworld : MonoBehaviour
{
    //this is the main overworld controller object.

    //audio
    [SerializeField] private OverworldAudio audio;

    //visuals
    [SerializeField] private Text partText; //In the top left corner, used to display the date. 
    [SerializeField] private Image backgroundFrame;
    [SerializeField] private EventManager evMan;   
    [SerializeField] private LevelTreeManager treeMan;

    [SerializeField] private BackgroundManager overworldBackgrounds;
    [SerializeField] private Part[] parts; //all their children are event holders.

    //messager
    [SerializeField] private Messager messageManager; // link to the messager object. Set during part loading.
    
    //day logic
    private int currentPartIndex;
    private int dayProgression;
    private bool ranImmediate;

    void Start()
    {
        //when we load, we check the part we are set at, and load that part in the game.
        //Debug.Log("carrier part index is " + Carrier.Instance.get_nextPartIndex());
        Carrier.Instance.distribute_exp();
        change_part(Carrier.Instance.get_nextPartIndex());
    }

    public void change_part(int newPartIndex)
    {
        currentPartIndex = newPartIndex;

        partText.text = parts[currentPartIndex].get_dateString();

        for (int i = 0; i < parts.Length; i++)
        {
            if (currentPartIndex == i) parts[i].gameObject.SetActive(true);
            else parts[i].gameObject.SetActive(false);
        }
        dayProgression = 0;
        backgroundFrame.sprite = overworldBackgrounds.get_backgroundSprite(parts[currentPartIndex].get_backgroundIndex());
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
            if (eh.validate(dayProgression))
            {
                //Debug.Log("event validated sir!");
                eh.setup_event();
            }
            else
            {
                eh.disable_event();
            }
        }
        messageManager.validate(parts[currentPartIndex].get_messagerID());

        if (!ranImmediate && parts[currentPartIndex].get_story() != null)
        {
            //load part's immediate
            ranImmediate = true;
            evMan.begin_immediate(parts[currentPartIndex].get_story());
        }
        else
        { 
            //if no immediate, then we play overworld music and also fade
            evMan.independent_from_black_fade();
            audio.ow_play_music(parts[currentPartIndex].get_musicIndex());
        }

    }
    public void load_level_tree()
    {
        treeMan.load(0);
    }

    //EVENT TRIGGERS, PASSED ON BY PART
    public void load_event(EventHolder ev)
    {
        //load a normal event
        //Debug.Log("loading event: " + ev.get_eventTitle());
        
        //fade in
        evMan.independent_to_black_fade();
        //evMan.begin_event(ev);
        StartCoroutine(pause_before_loading_event(ev));
    }
    public void load_combat(int id)
    {
        //load a combat mission
        //Debug.Log("loading combat: " + parts[currentPartIndex].get_combatMissionIndex());   
        Carrier.Instance.set_nextMissionIndex(id);
        StartCoroutine(pause_before_loading_combat_mission());
    }
    public void switch_part(int id)
    {
        StartCoroutine(pause_before_loading_part_directly(id));
    }

    //HELPERS
    IEnumerator pause_before_loading_event(EventHolder ev)
    {
        yield return new WaitForSeconds(1f);
        evMan.begin_event(ev);
    }
    IEnumerator pause_before_loading_part_directly(int id)
    {
        //fade
        evMan.independent_from_black_fade();
        yield return new WaitForSeconds(2f);
        change_part(id);
    }
    IEnumerator pause_before_loading_combat_mission()
    {
        //fade
        evMan.independent_to_black_fade();
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(2);
    }

    //GETTERS
    public int get_progression() { return dayProgression; }
    
    //SETTERS
    public void set_progression(int i) { dayProgression = i; }
    

}
