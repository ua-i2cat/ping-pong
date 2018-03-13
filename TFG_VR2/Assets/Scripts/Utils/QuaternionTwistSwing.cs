// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class QuaternionTwistSwing : MonoBehaviour
{
    private void Start()
    {
        Quaternion q = transform.rotation;
        Quaternion twistY, swingXZ;
        QuaternionUtils.TwistSwingX(q, out twistY, out swingXZ);

        float angle;
        Vector3 axis;
        twistY.ToAngleAxis(out angle, out axis);
        Debug.Log("Twist angle: " + angle + " axis: " + axis);
        if(angle == 0)
        {
            // Arbitrary axis
            // Twist around Vector3.up
            axis = Vector3.up;
        }
        twistY = Quaternion.AngleAxis(45, axis);

        swingXZ.ToAngleAxis(out angle, out axis);
        Debug.Log("SwingXZ angle: " + angle + " axis: " + axis);
        if(angle != 0)
        {
            swingXZ = Quaternion.AngleAxis(0, axis);
        }

        //swingXZ = Quaternion.AngleAxis(30, axis);
        transform.rotation = swingXZ * twistY;
    }
}