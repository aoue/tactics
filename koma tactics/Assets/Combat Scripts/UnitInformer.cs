using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitInformer : MonoBehaviour
{
    //previewer for units, enemy and allied alike.

    //click on a unit: heldUnit. When you are not mousing over a unit, it will show their stats.
    //hover over a unit: display this unit's information.

    //it has:
    // -name text (text)
    // -affinity image
    // -hp text and slider
    // -brk text and slider
    // -mvmt text
    // -4 trait slots
    // interactable:
    // -expand/shrink toggle
    //    -window with: (physa, physd, maga, magd)

    [SerializeField] private Sprite[] affSprites;
    [SerializeField] private Text nameText;
    [SerializeField] private Image affImage;
    [SerializeField] private Text stats_1; //the main stats view. hp, brk, mvmt.
    [SerializeField] private Text stats_2; //the side stats view. patk, pdef, maga, magd

    public void fill(Unit u)
    {
        //set all fields with the unit's corresponding data

        if (u == null) { hide(); }
        else
        {
            nameText.text = u.get_unitName();
            affImage.sprite = affSprites[u.get_aff()];

            stats_1.text = "HP: " + u.get_hp() + " / " + u.get_hpMax()
                + "\nBRK: " + u.get_brk() + " / " + u.get_brkMax()
                + "\nMovement: " + u.get_movement();

            stats_2.text = "Phys Atk: " + u.get_physa()
                + "\nPhys Def: " + u.get_physd()
                + "\nMag Atk: " + u.get_maga()
                + "\nMag Def: " + u.get_magd();

            if (!gameObject.active) gameObject.SetActive(true);
        }

    }

    public void hide()
    {
        gameObject.SetActive(false);
    }

}
