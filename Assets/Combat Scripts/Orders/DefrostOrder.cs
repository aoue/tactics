using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefrostOrder : Order
{
    public override int order_maga(int maga)
    {
        // defrost protocol. adds +1 HAC to attacks.
        return maga + 1;
    }
}
