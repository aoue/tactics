using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileInformer : MonoBehaviour
{
    [SerializeField] private Text titleText;
    [SerializeField] private Text descrText;

    public void fill(Tile t)
    {
        //display information based on the tile given.

        //Show:
        // -name
        // -movement cost
        // -description?
        titleText.text = t.get_tileName();
        descrText.text = "Movement cost: " + t.get_movementCost() + ".\n" + t.get_descr();

        if (!gameObject.active) gameObject.SetActive(true);
    }
}
