using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour
{
    //the tiles that the combatgrid is made out of.

    [SerializeField] private string tileName;
    [SerializeField] private int movementCost; //-1 for impassable.
    [SerializeField] private string descr;

    //for marking zone of control. start disabled.
    [SerializeField] private SpriteRenderer pZoC;
    [SerializeField] private SpriteRenderer eZoC;

    //control markers. Restrict movement of the opposing side if true.
    public bool player_controlled { get; set; }
    public bool enemy_controlled { get; set; }

    public int x { get; set; }
    public int y { get; set; }
    Unit heldUnit;

    public bool isValid { get; set; }

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
    public void highlight_mv()
    {
        //the tile has been told by the combatGrid to highlight for movement.
        //set sheen to blue.
        gameObject.GetComponent<SpriteRenderer>().color = new Color(204f/255f, 255f/255f, 255f/255f);
    }
    public void remove_highlight()
    {
        gameObject.GetComponent<SpriteRenderer>().color = new Color(255f / 255f, 255f / 255f, 255f / 255f);
    }

    public void set_ZoC_color()
    {
        //updates tiles ZoC images based on playerControlled
        //and enemyControlled
        if (player_controlled) pZoC.enabled = true;
        else pZoC.enabled = false;
        if (enemy_controlled) eZoC.enabled = true;
        else eZoC.enabled = false;
    }

    public int get_movementCost() { return movementCost; }
    public string get_tileName() { return tileName; }
    public string get_descr() { return descr; }

}
