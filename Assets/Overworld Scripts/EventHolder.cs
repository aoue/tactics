using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Ink.Runtime;

public enum eventType { NONE, HEART, BRO, MISSION};
public class EventHolder : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //holds an event, shows up on the overworld.

    //changes image depending on colour.
    //is the one to try to validate the event.

    [SerializeField] private TextAsset ev;
    [SerializeField] private string eventTitle;
    [SerializeField] private string eventDescr;

    //events can be flagged in three ways:
    [SerializeField] private eventType type;
    [SerializeField] private GameObject dangerIcon; //an exclamation mark; danger icon. This means the event will lock all other open events.
    [SerializeField] private GameObject heartIcon; //a heart; means the event has to do with love or something, man, idk.
    [SerializeField] private GameObject frienshipIcon; //a bro icon; means the event has to do with building friendship.

    //progression
    [SerializeField] private int minProgressionToEnable;
    [SerializeField] private bool addToProg; //if true, then when event is over, add progMod to partProgression
    [SerializeField] private bool setProg; //if true, then when event is over, set partProgression to progMod
    [SerializeField] private int progMod;

    //VIRTUALS - checking and modifying states
    public virtual void post_event()
    {
        //Note: modifying day progression is handled outside of this.

        //do whatever you want.
    }
    public virtual bool validate_event()
    {
        //Note: progression validation is handled outside of this.

        //overwrite to check any requirements you want.
        //things like:
        // -char rels, choices made, etc.

        //true means the event will be enabled and viewable.
        //false means it won't be.
        return true;
    }

    public int modify_day_progression(int dayProgress)
    {
        if (addToProg) return dayProgress + progMod;
        if (setProg) return progMod;
        return dayProgress;
    }
    public bool validate_progression(int overworldProgression)
    {
        return overworldProgression >= minProgressionToEnable;
    }
    public virtual void begin_event()
    {
        //when the button is clicked. 

        //disable input
        gameObject.GetComponent<Button>().interactable = false;

        //Start the event.
        gameObject.transform.parent.GetComponent<Part>().pass_event_to_overworld(this);

        //hide yourself
        gameObject.SetActive(false);
    }

    //RUNNING OR HIDING EVENT
    public void setup_event()
    {
        //called if the event passes validation.

        //check the danger/heart/friendship setting and enable whichever it calls for,
        //or none.
        switch (type)
        {
            case eventType.BRO:
                frienshipIcon.gameObject.SetActive(true);
                break;
            case eventType.HEART:
                heartIcon.gameObject.SetActive(true);
                break;
            case eventType.MISSION:
                dangerIcon.gameObject.SetActive(true);
                break;
            default:
                break;
        }
        gameObject.SetActive(true);
    }
    public void disable_event()
    {
        //called if the event fails to validate.
        gameObject.SetActive(false);
    }
    
    //HOVERING
    public void OnPointerEnter(PointerEventData eventData)
    {
        //called on pointer enter.
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        //called on pointer exit.
    }

    //GETTERS
    public TextAsset get_story() { return ev; }
    public string get_eventTitle() { return eventTitle; }
    public string get_eventDescr() { return eventDescr; }

    
}
