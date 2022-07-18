using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EventHolder : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //holds an event, shows up on the overworld.

    //changes image depending on colour.
    //is the one to try to validate the event.

    [SerializeField] private Event ev;
    [SerializeField] private GameObject heartIcon;
    public int get_id() { return ev.get_id(); }
    private bool added;

    public void begin_event()
    {
        //on button click. you know, the button attached to this object.
        EventManager.event_triggered(ev, true);

        //mark that this event is done for Part's tracking. :)
        //adds ev.get_id() to done events list.
        Part.doneEvents.Add(ev.get_id());


        //remove event from worldmanager active event list.
        Overworld.remove_active_event(ev.get_id());
        Destroy(this.gameObject);
    }

    public void disable_button()
    {
        gameObject.GetComponent<Button>().interactable = false;
    }

    public bool validate_event()
    {
        return ev.isEventValid();
    }

    public void setup_event()
    {
        //set your colour
        Image icon = gameObject.GetComponent<Image>();
        gameObject.GetComponent<Button>().interactable = true;
        switch (ev.get_event_type())
        {
            case eventType.RED:
                icon.color = Color.red;
                break;
            case eventType.BLUE:
                icon.color = Color.blue;
                break;
            case eventType.GREEN:
                icon.color = Color.green;
                break;
        }

        //if event has haich, then enable that aspect of the event image
        if (ev.get_hasHaich() == true)
        {
            heartIcon.SetActive(true);
        }

        gameObject.SetActive(true);
    }

    //detect when mouse enters and exits the event icon
    public void OnPointerEnter(PointerEventData eventData)
    {
        //called on pointer enter. tells eventmanager and asks
        //it to show the event preview (which it does, obviously).
        EventManager.event_hovered(ev, gameObject.transform.localPosition.x, gameObject.transform.localPosition.y);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        //called on pointer exit. tells eventmanager to stop showing the preview.
        EventManager.event_unhovered();
    }


}
