﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AggressionOrder : Order
{
    //called at the start of damage calculations in battlebrain to influence each individual stat.

    public override int order_physa(int physa)
    {
        return physa + 1;
    }
}
