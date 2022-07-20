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

    [SerializeField] private Image active_portrait;
    [SerializeField] private Sprite[] affSprites;
    [SerializeField] private Text nameText;
    [SerializeField] private Text unitTypesText;
    [SerializeField] private Image affImage;
    [SerializeField] private Text stats_1; //the main stats view. hp, brk, mvmt.
    [SerializeField] private Text stats_2; //the side stats view. patk, pdef, maga, magd

    [SerializeField] private Text traitName;
    [SerializeField] private Text traitdescr; //three lines.
    [SerializeField] private Button[] traitButtons; //four buttons.

    private Unit heldUnit; //the locked unit.
    public void set_heldUnit(Unit u)
    {
        heldUnit = u;
    }
    public Unit get_heldUnit() { return heldUnit; }

    private string[] unitTypeConverter = new string[2] { "Flying", "Amphib." };

    public void fill(Unit u, int pw, bool allowButtonsInteractable)
    {
        //set all fields with the unit's corresponding data

        //heldUnit is when the game enters player movement phase. That's when.
        if (u != null)
        {
            heldUnit = u;
            //portrait
            active_portrait.sprite = u.get_active_p();

            //main stats window
            string buildUnitTypeStr = "";
            foreach(Trait t in u.get_traitList())
            {
                //if not nothing, add.
                if (t != null && t.get_unitType() != UnitType.NOTHING)
                {
                    buildUnitTypeStr += " " + unitTypeConverter[(int)t.get_unitType() - 1] + ",";
                }               
            }
            nameText.text = u.get_unitName();
            unitTypesText.text = buildUnitTypeStr.TrimEnd(',');
            affImage.sprite = affSprites[u.get_aff()];

            stats_1.text = "HP: " + u.get_hp() + " / " + u.get_hpMax()
                + "\nBRK: " + u.get_brk() + " / " + u.get_brkMax()
                + "\nMovement: " + u.get_movement();

            stats_2.text = "Phys Atk: " + u.get_physa()
                + "\nPhys Def: " + u.get_physd()
                + "\nMag Atk: " + u.get_maga()
                + "\nMag Def: " + u.get_magd();

            //trait-abiltiy window
            for (int i = 0; i < traitButtons.Length; i++)
            {
                //fill trait-ability button text.
                if (u.get_traitList()[i] != null)
                {
                    if ( !allowButtonsInteractable || u.get_traitList()[i].get_isPassive() || u.get_traitList()[i].get_pwCost() > pw )
                    {
                        traitButtons[i].interactable = false;
                    }
                    else
                    {
                        traitButtons[i].interactable = true;
                    }
                        

                    traitButtons[i].gameObject.transform.GetChild(0).gameObject.GetComponent<Text>().text = "(" + (i+1) + ") " + u.get_traitList()[i].get_traitName();
                }
                else
                {
                    traitButtons[i].interactable = false;
                    traitButtons[i].gameObject.transform.GetChild(0).gameObject.GetComponent<Text>().text = "<i>Empty</i>";
                }
            }
            //fill title and descr text with element 0 - the locked one. Not moveable. Every unit in the game has a trait at this slot.
            traitName.text = u.get_traitList()[0].get_traitName();
            traitdescr.text = u.get_traitList()[0].get_traitDescr();


            if (!gameObject.active) gameObject.SetActive(true);
        }
    }
    public void traitButtonHover(int which)
    {
        //called when a trait button is hovered.
        //displays information.

        if (heldUnit != null && heldUnit.get_traitList()[which] != null)
        {
            traitName.text = heldUnit.get_traitList()[which].get_traitName();
            traitdescr.text = heldUnit.get_traitList()[which].get_traitDescr();
        }
        
    }

    public void hide()
    {
        gameObject.SetActive(false);
    }

}
