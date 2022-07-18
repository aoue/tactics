using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PortraitLibrary : MonoBehaviour
{
    //the portrait library.
    //holds all the portraits in the game, all accessible with an int.
    // CHAR INDEX LEGEND:
    // 0: yve
    // 1: friday
    // 2: mueler
    // 3: maddy
    //etc

    // EXPRESSION INDEX LEGEND:
    // 0: neutral
    // 1: 
    // 2: 
    //etc
    [SerializeField] private Sprite[] eventBgs;
    [SerializeField] private Sprite[] boxSprites; //for event previews. no expression variations, all characters are in there, 1 each.

    [SerializeField] private Sprite[] yveSpeaker;
    [SerializeField] private Sprite[] fridaySpeaker;
    [SerializeField] private Sprite[] muelerSpeaker;
    [SerializeField] private Sprite[] maddySpeaker;

    [SerializeField] private Sprite[] yveFull;
    [SerializeField] private Sprite[] fridayFull;
    [SerializeField] private Sprite[] muelerFull;
    [SerializeField] private Sprite[] maddyFull;

    public Sprite retrieve_eventBg(int id)
    {
        return eventBgs[id];
    }

    public Sprite retrieve_boxp(int index)
    {
        //used for event previews. nothing major.
        //only a handful of box sprites, anyway.
        //returns according to char index legend.
        return boxSprites[index];
    }

    public Sprite retrieve_speakerp(int index)
    {
        //the way this one works is a bit more complicated.
        //all characters have multiple full portraits, which may change during the dialogue.

        //the index passed in is subdivided into 2:
        int char_index = index / 100; //e.g. 502 -> 5
        int expression_index = index % 100; //e.g. 502 -> 2

        //now, char_index is used to select the array we want.
        //then, expression_index is used to select an element (which is a sprite) from the array.
        switch (char_index)
        {
            case 0: //yve
                return yveSpeaker[expression_index];
            case 1: //friday
                return fridaySpeaker[expression_index];
            case 2: //mueler
                return muelerSpeaker[expression_index];
            case 3: //maddy
                return maddySpeaker[expression_index];

                //etc.
        }

        return null;
    }


    public Sprite retrieve_fullp(int index)
    {
        //the way this one works is a bit more complicated.
        //all characters have multiple full portraits, which may change during the dialogue.

        //the index passed in is subdivided into 2:
        int char_index = index / 100; //e.g. 502 -> 5
        int expression_index = index % 100; //e.g. 502 -> 2

        //now, char_index is used to select the array we want.
        //then, expression_index is used to select an element (which is a sprite) from the array.
        switch (char_index)
        {
            case 0: //yve
                return yveFull[expression_index];
            case 1: //friday
                return fridayFull[expression_index];
            case 2: //mueler
                return muelerFull[expression_index];
            case 3: //maddy
                return maddyFull[expression_index];

            //etc.
        }

        return null;
    }
}
