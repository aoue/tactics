using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BaseOwnership { NEUTRAL, PLAYER, ENEMY };
public class BaseTile : Tile
{
    //inherits from tile.
    //a base tile has some special properties though.

    //-a flag to mark who owns it.
    //-it adds power at the start of round, if player owned.
    //-it can be deployed from at the start of round, if player owned.

    [SerializeField] private int pwGeneration; //how much power it adds at start of round
    [SerializeField] private SpriteRenderer baseImage; //we need a ref to change color on capture
    [SerializeField] private BaseOwnership ownership; //starts false, naturally.

    public override void set_ownerShip(BaseOwnership o)
    {
        switch (o)
        {
            case BaseOwnership.NEUTRAL:
                baseImage.color = Color.white;
                break;
            case BaseOwnership.PLAYER:
                baseImage.color = Color.blue;
                break;
            case BaseOwnership.ENEMY:
                baseImage.color = Color.red;
                break;
        }
    }

    public int get_pwGeneration() { return pwGeneration; }
    public override BaseOwnership get_ownership() { return ownership; }

}
