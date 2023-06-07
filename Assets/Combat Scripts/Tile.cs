using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour
{
    //the tiles that the combatgrid is made out of.

    [SerializeField] private string tileName;
    [SerializeField] private bool blocksAttacks;
    [SerializeField] private bool canBeTargeted;
    [SerializeField] private int movementCost; //-1 for impassable.
    [SerializeField] private int coverBonus; //increase to def and ice for unit on this tile.
    [SerializeField] private string descr;

    //for highlight and marking zoc, targeting, etc. start disabled.
    [SerializeField] private SpriteRenderer highlightLayer; //used to highlight the tile either light blue (for movement), green (for heal move), or red (for attack move)
    private float highlightAlpha; 
    [SerializeField] private SpriteRenderer targetIcon;
    [SerializeField] private SpriteRenderer zocRenderer;
    [SerializeField] private SpriteRenderer debrisRenderer;

    //control markers. Restrict movement of the opposing side if true.
    public bool player_controlled { get; set; }
    public bool enemy_controlled { get; set; }

    public int x { get; set; }
    public int y { get; set; }
    Unit heldUnit;

    public bool isValid { get; set; }
    public List<Tile> path { get; set; } //used for tracing paths during movement generation.

    void Start()
    {
        highlightAlpha = highlightLayer.color.a;
    }

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
    public void destroy_unit()
    {
        //called when a unit dies on the tile. Shows machine debris on the tile.
        //Note: for better immersion, etc, have several random debris images to choose from.
        //and every time a unit is destroyed on the same tile, randomly enable another debris image.
        debrisRenderer.enabled = true;
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
        highlightLayer.color = new Color(0f, 1f, 1f, highlightAlpha);
        highlightLayer.enabled = true;
    }
    public void highlight_deploy()
    {
        //the tile has been told by the combatGrid to highlight for movement.
        //set sheen to blue.
        Color setMe = Color.yellow;
        setMe.a = highlightAlpha;
        highlightLayer.color = setMe;
        highlightLayer.enabled = true;
    }
    public void highlight_atk(bool isAttack)
    {
        //the tile has been told by the combatGrid to highlight for attack. red: attack, green: heal.
        if (isAttack) highlightLayer.color = new Color(255f / 255f, 105f / 255f, 97f / 255f, highlightAlpha);
        else highlightLayer.color = new Color(110f / 255f, 1f, 46f / 255f, highlightAlpha);
        highlightLayer.enabled = true;
    }
    public void remove_highlight()
    {
        highlightLayer.enabled = false;
    }
    public void set_ZoC_color()
    {
        //updates tiles ZoC images based on playerControlled
        //and enemyControlled

        //the ZoC colour directly below a unit is always that unit's side's colour. Makes it easier for the player to see.
        if (occupied())
        {
            if (heldUnit.get_isAlly())
            {
                //blue
                zocRenderer.color = new Color(32f / 255f, 201f / 255f, 1f);
                zocRenderer.enabled = true;
            }
            else
            {
                //red
                zocRenderer.color = Color.red;
                zocRenderer.enabled = true;
            }
            return;
        }

        if (player_controlled && enemy_controlled)
        {
            //purple
            zocRenderer.color = new Color(71f / 255f, 0f, 128f / 255f);
            zocRenderer.enabled = true;
        }
        else if (player_controlled)
        {
            //blue
            zocRenderer.color = new Color(32f / 255f, 201f / 255f, 1f);
            zocRenderer.enabled = true;
        }
        else if (enemy_controlled)
        {
            //red
            zocRenderer.color = Color.red;
            zocRenderer.enabled = true;
        }
        else
        {
            zocRenderer.enabled = false;
        }
    }

    //Getters
    public bool get_canBeTargeted() { return canBeTargeted; }
    public bool get_blocksAttacks() { return blocksAttacks; }
    public int get_movementCost() { return movementCost; }
    public string get_tileName() { return tileName; }
    public string get_descr() { return descr; }
    public int get_cover() { return coverBonus; }
    public Unit get_heldUnit() { return heldUnit; }
}
