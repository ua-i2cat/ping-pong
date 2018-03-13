// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawOrientation : MonoBehaviour
{
	// Update is called once per frame
	void Update ()
    {
        float scale = transform.localScale.x;

        Debug.DrawLine(transform.position, transform.position + transform.forward * scale, Color.blue);
        Debug.DrawLine(transform.position, transform.position + transform.right * scale, Color.red);
        Debug.DrawLine(transform.position, transform.position + transform.up * scale, Color.green);
    }
}
