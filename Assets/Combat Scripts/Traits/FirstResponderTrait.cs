using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FirstResponderTrait : Trait
{
    //increases movement if not within 3(?) range of a target at turn start.
    [SerializeField] private int moveBonus;
    public override int modify_movement_atStart(int move, Unit self, Unit[] self_allies, Unit[] targets)
    {
        //calc min dist to targeet (overwrite at -1)
        //if min dist is ever <= 3, return move
        //otherwise once we escape loop, return move + 1

        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] != null)
            {
                int dist = Math.Abs(self.x - targets[i].x) + Math.Abs(self.y - targets[i].y);
                if (dist <= 3)
                {
                    return move;
                }           
            } 
        }
        return move + moveBonus;
    }

}
