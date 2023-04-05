using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HistoryEntry
{
    public string name { get; set; }
    public string sentence { get; set; }

    public HistoryEntry(string n, string s)
    {
        name = n;
        sentence = s;
    }
}
