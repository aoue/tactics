using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfinderTuple
{
    // a wrapper class that goes around tile, and holds values useful for A*.

    public PathfinderTuple()
    {
        reset();
    }
    public void reset()
    {
        prev = null;
        g = 0;
        h = 0;
        f = 0;
    }

    public PathfinderTuple prev {get; set;}
    public int g {get; set;}
    public int h {get; set;}
    public int f {get; set;}
}
