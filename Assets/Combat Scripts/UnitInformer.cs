using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitInformer : MonoBehaviour
{
    //previewer for units, enemy and allied alike.

    //click on a unit: heldUnit. When you are not mousing over a unit, it will show their stats.
    //hover over a unit: display this unit's information.

    //held data
    [SerializeField] private Sprite[] affSprites;
    
    //main display
    [SerializeField] private Image active_portrait;    
    [SerializeField] private Text nameText;
    [SerializeField] private Text unitTypesText;
    [SerializeField] private Image affImage;
    [SerializeField] private Text stats_1; //the main stats view. hp, brk, mvmt.

    //side display
    [SerializeField] private Text stats_2; //the side stats view. patk, pdef, maga, magd
    [SerializeField] private Image targetingImage; //from Targeting type
    [SerializeField] private Text targetingLabel; //from AoE Type
    [SerializeField] private Text traitName;
    [SerializeField] private Text traitdescr; //three lines.
    [SerializeField] private Button[] traitButtons; //four buttons.
    [SerializeField] private Button passButton; 

    private Unit heldUnit; //the locked unit.
    public void set_heldUnit(Unit u)
    {
        heldUnit = u;
    }
    public Unit get_heldUnit() { return heldUnit; }
   
    public void set_pass(bool s)
    {
        if (s)
        {
            passButton.interactable = true;
            passButton.gameObject.SetActive(true);
        }
        else
        {
            passButton.gameObject.SetActive(false);
        }
    }
    public void refresh()
    {
        if (heldUnit != null)
        {
            fill(heldUnit, -1, false);
        }
    }
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
            string buildUnitTypeStr = Carrier.Instance.get_affConverter()[u.get_aff()];
            
            HashSet<UnitType> addedTypes = new HashSet<UnitType>();
            foreach(Trait t in u.get_traitList())
            {
                //if not nothing, and not already added, add.             
                if (t != null && t.get_unitType() != UnitType.NOTHING && !addedTypes.Contains(t.get_unitType()))
                {
                    addedTypes.Add(t.get_unitType());
                    buildUnitTypeStr += "-" + Carrier.Instance.get_unitTypeConverter()[(int)t.get_unitType() - 1];
                }               
            }
            nameText.text = u.get_unitName();
            unitTypesText.text = buildUnitTypeStr;
            affImage.sprite = affSprites[u.get_aff()];

            stats_1.text = "HP: " + u.get_hp() + " / " + u.get_hpMax()
                + "\nBRK: " + u.get_brk() + " / " + u.get_brkMax()
                + "\nMovement: " + u.get_movement()
                + "\nControl: " + u.get_controlRange();

            if (u.get_isAlly())
            {
                if (u.get_unitOrder() != null)
                {
                    stats_1.text += "\n\nOrder—" + u.get_unitOrder().get_orderName()
                    + "\n<i>" + u.get_unitOrder().get_orderDescr() + "</i>";
                }
            }
            else
            {
                if (u.get_act_delay() > 0)
                {
                    stats_1.text += "\n\nStartup in —<i>" + u.get_act_delay() + " rounds</i>";
                }
            }

            stats_2.text = "Phys Atk: " + u.get_physa()
                + "\nPhys Def: " + (100f - (u.get_physd() * 100f))
                + "%\nEAC Atk: " + u.get_maga()
                + "\nEAC Def: " + (100f - (u.get_magd() * 100f)) + "%";

            active_portrait.gameObject.SetActive(true);

            //trait-abiltiy window           
            for (int i = 0; i < traitButtons.Length; i++)
            {               
                //fill trait-ability button text.
                if (u.get_traitList()[i] != null)
                {
                    //Debug.Log("uinformer.fill() trait " + i + " pw =  " + pw + " pwCost = " + u.get_traitList()[i].get_pwCost());
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
            //-must be active trait, must have 0 pw cost, etc. Locked.
            traitButtonHover(0);

            if (!gameObject.activeSelf) gameObject.SetActive(true);
        }
    }
    public void traitButtonHover(int which)
    {
        //called when a trait button is hovered.
        //displays information.

        if (heldUnit != null && heldUnit.get_traitList()[which] != null)
        {
            //set trait name and descr
            traitName.text = heldUnit.get_traitList()[which].get_traitName();
            traitdescr.text = heldUnit.get_traitList()[which].get_traitDescr();

            //set trait targeting diagram

            if (heldUnit.get_traitList()[which].get_isPassive())
            {
                targetingLabel.text = "";
                targetingImage.enabled = false;
            }
            else
            {
                targetingImage.sprite = Carrier.Instance.get_targetingSprites()[(int)heldUnit.get_traitList()[which].get_targetingType()];
                targetingLabel.text = Carrier.Instance.get_AoELabels()[(int)heldUnit.get_traitList()[which].get_AoEType()];

                if (heldUnit.get_traitList()[which].get_minimum_range() == 0)
                {
                    targetingLabel.text += "\n" + heldUnit.get_traitList()[which].get_range() + " Range";
                }
                else
                {
                    targetingLabel.text += "\n" + heldUnit.get_traitList()[which].get_minimum_range() + "-" + heldUnit.get_traitList()[which].get_range() + " Range";
                }

                targetingImage.enabled = true;
            }
        }
    }
    public void hide()
    {
        gameObject.SetActive(false);
        active_portrait.gameObject.SetActive(false);
    }

}
