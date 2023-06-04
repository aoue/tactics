using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagePreviewFloater : MonoBehaviour
{
    // create to preview the damage range that would be displayed on a tile.
    // in the format: (min-max) dmg

    // on hover over tile with a unit on it, during aoe highlight phase,
    // one of these will spawn over the unit. it will quickly fade in.
    // it will remain until a different tile is highlighted during aoe highlight phase,
    // at which point it will be destroyed!

    [SerializeField] private TextMesh actualText;

    public void setup(string toShow, bool setRed)
    {
        if (setRed){
            //set color to red
            actualText.color = new Color(1f, 0f, 0f);
        }
        else{
            //set color to green
            actualText.color = new Color(0f, 1f, 0f);
        }
        
        float xOffset = transform.position.x - 0.025f;
        float yOffset = transform.position.y - 0.25f;
        transform.position = new Vector3(xOffset, yOffset, 0f);
        gameObject.GetComponent<MeshRenderer>().sortingLayerName = "Unit Layer";
        gameObject.GetComponent<MeshRenderer>().sortingOrder = 4;
        actualText.text = toShow;
        StartCoroutine(control());
    }
    public void remove()
    {
        Destroy(gameObject);
    }
    IEnumerator control()
    {
        float lifetime = 0.2f;

        Vector3 targetPosition = new Vector3(transform.position.x + 0.05f , transform.position.y + 0.05f, transform.position.z);

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
    }
}
