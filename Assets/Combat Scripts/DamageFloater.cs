using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageFloater : MonoBehaviour
{
    // create to display damage on a tile.
    // features:
    // -set color to either red or green depending
    
    [SerializeField] private TextMesh actualText;

    public void setup(string toShow, bool setRed, float lifetime)
    {
        if (setRed){
            //set color to red
            actualText.color = new Color(1f, 0f, 0f);
        }
        else{
            //set color to green
            actualText.color = new Color(0f, 1f, 0f);
        }
        
        float xOffset = Random.Range(transform.position.x - 0.025f, transform.position.x + 0.025f);
        float yOffset = Random.Range(transform.position.y - 0.025f, transform.position.y + 0.025f);
        transform.position = new Vector3(xOffset, yOffset, 0f);
        gameObject.GetComponent<MeshRenderer>().sortingLayerName = "Unit Layer";
        gameObject.GetComponent<MeshRenderer>().sortingOrder = 4;
        actualText.text = toShow;
        StartCoroutine(control(lifetime));
    }
    IEnumerator control(float lifetime)
    {
        // after being created, the icon should constantly be floating away from the user.
        // it should also fade in, linger a bit, then fade out while floating.
        // finally, it will destroy itself.
        // before being finally faded out and destroying itself.
        
        // the object will last for 3*lifetime
        // the first third, it will fade in from nothing
        // the second third, nothing
        // the final third, it will fade all the way out

        Vector3 targetPosition = new Vector3(transform.position.x + 0.1f , transform.position.y + 0.1f, transform.position.z);

        float elapsedTime = 0f;
        while (elapsedTime < lifetime)
        {
            //fade in
            float value = elapsedTime / lifetime;
            Color inc_light = new Color(actualText.color.r, actualText.color.g, actualText.color.b, value);
            actualText.color = inc_light;

            elapsedTime += Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, elapsedTime / lifetime * Time.deltaTime);
            yield return null;
        }
        elapsedTime = 0f;
        while (elapsedTime < lifetime/2)
        {
            //fade out
            float value = 1f - (elapsedTime / lifetime*2);
            Color inc_dark = new Color(actualText.color.r, actualText.color.g, actualText.color.b, value);
            actualText.color = inc_dark;

            elapsedTime += Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, elapsedTime / lifetime/2 * Time.deltaTime);
            yield return null;
        }

        Destroy(gameObject);
    }
}
