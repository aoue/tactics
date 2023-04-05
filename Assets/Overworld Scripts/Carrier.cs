using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum UnitPresence { PRESENT, GONE, UNKNOWN };
public class Carrier : MonoBehaviour
{
    //Used to transport information between scenes.
    //It is a prefab - and we directly modify it. This means its information is constant in the game.
    //A savegame file will also be used to fill this object. The whole gamestate can be contained in it.
    private static Carrier _instance;
    public static Carrier Instance { get { return _instance; } }

    //some things it transports include:
    // -the reserve party. (who's in it, their stats, which of their traits are equipped/unlocked)
    // -the index of the next part to load (set after combat missions, by the mission, on victory)
    // -the index of the next combat mission to load (set after mission event, by the overworld, on click)
    // -a history of what decisions were made, etc

    private int nextPartIndex; //set by combat mission when loading overworld.
    private int nextMissionIndex; //set by overworld when loading a combat mission.
    private bool startingNewGame; //set to true by main menu on the new game button. Otherwise, always false.

    private int gainedExp; //exp gained in a mission

    [SerializeField] private Unit[] allPlayerUnits; //the carrier knows all the units.
    //controls which units are allowed to be edited in the level tree. Parallel to allPlayerUnits.
    // 0: can click
    // 1: show picture, but cannot click
    // 2: never met, don't show picture
    private int[] allPlayerUnitsStates; 

    [SerializeField] private Sprite[] targetingSprites; //order: line, square, radius, self

    private string[] AoELabels = {"Single", "All-between", "All", "Adjacent-Four"}; //order: single, all betweeen, all
    private string[] unitTypeConverter = new string[1] { "Creature" };
    private string[] affConverter = new string[3] { "Agile", "Balanced", "Heavy" };

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this);
        }
    }

    //GAMESTATE
    public void new_game()
    {
        //called by main menu
        gainedExp = 0;
        nextPartIndex = 0;
        nextMissionIndex = 0;

        //setup units into their initial positions.
        allPlayerUnitsStates = new int[3];        
    }
    public void distribute_exp()
    {
        //distributes exp to all units
        for(int i = 0; i < allPlayerUnits.Length; i++)
        {
            allPlayerUnits[i].set_exp(allPlayerUnits[i].get_exp() + gainedExp);
        }
        gainedExp = 0;
    }

    //GETTERS
    public int get_nextPartIndex() { return nextPartIndex; }
    public int get_nextMissionIndex() { return nextMissionIndex; }
    public Unit[] get_allUnitList() {return allPlayerUnits;}
    public int[] get_allUnitStates() {return allPlayerUnitsStates;}
    public int get_exp() { return gainedExp; }
    public string[] get_AoELabels() { return AoELabels; }
    public string[] get_unitTypeConverter() { return unitTypeConverter; }
    public string[] get_affConverter() { return affConverter; }
    public Sprite[] get_targetingSprites() { return targetingSprites; }

    //SETTERS
    public void set_level_cap(int i) { }
    public void set_exp(int i) { gainedExp = 0; }
    public void inc_exp(int i) { gainedExp += i; }
    public void set_nextPartIndex(int i) { nextPartIndex = i; }
    public void set_nextMissionIndex(int i) { nextMissionIndex = i; }
    

}
