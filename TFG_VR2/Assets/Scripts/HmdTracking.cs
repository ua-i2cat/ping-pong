// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HmdTracking : MonoBehaviour
{
    public Transform hmd;

    private Vector3 hmdOffset;

    void Start ()
    {
        hmdOffset = transform.Find("hmdOffset").position - transform.position;
    }
	
	void Update ()
    {
        transform.position = new Vector3(hmd.position.x, hmd.position.y - hmdOffset.y, hmd.position.z - hmdOffset.z);
    }
}
