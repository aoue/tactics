using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PortraitLibrary : MonoBehaviour
{
    //the portrait library.
    //holds all the portraits in the game, all accessible with an int.

    //the backgrounds used during overworld events.
    //all in the game are here.
    //Index:
    // 0:
    [SerializeField] private Sprite[] bgs;
    [SerializeField] private Sprite[] frameCGs;

    //for event previews. no expression variations, all characters are in there, 1 sprite each.
    //The sprite is neck up, talking-portrait. Also used in combat events for the portrait slot.
    //(set with the p() function in Ink.)
    //Index:
    // 0: mc
    // 1: friday
    // 2: yve
    // 3: nai
    // etc
    [SerializeField] private Sprite[] boxSprites;


    //full portraits. Here, the index are subdivided into two keys:
    // / 100 = which character
    //Index:
    // 0: mc
    // 1: friday
    // 2: yve
    // 3: nai
    // etc

    // % 100 = which expression
    //Index:
    // 0: neutral
    // 1: 
    // etc
    [SerializeField] private Sprite[] mcFull;
    [SerializeField] private Sprite[] fridayFull;
    [SerializeField] private Sprite[] yveFull;
    [SerializeField] private Sprite[] naiFull;

    public Sprite retrieve_cgFrame(int id)
    {
        return frameCGs[id];
    }

    public Sprite retrieve_eventBg(int id)
    {
        return bgs[id];
    }

    public Sprite retrieve_boxp(int index)
    {
        //used for event previews. nothing major.
        //only a handful of box sprites, anyway.
        //they don't have expressions - just a single shot of the speaker—to help the player match name with face.
        //returns according to char index legend.
        return boxSprites[index];
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
                return mcFull[expression_index];
            case 1: //friday
                return fridayFull[expression_index];
            case 2: //mueler
                return yveFull[expression_index];
            case 3: //maddy
                return naiFull[expression_index];

                //etc.
        }

        return null;
    }

}
