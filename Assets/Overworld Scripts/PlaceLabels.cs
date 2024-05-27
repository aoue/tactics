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
        {"0-right-high", (new Vector2(-80f, 415f), 800f)},
        {"0-right-mid", (new Vector2(-315f, 110f), 800f)},
        {"0-right-low", (new Vector2(-500f, -65f), 800f)},
        {"0-center-mid", (new Vector2(-545f, 185f), 800f)},
        {"0-center-low", (new Vector2(-550f, -95f), 800f)},

        // PORTRAIT SLOT 1 (MID-LEFT)
        {"1-right-high", (new Vector2(240f, 300f), 800f)},
        {"1-right-mid", (new Vector2(120f, 150f), 800f)},
        {"1-right-low", (new Vector2(60f, -50f), 800f)},
        {"1-center-mid", (new Vector2(-330f, 120f), 800f)},
        {"1-center-low", (new Vector2(-370f, -75f), 800f)},
        {"1-left-high", (new Vector2(-650f, 260f), 600f)},
        {"1-left-mid", (new Vector2(-580f, 120f), 600f)},
        {"1-left-low", (new Vector2(-600f, -25f), 600f)},

        // PORTRAIT SLOT 2 (CENTER)
        {"2-right-high", (new Vector2(480f, 300f), 800f)},
        {"2-right-center", (new Vector2(360f, 150f), 800f)},
        {"2-right-low", (new Vector2(300f, -50f), 800f)},
        {"2-center-mid", (new Vector2(0f, 120f), 800f)},
        {"2-center-low", (new Vector2(-95f, -45f), 800f)},
        {"2-left-high", (new Vector2(-480f, 300f), 800f)},
        {"2-left-med", (new Vector2(-360f, 150f), 800f)},
        {"2-left-low", (new Vector2(-300f, -50f), 800f)},

        // PORTRAIT SLOT 3 (MID-RIGHT)
        {"3-right-high", (new Vector2(630f, 310f), 600f)},
        {"3-right-mid", (new Vector2(525f, 160f), 600f)},
        {"3-right-low", (new Vector2(600f, -55f), 600f)},
        {"3-center-mid", (new Vector2(250f, 135f), 800f)},
        {"3-center-low", (new Vector2(270f, -95f), 800f)},
        {"3-left-high", (new Vector2(-245f, 340f), 800f)},
        {"3-left-mid", (new Vector2(-195f, 190f), 800f)},
        {"3-left-low", (new Vector2(45f, -45f), 800f)},

        // PORTRAIT SLOT 4 (FAR RIGHT)
        {"4-left-high", (new Vector2(85f, 318f), 800f)},
        {"4-left-mid", (new Vector2(-15f, 130f), 800f)},
        {"4-left-low", (new Vector2(215f, -125f), 800f)},
        {"4-center-mid", (new Vector2(505f, 105f), 800f)},
        {"4-center-low", (new Vector2(530f, -110f), 800f)},

        // PORTRAIT SLOT 5 (CLOSEUP)
        {"5-right-high", (new Vector2(555f, 360f), 800f)},
        {"5-right-center", (new Vector2(495f, 70f), 800f)},
        {"5-right-low", (new Vector2(420f, -135f), 800f)},
        {"5-center-mid", (new Vector2(0f, -30f), 800f)},
        {"5-center-low", (new Vector2(-55f, -230f), 800f)},
        {"5-left-high", (new Vector2(-545f, 340f), 800f)},
        {"5-left-med", (new Vector2(-500f, 30f), 800f)},
        {"5-left-low", (new Vector2(-415f, -60f), 800f)},
    };

    public (Vector2, float) label_vals(string label)
    {
        return positions[label];
    }


    
}
