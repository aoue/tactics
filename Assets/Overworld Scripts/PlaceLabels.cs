using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceLabels
{
    // Manages labels and box placement for eventmanager.
    // Dictionary format:
    //  string of label name: (vector2 of textbox position, width of textbox position)

    Dictionary<string, (Vector2, float)> positions = new Dictionary<string, (Vector2, float)>()
    {
        // SPECIAL
        {"bottom-narration", (new Vector2(100f, -200f), 1200f)},
        {"nvl", (new Vector2(0f, 500f), 1200f)},

        // PORTRAIT SLOT 0 (FAR LEFT)
        // PORTRAIT SLOT 1 (MID-LEFT)

        // PORTRAIT SLOT 2 (CENTER)
        {"center-right-chin", (new Vector2(480f, 300f), 800f)},
        {"center-left-chin", (new Vector2(-200f, 250f), 800f)},

        // PORTRAIT SLOT 2 (MID-RIGHT)
        // PORTRAIT SLOT 2 (FAR RIGHT)

        // PORTRAIT SLOT 5 (CLOSEUP)
        //{"friday-close-right", (new Vector2(700f, 317f), 800f)},
    };

    public (Vector2, float) label_vals(string label)
    {
        return positions[label];
    }


    
}
