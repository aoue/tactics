using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{
    //fade manager. used for fading to black and then back.
    /*
    - overworld -> event
	- overworld -> dungeon
	- dungeon -> event
	- dungeon -> combat
	- combat -> dungeon
	- dungeon -> overworld
    */

    //at scene start:
    // -object not active.

    [SerializeField] Image blackOutSquare;
    private float default_duration = 1.0f;

    public float get_default_duration() { return default_duration; }

    public void hide()
    {
        gameObject.SetActive(false);
    }

    private float convert_time_arg(float timeArg)
    {
        if (timeArg == -1f)
        {
            return default_duration;
        }
        return timeArg;
    }
    public void fade_to_black(float time = -1f)
    {      
        gameObject.SetActive(true);
        StartCoroutine(fade(true, convert_time_arg(time)));
    }
    public void fade_from_black(float time = -1f)
    {
        gameObject.SetActive(true);
        StartCoroutine(fade(false, convert_time_arg(time)));
    }

    public void fade_from_black_cheat(float time = -1f)
    {
        //sets to dark, then fades to light.
        Color objectColor = blackOutSquare.GetComponent<Image>().color;
        objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, 1f);
        blackOutSquare.GetComponent<Image>().color = objectColor;

        gameObject.SetActive(true);
        StartCoroutine(fade(false, convert_time_arg(time)));
    }

    IEnumerator fade(bool fadeToBlack, float fadeSpeed = -1f)
    {
        Color objectColor = blackOutSquare.GetComponent<Image>().color;
        float fadeAmount;
        if (fadeToBlack == true)
        {
            while ( blackOutSquare.GetComponent<Image>().color.a < 1)
            {
                fadeAmount = objectColor.a + (convert_time_arg(fadeSpeed) * Time.deltaTime);

                objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
                blackOutSquare.GetComponent<Image>().color = objectColor;
                yield return null;
            }
            fade_from_black(convert_time_arg(fadeSpeed));
        }
        else
        {
            while ( blackOutSquare.GetComponent<Image>().color.a > 0)
            {
                fadeAmount = objectColor.a - (convert_time_arg(fadeSpeed) * Time.deltaTime);

                objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
                blackOutSquare.GetComponent<Image>().color = objectColor;
                yield return null;
            }
            gameObject.SetActive(false);
        }
        
    }

}
