using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTree : MonoBehaviour
{
    // Holds all the information about how a unit levels up, including:
    //  -stat increases at each level (stored in a virtual function)
    //  -moves learnable at each level, ordered vertically.
    //  -
    // Information is stored here and retrieved by the level tree manager.

    
    [SerializeField] private LevelTreeMove[] treeMoves;
    // -move
    // -cost
    // -stat increases


    public LevelTreeMove[] get_treeMoves() { return treeMoves; }

}
