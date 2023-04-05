using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelTreeManager : MonoBehaviour
{
    // The level tree manager, which is used between missions. The player spends exp (seperate for each unit) to learn moves.
    // every time the unit learns a move, they also get a stat increase tied to the move learned.
    // still to-do:
    //  -moves locked/unlocked
    //  -save moves learned somewhere
    //  -draw lines between each equip button and its corresponding loadout slot

    [SerializeField] private Sprite unknownSwitchImage; // image for switch buttons when the unit in question has not been met.
    [SerializeField] private TextMeshProUGUI expText; // text that displays amount of exp
    [SerializeField] private TextMeshProUGUI expCostText; // text that displays amount of exp

    [SerializeField] private Button[] switchButtons; // the top buttons used to switch between units. Parallel to Carrier's allPlayerUnits (which is static.)
    [SerializeField] private LevelTree[] unitTrees; // array of all levelTrees for all units, which specify the unit's progression.
    [SerializeField] private Button[] levelTreeButtons; // the side panel buttons that hold the move from the unitTrees
    [SerializeField] private TextMeshProUGUI developmentTitle;

    [SerializeField] private CanvasGroup advancementPanelGroup; // the master object for the advancement panel.
    [SerializeField] private TextMeshProUGUI unitStatText;
    [SerializeField] private CanvasGroup advancementMoveGroup;
    [SerializeField] private TextMeshProUGUI moveTitleText;
    [SerializeField] private TextMeshProUGUI moveInfoText;
    [SerializeField] private TextMeshProUGUI moveAOEText;
    [SerializeField] private TextMeshProUGUI moveIncText;
    [SerializeField] private Image moveAOEImage;
    [SerializeField] private Button learnButton; // the button used to spend exp learning a move.
    [SerializeField] private Button[] equipButtons; // the buttons used to add a move to the active traitlist.
    [SerializeField] private TextMeshProUGUI[] loadoutTexts; // displays the equipped moves. Set by unit's traitlist.

    [SerializeField] private Image portraitSlot;

    // All the units are modified in the carrier.instance :)
    // we get the unit using currentUnitIndex into the carrier. This index is also parallel to the unitTrees.
    // the unit trees are locked to a unique unit id. This means that level tree 0 is always mc, button 1 is always friday, etc.
    
    private int currentUnitIndex;
    private LevelTree currentTree;
    private LevelTreeMove currentTreeMove;
    private int currentTreeMoveIndex;

    // SETUP AND SWITCHING
    public void load(int whichUnit)
    {
        // called by overworld HQ button. Opens level tree manager to unit 0 (mc)
        // mc is always available as a unit :)
        // even if a unit leaves the party, its spot in the switch bar will stay. It will just be disabled. :)
        currentUnitIndex = whichUnit;
        advancementMoveGroup.alpha = 0f;

        // enable switch buttons
        validateSwitchButtons();

        // load up level tree moves
        displayCurrentTree();

        // fade in with alpha; either directly on the canvas or with a group if necessary
        gameObject.SetActive(true);
    }
    public void close()
    {
        // called by close button.

        // fade with canvas group
        gameObject.SetActive(false);
    }  
    void displayCurrentTree()
    {
        // displays the current level tree of the current unit. 
        currentTree = unitTrees[currentUnitIndex];
        currentTreeMove = null;
        currentTreeMoveIndex = -5;

        // display the portrait of the current unit
        Unit u = Carrier.Instance.get_allUnitList()[currentUnitIndex];
        portraitSlot.sprite = u.get_active_p();
        fillUnitPanel();
        moveIncText.text = "";
        developmentTitle.text = "Development - " + u.get_unitName();


        // set the state of all the levelTreeButtons. They are one of:
        // -(temp) no move in slot: hide button
        // -learned. Button can be clicked.
        // -can_learn. Button can be clicked.
        // -cannot_learn. button cannot be clicked.
        // -unknown. button cannot be clicked (yet)
        for (int i = 0; i < levelTreeButtons.Length && i < currentTree.get_treeMoves().Length; i++)
        {
            // Debug.Log("the state of u.get_learnedList()[" + i + "] is " + u.get_learnedList()[i]);
            if (currentTree.get_treeMoves()[i] == null)
            {
                levelTreeButtons[i].gameObject.SetActive(false);
            }
            else
            {
                // the button corresponds to a move. Its learn status can be in three states:
                // -CAN_LEARN: the move can be can be clicked
                // -LEARNED: the move can be clicked
                // -CANNOT_LEARN and UNKNOWN: the move cannot be clicked
                levelTreeButtons[i].gameObject.SetActive(true);
                if(u.get_learnedList()[i] == moveLearnState.CANNOT_LEARN || u.get_learnedList()[i] == moveLearnState.UNKNOWN)
                {
                    levelTreeButtons[i].interactable = false;
                }
                else
                {
                    levelTreeButtons[i].interactable = true;
                }  
                levelTreeButtons[i].gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = currentTree.get_treeMoves()[i].get_attached_move().get_traitName();
            }
        }
        fill_loadout();
        // when the level tree is loaded, this slot is affected. 
        // Possibilities:
        //  -it has already been learned, so is equippable.
        //  -it is learnable, but has not been learned. 
        //  -its counterpart has been learend, so it is not learnable.
        //  -it has not yet been unlocked, so no information is displayed.
    }
    void fill_loadout()
    {
        // fill the loadout move slots
        // Carrier.Instance.get_allUnitList()[currentUnitIndex].get_traitList()[i].get_traitName();
        for(int i = 0; i < loadoutTexts.Length; i++)
        {
            if (Carrier.Instance.get_allUnitList()[currentUnitIndex].get_traitList()[i] == null)
            {
                
                loadoutTexts[i].text = "<i>Empty</i>";
            }
            else
            {
                loadoutTexts[i].text = Carrier.Instance.get_allUnitList()[currentUnitIndex].get_traitList()[i].get_traitName();
            }
        }
    }
    void validateSwitchButtons()
    {
        // future: enable progressively more buttons based on progress into the game.

        // enable all switch buttons, except for current unit one.
        for (int i = 0; i < switchButtons.Length; i++)
        {
            switch(Carrier.Instance.get_allUnitStates()[i])
            {
                case 0:
                    switchButtons[i].interactable = true;
                    break;
                case 1:
                    switchButtons[i].interactable = false;
                    break;
                default:
                    
                    switchButtons[i].gameObject.GetComponent<Image>().sprite = unknownSwitchImage;
                    switchButtons[i].interactable = false;
                    break;
            }   
        }
        switchButtons[currentUnitIndex].interactable = false;
    }
    public void click_switchButton(int which)
    {
        // do a fade between these screens

        currentUnitIndex = which;
        advancementMoveGroup.alpha = 0f;
        validateSwitchButtons();
        displayCurrentTree();
    }

    // LEVEL TREE
    public void levelTreeMove_clicked(int which)
    {
        // called when a levelTreeMove slot button is clicked.
        currentTreeMove = currentTree.get_treeMoves()[which];
        currentTreeMoveIndex = which;

        // fill advancement panel
        fillUnitPanel();
        fillMovePanel(currentTreeMove, currentTreeMoveIndex);       
    }
    public void levelTreeMove_hovered(int which)
    {
        // called when a levelTreeMove slot button is hovered.
        // temporarily fills the move panel with the info from this move.
        if (currentTree.get_treeMoves()[which] != null)
        {
            fillMovePanel(currentTree.get_treeMoves()[which], which);
        }
    }
    public void levelTreeMove_unhovered()
    {
        // called when a levelTreeMove slot button is unhovered.
        // fills the move panel with the info from the held move.
        if (currentTreeMove != null)
        {
            fillMovePanel(currentTreeMove, currentTreeMoveIndex);
        }       
    }
    public void equippedMoveButton_hovered(int which)
    {
        // when one of the equipped move buttons is hovered, it should display the move's information.
        Trait move = Carrier.Instance.get_allUnitList()[currentUnitIndex].get_traitList()[which];
        if (move != null)
        {
            moveIncText.text = "";

            // fill move panel:
            // move name, move descr
            moveTitleText.text = move.get_traitName();
            moveInfoText.text = move.get_traitDescr();

            if (!move.get_isPassive())
            {
                moveAOEImage.sprite = Carrier.Instance.get_targetingSprites()[(int)move.get_targetingType()];
                moveAOEImage.enabled = true;
                moveAOEText.text = "aoe text";

                moveAOEText.text = Carrier.Instance.get_AoELabels()[(int)move.get_AoEType()];
                if (move.get_minimum_range() == 0)
                {
                    moveAOEText.text += "\n" + move.get_range() + " Range";
                }
                else
                {
                    moveAOEText.text += "\n" + move.get_minimum_range() + "-" + move.get_range() + " Range";
                }
            }
            else
            {
                moveAOEImage.enabled = false;
                moveAOEText.text = "";
            }
            // disable learn button and equip buttons
            expCostText.text = "Already Learned";
            learnButton.interactable = false;
            foreach(Button eqButton in equipButtons)
            {
                eqButton.interactable = false;
            }
            advancementMoveGroup.alpha = 1f;
        }
    }
    void displayExp()
    {
        expText.text = "unspent exp: " + Carrier.Instance.get_allUnitList()[currentUnitIndex].get_exp();
    }

    // ADVANCEMENT PANEL
    void fillUnitPanel()
    {
        // called when a levelTreeMove button is clicked.
        // info that needs to be displayed:
        //  unit:
        //   -name, hpmax, brkmax, pa, pd, ma, md, mv, class str
        //  move:
        //   -move title, move descr, move aoe diagram
        //  learn:
        //   -exp cost, stat increase information

        Unit u = Carrier.Instance.get_allUnitList()[currentUnitIndex];

        HashSet<UnitType> addedTypes = new HashSet<UnitType>();
        string buildUnitTypeStr = Carrier.Instance.get_affConverter()[u.get_aff()];
        foreach(Trait t in u.get_traitList())
        {
            //if not nothing, and not already added, add.             
            if (t != null && t.get_unitType() != UnitType.NOTHING && !addedTypes.Contains(t.get_unitType()))
            {
                addedTypes.Add(t.get_unitType());
                buildUnitTypeStr += "-" + Carrier.Instance.get_unitTypeConverter()[(int)t.get_unitType() - 1];
            }
        }

        unitStatText.text = "HP: " +  u.get_hpMax()
                + "\nBRK: " + u.get_brkMax()
                + "\nPhys Atk: " + u.get_physa()
                + "\nPhys Def: " + (100f - (u.get_physd() * 100f))
                + "%\nEAC Atk: " + u.get_maga()
                + "\nEAC Def: " + (100f - (u.get_magd() * 100f))
                + "%\n\n" + buildUnitTypeStr;
        displayExp();
    }
    void fillMovePanel(LevelTreeMove treeSlot, int which)
    {
        moveTitleText.text = treeSlot.get_attached_move().get_traitName();
        moveInfoText.text = treeSlot.get_attached_move().get_traitDescr();

        if (!treeSlot.get_attached_move().get_isPassive())
        {
            moveAOEImage.sprite = Carrier.Instance.get_targetingSprites()[(int)treeSlot.get_attached_move().get_targetingType()];
            moveAOEImage.enabled = true;
            moveAOEText.text = "aoe text";

            moveAOEText.text = Carrier.Instance.get_AoELabels()[(int)treeSlot.get_attached_move().get_AoEType()];
            if (treeSlot.get_attached_move().get_minimum_range() == 0)
            {
                moveAOEText.text += "\n" + treeSlot.get_attached_move().get_range() + " Range";
            }
            else
            {
                moveAOEText.text += "\n" + treeSlot.get_attached_move().get_minimum_range() + "-" + treeSlot.get_attached_move().get_range() + " Range";
            }
        }
        else
        {
            moveAOEImage.enabled = false;
            moveAOEText.text = "";
        }
        
        Unit u = Carrier.Instance.get_allUnitList()[currentUnitIndex];
        switch(u.get_learnedList()[which])
        {
            case moveLearnState.UNKNOWN:
                expCostText.text = "Learn for " + currentTree.get_treeMoves()[which].get_learnExpCost() + " EXP";
                moveIncText.text = treeSlot.get_increases_string();
                learnButton.interactable = false;
                foreach(Button eqButton in equipButtons)
                {
                    eqButton.interactable = false;
                }
                break;
            case moveLearnState.CAN_LEARN:
                expCostText.text = "Learn for " + currentTree.get_treeMoves()[which].get_learnExpCost() + " EXP";
                moveIncText.text = treeSlot.get_increases_string();
                if (treeSlot.get_learnExpCost() <= u.get_exp()) learnButton.interactable = true;
                foreach(Button eqButton in equipButtons)
                {
                    eqButton.interactable = false;
                }
                break;
            case moveLearnState.LEARNED:
                expCostText.text = "Already Learned";
                moveIncText.text = "";
                learnButton.interactable = false;
                foreach(Button eqButton in equipButtons)
                {
                    eqButton.interactable = true;
                }
                break;
            case moveLearnState.CANNOT_LEARN:
                expCostText.text = "Cannot Learn";
                moveIncText.text = "";
                learnButton.interactable = false;
                foreach(Button eqButton in equipButtons)
                {
                    eqButton.interactable = false;
                }
                break;
        }
        advancementMoveGroup.alpha = 1f;
    }
    public void click_learn_button()
    {
        // called when the learn button is clicked.
        Unit u = Carrier.Instance.get_allUnitList()[currentUnitIndex];

        // decrement exp
        u.set_exp(u.get_exp() - currentTreeMove.get_learnExpCost());
        displayExp();

        // adjust the known moves list
        // -set current move to LEARNED
        // -set its rowmate to CANNOT_LEARN (it's rowmate will either be 1 greater or 1 lower index than it.)
        //  0 and 1 are rowmates, 2 and 3 are rowmates, etc...
        //  so if the index is even, then rowmate is one greater; if odd, one lower
        u.get_learnedList()[currentTreeMoveIndex] = moveLearnState.LEARNED;
        int rowmateIndex;
        if (currentTreeMoveIndex % 2 == 0) rowmateIndex = currentTreeMoveIndex + 1;
        else rowmateIndex = currentTreeMoveIndex - 1;
        u.get_learnedList()[rowmateIndex] = moveLearnState.CANNOT_LEARN;
        levelTreeButtons[rowmateIndex].interactable = false;

        // set the next move pair to can_learn and enable their level tree buttons
        u.get_learnedList()[currentTreeMoveIndex + 2] = moveLearnState.CAN_LEARN;
        u.get_learnedList()[rowmateIndex + 2] = moveLearnState.CAN_LEARN;
        levelTreeButtons[currentTreeMoveIndex + 2].interactable = true;
        levelTreeButtons[rowmateIndex + 2].interactable = true;

        // increment the unit's stats
        currentTreeMove.learn_increases(u);

        // adjust level tree buttons, etc.
        fillUnitPanel();
        fillMovePanel(currentTreeMove, currentTreeMoveIndex);
    }
    public void click_equipButton(int which)
    {
        // called when one of the equip buttons is clicked.
        // does one of two possible things:
        // 1. if the move is not already equipped, then equips the move to the unit's moveList[which]
        // 2. if the move is already equipped, then swap the indices in the unit's moveList with the move at which.
        Debug.Log("equip button clicked, which = " + which);

        Unit u = Carrier.Instance.get_allUnitList()[currentUnitIndex];
        Trait toEquip = currentTreeMove.get_attached_move();

        // cycle through, remove the move if it is equipped in any other slot, and put it in slot [which]
        for(int i = 0; i < u.get_traitList().Length; i++)
        {
            if (i == which)
            {
                u.get_traitList()[i] = toEquip;
            }
            else if (u.get_traitList()[i] == toEquip)
            {
                u.get_traitList()[i] = null;
            }
        }

        // refresh visuals
        fill_loadout();
    }
}
