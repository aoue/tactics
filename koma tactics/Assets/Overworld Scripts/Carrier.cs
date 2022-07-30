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

    public void new_game()
    {
        //called by main menu
        startingNewGame = true;
    }

    public void reset()
    {
        //set to the state that it has at the start of the game.
        //this works, because main menu sets startingNewGame to true. No where else can it be set to true.
        startingNewGame = false;
        nextPartIndex = 0;
        nextMissionIndex = 0;
        reserveParty = new List<Unit>();
    }

    //GETTERS
    public bool get_startingNewGame() { return startingNewGame; }
    public int get_nextPartIndex() { return nextPartIndex; }
    public int get_nextMissionIndex() { return nextMissionIndex; }
    public List<Unit> get_reserveParty() { return reserveParty; }

    //SETTERS
    public void set_nextPartIndex(int i) { nextPartIndex = i; }
    public void set_nextMissionIndex(int i) { nextMissionIndex = i; }

}
