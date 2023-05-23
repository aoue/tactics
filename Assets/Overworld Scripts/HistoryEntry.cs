using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HistoryEntry
{
    public string name { get; set; }
    public string sentence { get; set; }
    public Color nameColor {get; set;}
    public Color sentenceColor {get; set;}

    public HistoryEntry(string n, string s, Color nColor, Color sColor)
    {
        name = n;
        sentence = s;
        nameColor = nColor;
        sentenceColor = sColor;
    }
}
