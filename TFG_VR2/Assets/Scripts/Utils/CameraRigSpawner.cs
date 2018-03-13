// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class Moves the CameraRig so that CamEye matches its transform position
public class CameraRigSpawner : MonoBehaviour
{
    public Transform CameraRig;
    public Transform CamEye;

	void Start ()
    {
        Vector3 diff = CamEye.position - CameraRig.position;
        CameraRig.position = transform.position - diff;
	}
}
