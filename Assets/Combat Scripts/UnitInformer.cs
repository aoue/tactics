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
    [SerializeField] private Sprite emptyTraitIconSprite; 
    
    //main display
    [SerializeField] private Image active_portrait;
    [SerializeField] private Text nameText;
    [SerializeField] private Text unitTypesText;
    [SerializeField] private Text stats_1; //the first column of main stats view; hp, brk, mvmt, control.
    [SerializeField] private Text stats_2; //the second column of main stats view; patk, pdef, maga, magd

    [SerializeField] private Image order_highlight; //the bg behind the order text that turns on to draw player attention that this action will be setting the order.
    [SerializeField] private Text order_text; //the third column of main stats view; order text

    //side display 
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
    
    public bool is_traitButton_interactable(int which)
    {
        if (traitButtons[which].interactable == true )
        {
            return true;
        }
        return false;
    }
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
            fill(heldUnit, -1, false, false);
        }
    }
    public void fill(Unit u, int pw, bool allowButtonsInteractable, bool set_order)
    {
        //set all fields with the unit's corresponding data
        // if unit is a player unit and orderSet is false, then highlight the bg behind the player unit's order text to remind them that they will trigger an order.

        if (set_order) { order_highlight.enabled = true; }
        else { order_highlight.enabled = false; }

        //heldUnit is when the game enters player movement phase. That's when.
        if (u != null)
        {
            heldUnit = u;
            //portrait
            active_portrait.sprite = u.get_active_p();

            //main stats window
            string buildUnitTypeStr = "CLASSES\n";
            
            HashSet<UnitType> addedTypes = new HashSet<UnitType>();
            for(int i = 0; i < u.get_traitList().Length; i++)
            {
                //if not nothing, and not already added, add.             
                if (u.get_traitList()[i] != null && u.get_traitList()[i].get_unitType() != UnitType.NOTHING && !addedTypes.Contains(u.get_traitList()[i].get_unitType()))
                {
                    addedTypes.Add(u.get_traitList()[i].get_unitType());
                    buildUnitTypeStr += Carrier.Instance.get_unitTypeConverter()[(int)u.get_traitList()[i].get_unitType() - 1];
                    if (i != u.get_traitList().Length - 1)
                    {
                        buildUnitTypeStr += "-";
                    }
                }               
                
            }
            nameText.text = u.get_unitName();
            unitTypesText.text = buildUnitTypeStr;

            string defString = u.get_physd().ToString();
            string magdString = u.get_magd().ToString();
            if (u.get_isBroken()) { defString = "BRK"; magdString = "BRK"; }

            stats_1.text = "VITAL." + u.get_hp() + "/" + u.get_hpMax()
                + "\nBRK." + u.get_brk() + "/" + u.get_brkMax()
                + "\nMVMT." + u.get_movement()
                + "\nCTRL." + u.get_controlRange()
                + "\nBLOCK." + defString
                + "\nRESIS." + magdString;
            stats_2.text = "";

            if (u.get_isAlly())
            {
                if (u.get_unitOrder() != null)
                {
                    order_text.text = u.get_unitOrder().get_orderName()
                    + "\n<i>" + u.get_unitOrder().get_orderDescr() + "</i>";
                }
            }
            else
            {
                order_highlight.enabled = false;
                if (u.get_act_delay() > 0)
                {
                    order_text.text = "Ready in —<i>" + u.get_act_delay() + " rounds</i>";
                }
                else order_text.text = "";
            }
            active_portrait.gameObject.SetActive(true);

            //trait-abiltiy window
            for (int i = 0; i < traitButtons.Length; i++)
            {               
                //fill trait-ability button text.
                if (u.get_traitList()[i] != null)
                {
                    //Debug.Log("uinformer.fill() trait " + i + " pw =  " + pw + " pwCost = " + u.get_traitList()[i].get_pwCost());
                    if ( !allowButtonsInteractable || u.get_traitList()[i].get_isPassive() || u.get_traitList()[i].get_pwCost() > pw || (u.get_traitList()[i].get_mustSetup() && u.get_hasMoved()) )
                    {
                        traitButtons[i].interactable = false;
                    }
                    else
                    {
                        traitButtons[i].interactable = true;
                    }
                    //set text and move icon
                    traitButtons[i].gameObject.transform.GetChild(1).gameObject.GetComponent<Text>().text = "(" + (i+1) + ")\n" + u.get_traitList()[i].get_traitName();
                    traitButtons[i].gameObject.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = u.get_traitList()[i].get_traitIconSprite();
                    traitButtons[i].gameObject.transform.GetChild(0).gameObject.SetActive(true);
                }
                else
                {
                    //disable button, set text to empty, set move icon to empty
                    traitButtons[i].interactable = false;
                    traitButtons[i].gameObject.transform.GetChild(1).gameObject.GetComponent<Text>().text = "";
                    traitButtons[i].gameObject.transform.GetChild(0).gameObject.SetActive(false);
                    //traitButtons[i].gameObject.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = emptyTraitIconSprite;
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
