// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointConstraint : MonoBehaviour
{
    public enum ConstrainerType { Constrainer_angle, Constrainer_minmaxangle, Constrainer_plane, Constrainer_minmaxangle_plane, Constrainer_twist };
    static public Dictionary<ConstrainerType, Type> Constrainers = new Dictionary<ConstrainerType, Type>()
    {
        { ConstrainerType.Constrainer_angle, typeof(Constrainer_angle) },
        { ConstrainerType.Constrainer_plane, typeof(Constrainer_plane) },
        { ConstrainerType.Constrainer_twist, typeof(Constrainer_twist) },
        { ConstrainerType.Constrainer_minmaxangle, typeof(Constrainer_minmaxangle) },
        { ConstrainerType.Constrainer_minmaxangle_plane, typeof(Constrainer_minmaxangle_plane) }
    };

    public ConstrainerType constrainerType;
    public Constrainer constrainer;

    void OnValidate()
    {
        constrainer = (Constrainer)Activator.CreateInstance(Constrainers[constrainerType]);
        Debug.Assert(constrainer != null);
    }
}
