using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PortraitLibrary : MonoBehaviour
{
    //the portrait library.
    //holds all the portraits in the game, all accessible with an int.

    //the backgrounds used during overworld events.
    [SerializeField] private CharacterImageSet[] portraitSets;
    [SerializeField] private ImageSet bgs;
    [SerializeField] private ImageSet overlays;
    [SerializeField] private ImageSet messagerImages;
    [SerializeField] private ImageSet combatImages;
    
    // CHARACTER PORTRAIT SETS
    public Sprite retrieve_boxp(int index)
    {
        //the index passed in is subdivided into two parts:
        int char_index = index / 100; //e.g. 502 -> 5
        int expression_index = index % 100; //e.g. 502 -> 2
        return portraitSets[char_index].get_boxImage(expression_index);
    }
    public Sprite retrieve_fullp(int index)
    {
        //the index passed in is split into two parts:
        int char_index = index / 100; //e.g. 502 -> 5
        int expression_index = index % 100; //e.g. 502 -> 2
        return portraitSets[char_index].get_fullImage(expression_index);
    }

    // NON CHARACTER PORTRAIT SETS
    public Sprite retrieve_combatImage(int id)
    {
        // this is used for combat scene images, i think?
        return combatImages.get_fullImage(id);
    }
    public Sprite retrieve_messagerImage(int id)
    {
        // this is used for combat scene images, i think?
        return messagerImages.get_fullImage(id);
    }
    public Sprite retrieve_eventBg(int id)
    {
        return bgs.get_fullImage(id);
    }
    public Sprite retrieve_overlay(int id)
    {
        return overlays.get_fullImage(id);
    }

    

}
