using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TraitFloater : MonoBehaviour
{
    // create to show move icons on the field.
    // useful to account for the lack of animation.
    // we show the move icon on the map near the unit, implying the unit just used it.

    // it fades in with the attack
    // lingers for the duration
    // and fades out as the attack ends, deleting itself afterwards.

    [SerializeField] private SpriteRenderer renderer;

    public void setup(Sprite toShow, float lifetime)
    {       
        renderer.sprite = toShow;
        Vector2 spawn = UnityEngine.Random.insideUnitCircle / 2;
        transform.position = new Vector2 (transform.position.x + spawn.x, transform.position.y + spawn.y);
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

        Vector3 dest = UnityEngine.Random.insideUnitCircle * 1.5f;
        Vector3 targetPosition = new Vector3(transform.position.x + dest.x, transform.position.y + dest.y, transform.position.z);

        float elapsedTime = 0f;
        while (elapsedTime < lifetime)
        {
            //fade in
            float value = elapsedTime / lifetime;
            Color inc_light = new Color(1f, 1f, 1f, value);
            renderer.color = inc_light;

            elapsedTime += Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime);
            yield return null;
        }
        elapsedTime = 0f;
        while (elapsedTime < lifetime/2)
        {
            //fade out
            float value = 1f - (elapsedTime / lifetime*2);
            Color inc_dark = new Color(1f, 1f, 1f, value);
            renderer.color = inc_dark;

            elapsedTime += Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime);
            yield return null;
        }

        Destroy(gameObject);
    }
    

}
