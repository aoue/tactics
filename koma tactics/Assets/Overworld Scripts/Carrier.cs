using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private List<Unit> reserveParty;
    private int nextPartIndex; //set by combat mission when loading overworld.
    private int nextMissionIndex; //set by overworld when loading a combat mission.
    private bool startingNewGame; //set to true by main menu on the new game button. Otherwise, always false.

    private int gainedExp; //exp gained in a mission
    private int levelCap; //the max level a unit is allowed to achieve. Stops the player from overleveling.

    [SerializeField] private Unit[] allPlayerUnits; //the carrier knows all the units.

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
        nextMissionIndex = 0;
    }
    public void reset()
    {
        //set to the state that it has at the start of the game.
        //this works, because main menu sets startingNewGame to true. No where else can it be set to true.
        gainedExp = 0;
        nextPartIndex = 0;
        nextMissionIndex = 0;
        reserveParty = new List<Unit>();
    }

    //PARTY MODIFICATION
    public void add_to_party(int id)
    {
        if (!reserveParty.Contains( allPlayerUnits[id] ))
        {
            reserveParty.Add(allPlayerUnits[id]);

        }
    }
    public void remove_from_party(int id)
    {
        reserveParty.Remove(allPlayerUnits[id]);
    }

    //GETTERS
    public int get_nextPartIndex() { return nextPartIndex; }
    public int get_nextMissionIndex() { return nextMissionIndex; }
    public List<Unit> get_reserveParty() { return reserveParty; }  
    public int get_exp() { return gainedExp; }
    public int get_level_cap() { return levelCap; }

    //SETTERS
    public void set_level_cap(int i) { }
    public void set_exp(int i) { gainedExp = 0; }
    public void inc_exp(int i) { gainedExp += i; }
    public void set_nextPartIndex(int i) { nextPartIndex = i; }
    public void set_nextMissionIndex(int i) { nextMissionIndex = i; }

}
