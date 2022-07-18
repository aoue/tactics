using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundManager : MonoBehaviour
{
    //holds all the overworld backgrounds
    //dungeon backgrounds too
    //dia backgrounds too


    [SerializeField] private Sprite[] backgroundSprites;

    public Sprite get_backgroundSprite(int chapter) { return backgroundSprites[chapter]; }

}
