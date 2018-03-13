// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HipTransformController : MonoBehaviour {

    public Transform headTransform;
    public Transform footTransform;

    private Transform hipTransform;

	// Use this for initialization
	void Start ()
    {
        hipTransform = GetComponent<Transform>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        Vector3 difference = headTransform.position - footTransform.position;
        hipTransform.position = new Vector3(difference.x, difference.y / 2, difference.z);
        hipTransform.rotation = headTransform.rotation;
	}
}
