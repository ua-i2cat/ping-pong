// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IK_Solver
{
    public enum SolverId { CCD, FABRIK, FABRIK_Constrained };
    static public Dictionary<SolverId, Type> Solvers = new Dictionary<SolverId, Type>()
    {
        { SolverId.CCD, typeof(CCD_Solver) },
        { SolverId.FABRIK, typeof(FABRIK_Solver) },
        { SolverId.FABRIK_Constrained, typeof(FABRIK_Constrained_Solver) }
    };

    public abstract void Solve(Transform[] joints, Transform target, float threshold = 0.1f, Constrain_minmaxangle[] constraints = null);
}
