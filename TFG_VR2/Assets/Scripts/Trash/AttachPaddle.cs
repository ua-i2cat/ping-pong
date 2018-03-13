// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachPaddle : MonoBehaviour
{
    public GameObject paddlePrefab;
    private GameObject paddleInstance;

	// Use this for initialization
	void Start ()
    {
        paddleInstance = Instantiate(paddlePrefab);
        paddleInstance.GetComponent<FixedJoint>().connectedBody = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    private void OnDestroy()
    {
        Destroy(paddleInstance);
    }
}
