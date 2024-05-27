using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HistoryEntry
{
    public string sentence {get; set;}
    public Color bgColor {get; set;}
    public Sprite speakerSprite {get; set;}

    public HistoryEntry(string s, Color sColor, Sprite speaker)
    {
        sentence = s;
        bgColor = sColor;
        speakerSprite = speaker;
    }

}
