using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CautionOrder : Order
{
    //this order multiplies physd and magd by 1.3x

    //called at the start of damage calculations in battlebrain to influence each individual stat.

    public override double order_physd(double physd)
    {
        return (physd / 1.3f);
    }
    public override double order_magd(double magd)
    {
        return (magd / 1.3f);
    }
}
