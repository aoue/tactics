using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PortraitLibrary : MonoBehaviour
{
    //the portrait library.
    //holds all the portraits in the game, all accessible with an int.

    //the backgrounds used during overworld events.
    [SerializeField] private Sprite[] bgs;
    [SerializeField] private Sprite[] overlays;
    [SerializeField] private Sprite[] frameCGs;

    //full portraits for standing in the vn
    // 0: friday
    // 1: mc
    // 2: bonelord
    [SerializeField] private CharacterPortraits[] portraitSets;

    public Sprite retrieve_cgFrame(int id)
    {
        // this is used for combat scene images, i think?
        return frameCGs[id];
    }

    public Sprite retrieve_eventBg(int id)
    {
        return bgs[id];
    }
    public Sprite retrieve_overlay(int id)
    {
        return overlays[id];
    }

    public Sprite retrieve_boxp(int index)
    {
        //the index passed in is subdivided into 2:
        int char_index = index / 100; //e.g. 502 -> 5
        int expression_index = index % 100; //e.g. 502 -> 2

        return portraitSets[char_index].get_boxPortrait(expression_index);
    }

    public Sprite retrieve_fullp(int index)
    {
        //the index passed in is subdivided into 2:
        int char_index = index / 100; //e.g. 502 -> 5
        int expression_index = index % 100; //e.g. 502 -> 2

        return portraitSets[char_index].get_fullPortrait(expression_index);
    }

}
