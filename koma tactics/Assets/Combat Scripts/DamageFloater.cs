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
        
        float xOffset = Random.Range(transform.position.x * 0.975f, transform.position.x * 1.025f);
        float yOffset = Random.Range(transform.position.y * 0.975f, transform.position.y + 1.025f);

        transform.position = new Vector3(xOffset, yOffset, 0f);

        gameObject.GetComponent<MeshRenderer>().sortingLayerName = "Unit Layer";
        gameObject.GetComponent<MeshRenderer>().sortingOrder = 4;

        actualText.text = toShow;

        Destroy(gameObject, lifetime);
    }

}
