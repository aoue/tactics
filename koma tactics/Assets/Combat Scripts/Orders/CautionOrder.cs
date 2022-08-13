using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CautionOrder : Order
{
    //this order multiplies physd and magd by 1.3x

    //called at the start of damage calculations in battlebrain to influence each individual stat.

    public override int order_physd(int physd)
    {
        return (int)(physd * 1.3f);
    }
    public override int order_magd(int magd)
    {
        return (int)(magd * 1.3f);
    }
}
