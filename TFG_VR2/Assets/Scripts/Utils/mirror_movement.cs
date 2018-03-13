// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mirror_movement : MonoBehaviour
{
    public GameObject original;
	
	void Update ()
    {
        transform.localRotation = original.transform.localRotation;
    }
}
