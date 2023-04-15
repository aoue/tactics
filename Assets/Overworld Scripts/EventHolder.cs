using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Ink.Runtime;

public enum eventType { RED_COMBAT, RED, BLUE, GREEN };
public class EventHolder : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // this holds info and starts the event.

    //is the one to try to validate the event.
    [SerializeField] private PortraitLibrary pLib;
    [SerializeField] private GameObject combatIcon;
    [SerializeField] private Button beginButton;
    [SerializeField] private Image[] attachedFrames;
    [SerializeField] private TextAsset ev;
    [SerializeField] private eventType type;
    
    //progression
    [SerializeField] private int minProgressionToEnable; // the minimum value of progression needed for the event to validate.
    [SerializeField] private int incProgression; // if true, then add +1 to part's progression.
    [SerializeField] private int[] attachedCharacters; // ids of box images shown in the attached image slots.

    [SerializeField] private int partLoadIndex; // if is a RED type event, load this part index on click. Otherwise, ignore it.
    [SerializeField] private int combatLoadIndex; // if is a RED_COMBAT type event, load this mission index on click. Otherwise, ignore it.

    private bool completed; // starts false. Becomes true once event has been run. Must be false for event to validate.

    //RUNNING OR HIDING EVENT
    public void setup_event()
    {
        //called if the event passes validation.

        // set the colour and visual effects accompanying of the event.
        Image frame = gameObject.GetComponent<Image>();
        switch(type)
        {
            case eventType.RED_COMBAT:
                frame.color = new Color(255f / 255f, 102f / 255f, 102f/ 255f);
                combatIcon.SetActive(true);
                break;
            case eventType.RED:
                frame.color = new Color(255f / 255f, 102f / 255f, 102f / 255f);
                combatIcon.SetActive(false);
                break;
            case eventType.BLUE:
                frame.color = new Color(102 / 255f, 102f / 255f, 255f / 255f);
                combatIcon.SetActive(false);
                break;
            case eventType.GREEN:
                frame.color = new Color(102f / 255f, 255f / 255f, 102f / 255f);
                combatIcon.SetActive(false);
                break;
            default:
                break;
        }
        

        // fill the attached character portrait slots
        // while less than length, turn on slot and fill with character image
        for(int i = 0; i < attachedCharacters.Length; i++)
        {
            attachedFrames[i].sprite = pLib.retrieve_boxp(attachedCharacters[i]);
            attachedFrames[i].gameObject.SetActive(true);
        }
        // while greater or equal to length, hide slot
        for(int i = attachedCharacters.Length; i < 4; i++)
        {
            attachedFrames[i].gameObject.SetActive(false);
        }
        gameObject.SetActive(true);
    }
    public void disable_event()
    {
        //called if the event fails to validate.
        gameObject.SetActive(false);
    }

    public virtual void begin_event()
    {
        // called when the event button is clicked. 
        beginButton.interactable = false;

        // not only that, but we need to disable all buttons of all events, right?

        switch(type)
        {
            case eventType.RED_COMBAT:
                // load combat mission
                gameObject.transform.parent.GetComponent<Part>().pass_combat_to_overworld(combatLoadIndex);
                break;
            case eventType.RED:
                // load new part
                gameObject.transform.parent.GetComponent<Part>().pass_part_to_overworld(partLoadIndex);
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
