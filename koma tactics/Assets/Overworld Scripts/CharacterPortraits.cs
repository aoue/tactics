using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPortraits : MonoBehaviour
{
    // holds the portraits (full and box)
    // for a given character.

    // see the text file for notes on which index corresponds to which expression.


    [SerializeField] private Sprite[] full;
    [SerializeField] private Sprite[] box;

    public Sprite get_fullPortrait(int i){return full[i]; }
    public Sprite get_boxPortrait(int i){return box[i]; }

}
