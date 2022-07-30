using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Carrier : MonoBehaviour
{
    //Used to transport information between scenes.
    //It is a prefab - and we directly modify it. This means its information is constant in the game.
    //A savegame file will also be used to fill this object. The whole gamestate can be contained in it.


    //some things it transports include:
    // -the reserve party. (who's in it, their stats, which of their traits are equipped/unlocked)
    // -the index of the next part to load (set after combat missions, by the mission, on victory)
    // -the index of the next combat mission to load (set after mission event, by the overworld, on click)
    // -a history of what decisions were made, etc

    private List<Unit> reserveParty;
    private int nextPartIndex;
    private int nextMissionIndex;


    //GETTERS
    public int get_nextPartIndex() { return nextPartIndex; }
    public int get_nextMissionIndex() { return nextMissionIndex; }
    public List<Unit> get_reserveParty() { return reserveParty; }

    //SETTERS
    public void set_nextPartIndex(int i) { nextPartIndex = i; }
    public void set_nextMissionIndex(int i) { nextMissionIndex = i; }

}
