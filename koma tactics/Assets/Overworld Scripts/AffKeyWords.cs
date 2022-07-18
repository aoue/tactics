using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AffKeyWords
{
    //holds all the aff stuff in a central place, so we don't have to maintain copies all over.

    //mults (don't forget to change the real values in battle brain too.)
    public static string[,] affMultTextArray = new string[3, 3]
    {
        {"x1.0", "x0.75", "x1.25"},
        {"x1.25", "x1.0", "x0.75"},
        {"x0.75", "x1.25", "x1.0"}
    };
    
    //coloring
    public static Color get_aff_color(int index)
    {       
        //update: move slots are not coloured based on affinity anymore.
        //always return the default colour OR dark if index is -1
        if (index == -1)
        {
            return new Color(1f, 1f, 1f, 1f);
        }
        return new Color(60f / 255f, 106f / 255f, 108f / 255f, 1f);
        
        /*
        //returns color associated with aff and alpha set to 1.
        switch (index)
        {
            case 0: //earf
                return new Color(84f / 255f, 51f / 255f, 3f / 255f, 1f);
            case 1: //wind
                return new Color(85f / 255f, 224f / 255f, 122f / 255f, 1f);
            case 2: //water
                return new Color(63f / 255f, 160f / 255f, 235f / 255f, 1f);
            default: //null
                return new Color(1f, 1f, 1f, 1f);
        }      
        */
    }
    


    //aff names
    public static string get_affName(int index)
    {
        switch (index)
        {
            case 0: //earf
                return "Light";
            case 1: //wind
                return "Medium";
            case 2: //water
                return "Heavy";
            default: //null
                return "null";
        }
    }

}
