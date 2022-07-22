using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour
{
    //the tiles that the combatgrid is made out of.

    [SerializeField] private string tileName;
    [SerializeField] private int movementCost; //-1 for impassable.
    [SerializeField] private float coverReduction; //high means more dmg reduction. From 0 to 1.
    [SerializeField] private string descr;

    //for marking zone of control. start disabled.
    [SerializeField] private SpriteRenderer targetIcon;
    [SerializeField] private SpriteRenderer zocRenderer;

    //control markers. Restrict movement of the opposing side if true.
    public bool player_controlled { get; set; }
    public bool enemy_controlled { get; set; }

    public int x { get; set; }
    public int y { get; set; }
    Unit heldUnit;

    public bool isValid { get; set; }

    //State
    public virtual BaseOwnership get_ownership() { return BaseOwnership.NEUTRAL; }
    public virtual void set_ownerShip(BaseOwnership o) { }
    public void set_coords(int newX, int newY)
    {
        x = newX;
        y = newY;

        //(temporary)
        isValid = false;
    }
    public void place_unit(Unit u)
    {
        heldUnit = u;
    }
    public void remove_unit()
    {
        heldUnit = null;
    }
    public bool occupied()
    {
        if (heldUnit == null) return false;
        return true;
    }

    //Mouse
    void OnMouseEnter()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            //Debug.Log("mouse entered " + x + ", " + y);
            transform.parent.GetComponent<CombatGrid>().tile_hovered(x, y, heldUnit);
        }
    }
    void OnMouseDown()
    {
        //pass control to parent (combatGrid)
        //Debug.Log("clicked on " + x + ", " + y);

        //stops clicks from going through the ui canvas
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            transform.parent.GetComponent<CombatGrid>().tile_clicked(x, y, heldUnit);
        }

    }

    //Visuals
    public void highlight_target_mv()
    {
        targetIcon.color = new Color(0f, 0f, 1f);
        targetIcon.enabled = true;
    }
    public void highlight_target(bool isAttack)
    {
        //if attack; dark red
        //if not; dark green
        if (isAttack)
        {
            targetIcon.color = new Color(200f / 255f, 0f, 0f);
        }
        else 
        {
            targetIcon.color = new Color(0f, 200f / 255f, 0f);

        }

        targetIcon.enabled = true;
    }
    public void hide_target_icon()
    {
        targetIcon.enabled = false;
    }
    public void highlight_mv()
    {
        //the tile has been told by the combatGrid to highlight for movement.
        //set sheen to blue.
        gameObject.GetComponent<SpriteRenderer>().color = new Color(204f/255f, 255f/255f, 255f/255f);
    }
    public void highlight_deploy()
    {
        //the tile has been told by the combatGrid to highlight for movement.
        //set sheen to blue.
        gameObject.GetComponent<SpriteRenderer>().color = Color.yellow;
    }
    public void highlight_atk(bool isAttack)
    {
        //the tile has been told by the combatGrid to highlight for attack. red: attack, green: heal.
        if (isAttack) gameObject.GetComponent<SpriteRenderer>().color = new Color(255f / 255f, 105f / 255f, 97f / 255f);
        else gameObject.GetComponent<SpriteRenderer>().color = new Color(255f / 255f, 105f / 255f, 97f / 255f);
    }
    public void remove_highlight()
    {
        gameObject.GetComponent<SpriteRenderer>().color = new Color(255f / 255f, 255f / 255f, 255f / 255f);
    }
    public void set_ZoC_color()
    {
        //updates tiles ZoC images based on playerControlled
        //and enemyControlled
        if (player_controlled && enemy_controlled)
        {
            zocRenderer.color = new Color(111f / 255f, 0f, 161f / 255f);
            zocRenderer.enabled = true;
        }
        else if (player_controlled)
        {
            zocRenderer.color = new Color(32f / 255f, 201f / 255f, 1f);
            zocRenderer.enabled = true;
        }
        else if (enemy_controlled)
        {
            zocRenderer.color = Color.red;
            zocRenderer.enabled = true;
        }
        else
        {
            zocRenderer.enabled = false;
        }
    }

    //Getters
    public int get_movementCost() { return movementCost; }
    public string get_tileName() { return tileName; }
    public string get_descr() { return descr; }
    public float get_cover() { return coverReduction; }
    public Unit get_heldUnit() { return heldUnit; }
}
