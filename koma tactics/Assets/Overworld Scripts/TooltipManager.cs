using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TooltipManager : MonoBehaviour
{
    //manages the event tooltip.
    //takes calls direct from EventHolders.

    //on event mouseover, does: (even if event is not interactable. but it must be shown.)
    // - fills event's location to location text.
    // - fills event's title to title text.
    // - sets up portraits from event's information.


    [SerializeField] private Text locationText; //goes at the very top of the event preview.
    [SerializeField] private Text infoText; //between location text/title text and portrait slots. gives some information. 3 lines, but all are short.
    [SerializeField] private Image[] portraitSlots; //4 of them. the event's cast previews. 90x90. a -1 means no sprite.
    [SerializeField] private Canvas evCanvas;

    [SerializeField] private RectTransform rt;

    public void dismiss()
    {
        gameObject.SetActive(false);
    }
    void handle_positioning(float x, float y)
    {
        //doesn't work with different resolutions - for now, the tooltip is stationary.
        /*
        //if we go off the right side
        float xOffset = rt.rect.width * evCanvas.scaleFactor;
        float yOffset = rt.rect.height * evCanvas.scaleFactor;
        if ( x > 1920 * evCanvas.scaleFactor)
        {

        }

        //if we go off the top
        if ( y > 1080 * evCanvas.scaleFactor)
        {

        }

        gameObject.transform.localPosition = new Vector2(x + xOffset, y + yOffset);        
        */
    }
    public void show_event_preview(Event ev, PortraitLibrary pLibrary, float x, float y)
    {
        //First, position yourself next to the actual event. 
        //x and y are the event's coords, so we'll set ourselves nearby with relation to them.
        //default on the right, but if x is too high, then we'll show up on the left, instead. :)
        handle_positioning(x, y);

        //Fill out event information, like location, event title, and charges consumed by it.
        locationText.text = ev.get_noteTitle();

        infoText.text = ev.get_eventPreviewLines(); //ayy

        //for slots we have images for - fill them.
        for (int i = 0; i < ev.get_portraitPreviews().Length; i++)
        {
            portraitSlots[i].sprite = pLibrary.retrieve_boxp(ev.get_portraitPreviews()[i]);
            portraitSlots[i].gameObject.SetActive(true);
        }
        //for slots we don't have images for - hide them.
        for (int i = ev.get_portraitPreviews().Length; i < 4; i++)
        {
            portraitSlots[i].gameObject.SetActive(false);
        }

        gameObject.SetActive(true);
    }

}
