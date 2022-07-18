using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Notifier : MonoBehaviour
{
    //shows notifications at the start of days and after events
    [SerializeField] private GameObject DialogueCanvas; //hides it at the end of events.
    [SerializeField] private GameObject mainObject;
    [SerializeField] private Text titleText;
    [SerializeField] private Text bodyText;
    [SerializeField] private Text summaryText;

    void Update()
    {
        //on space bar pressed, close notifier.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            dismiss();
        }
    }

    public virtual void show_note(string titleNote, string bodyNote, string summaryNote)
    {
        //three parts:
        // - title note, goes at the top. the event's name.
        // - body note, it's the flavour text.
        // - summary note, it's the tldr; what this really means.
        
        titleText.text = titleNote;
        bodyText.text = bodyNote;
        summaryText.text = summaryNote;
        mainObject.SetActive(true);
    }
    public virtual void dismiss()
    {
        //dismisses the notification.
        Overworld.check_active_events_status();
        mainObject.SetActive(false);
        DialogueCanvas.SetActive(false);
    }

    public void set_title(string t) { titleText.text = t;}
    public void set_body(string t) { bodyText.text = t; }
    public void set_sum(string t) { summaryText.text = t; }
    public GameObject get_mainObj() { return mainObject; }
    public GameObject get_diaCanvas() { return DialogueCanvas; }
}
