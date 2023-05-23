using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterImageSet : ImageSet
{
    // holds a character image set, both full portraits and box portraits.
    [SerializeField] private Sprite[] box;

    public Sprite get_boxImage(int i){return box[i]; }
}
