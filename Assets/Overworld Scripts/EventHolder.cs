using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Ink.Runtime;

public enum eventType { NONE, LOAD_PART, LOAD_MISSION };
public class EventHolder : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // this holds info and starts the event.

    //is the one to try to validate the event.
    [SerializeField] private PortraitLibrary pLib;
    [SerializeField] private Sprite combatSprite;
    [SerializeField] private Sprite storySprite;
    [SerializeField] private Sprite unknownSprite;
    [SerializeField] private Button beginButton;
    [SerializeField] private Image attachedFrame;
    [SerializeField] private TextAsset ev;
    [SerializeField] private eventType type;
    
    //progression
    [SerializeField] private int minProgressionToEnable; // the minimum value of progression needed for the event to validate.
    [SerializeField] private int incProgression; // if true, then add +1 to part's progression.
    [SerializeField] private int attachedCharacter; // ids of box images shown in the attached image slots.

    [SerializeField] private int loadIndex; // the index of the part or mission that will be loaded on this.

    private bool completed; // starts false. Becomes true once event has been run. Must be false for event to validate.

    //RUNNING OR HIDING EVENT
    public void setup_event()
    {
        //called if the event passes validation.
        
        // fill the attached character portrait slots
        // while less than length, turn on slot and fill with character image
        switch(attachedCharacter)
        {
            case -3:
                attachedFrame.sprite = unknownSprite;
                break;
            case -2:
                attachedFrame.sprite = combatSprite;
                break;
            case -1:
                attachedFrame.sprite = storySprite;
                break;
            default:
                attachedFrame.sprite = pLib.retrieve_boxp(attachedCharacter);
                break;

        }
        if (type == eventType.NONE)
        {
            beginButton.gameObject.GetComponent<Image>().color = new Color(0.76f, 0.76f, 0.76f);
        }
        else
        {
            beginButton.gameObject.GetComponent<Image>().color = new Color(200f/255f, 46f/255f, 46f/255f);
        }
        gameObject.SetActive(true);
    }
    public void disable_event()
    {
        //called if the event fails to validate.
        gameObject.SetActive(false);
    }

    public void begin_event()
    {
        // called when the event button is clicked. 
        beginButton.interactable = false;

        // not only that, but we need to disable all buttons of all events, right?

        switch(type)
        {
            case eventType.LOAD_MISSION:
                // load combat mission
                gameObject.transform.parent.GetComponent<Part>().pass_combat_to_overworld(loadIndex);
                break;
            case eventType.LOAD_PART:
                // load new part
                gameObject.transform.parent.GetComponent<Part>().pass_part_to_overworld(loadIndex);
                break;
            default:
                // play event as normal
                gameObject.transform.parent.GetComponent<Part>().pass_event_to_overworld(this);
                break;
        }
        
    }
    public int modify_day_progression(int dayProgress)
    {
        return dayProgress + incProgression;
    }
    public bool validate(int overworldProgression)
    {
        return !completed && overworldProgression >= minProgressionToEnable;
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
    public void set_completed() { completed = true; }
    public TextAsset get_story() { return ev; }

    
}
