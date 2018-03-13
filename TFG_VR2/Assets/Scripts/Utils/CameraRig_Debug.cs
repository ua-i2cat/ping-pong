// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRig_Debug : MonoBehaviour
{
    private Transform hmd;
    private Transform rightController;
    private Transform leftController;

    void Start ()
    {
        hmd = transform.GetChild(transform.childCount - 1);
        leftController = transform.GetChild(0);
        rightController = transform.GetChild(1);
    }
	
	void Update ()
    {
        Debug.DrawLine(hmd.position, rightController.position, Color.red);
        Debug.DrawLine(hmd.position, leftController.position, Color.blue);
    }
}
