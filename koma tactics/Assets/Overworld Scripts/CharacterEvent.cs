using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEvent : Event
{

    //automatically adjusts progress
    [SerializeField] private int characterIndex;
    public override void post_event()
    {
        //find parent
        //childObject.transform.parent.gameObject
        //Debug.Log(gameObject.transform.parent.gameObject);
        //this.gameObject.transform.parent.gameObject.GetComponent<CharHolder>().progress++;

        Overworld.progress_charEvent(characterIndex);
    }


}
