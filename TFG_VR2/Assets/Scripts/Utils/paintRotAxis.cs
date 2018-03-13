// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class paintRotAxis : MonoBehaviour
{
    public Color c;

    public Transform parent;
    public Transform child;

	void Update ()
    {
        QuaternionUtils.DrawRotationAxisFromPos(parent, transform, child, c);
    }
}
