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

        if ( t.get_movementCost() < 0 ) descrText.text = "Impassable";
        else descrText.text = "Movement cost: " + t.get_movementCost();


        descrText.text +=".\n" + (t.get_cover() * 100) 
            + "% Cover.\n" + t.get_descr();

        if (!gameObject.activeSelf) gameObject.SetActive(true);
    }
    public void hide()
    {
        gameObject.SetActive(false);
    }
}
