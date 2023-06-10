using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatFader : MonoBehaviour
{
    [SerializeField] private Image fadeTapestry;
    
    public void fade_from_black()
    {
        Color initFadeTapestry = new Color(0f, 0f, 0f, 1f);
        fadeTapestry.color = initFadeTapestry;
        fadeTapestry.gameObject.SetActive(true);
        fadeTapestry.raycastTarget = true;
        StartCoroutine(cr_fade_from_black());
    }
    IEnumerator cr_fade_from_black()
    {
        // hide textbox
        yield return new WaitForSeconds(1f);
        float duration = 1f;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            Color newAlpha = new Color(0f, 0f, 0f, 1f - elapsedTime / duration);
            fadeTapestry.color = newAlpha;
            yield return null;
        }
        fadeTapestry.raycastTarget = false;
    }

    public void fade_to_black()
    {
        Color initFadeTapestry = new Color(0f, 0f, 0f, 0f);
        fadeTapestry.color = initFadeTapestry;
        fadeTapestry.gameObject.SetActive(true);
        fadeTapestry.raycastTarget = true;
        StartCoroutine(cr_fade_to_black());
    }
    IEnumerator cr_fade_to_black()
    {
        // hide textbox
        float duration = 1f;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            Color newAlpha = new Color(0f, 0f, 0f, elapsedTime / duration);
            fadeTapestry.color = newAlpha;
            yield return null;
        }
        yield return new WaitForSeconds(1f);
        fadeTapestry.raycastTarget = false;
    }


}
