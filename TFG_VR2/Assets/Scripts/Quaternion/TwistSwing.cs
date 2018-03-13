// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwistSwing : MonoBehaviour
{
    public Transform JointZ;
    public Transform JointY;
    public Transform JointX;

	// Use this for initialization
	void Start ()
    {
        Quaternion relZ = Quaternion.Inverse(JointZ.parent.rotation) *
            JointZ.rotation;

        Quaternion tZ, sZ;
        Quat.TwistSwingZ(relZ, out tZ, out sZ);

        float angle; Vector3 axis;
        relZ.ToAngleAxis(out angle, out axis);
        //Debug.Log(angle);
        //Debug.Log(axis);

        Quaternion twistZ = Quat.Normalize(new Quaternion(0, 0, relZ.z, relZ.w));
        Quaternion swingZ = relZ * Quaternion.Inverse(twistZ);

        swingZ.ToAngleAxis(out angle, out axis);
        axis = JointZ.TransformDirection(axis);
        Debug.DrawLine(JointZ.position, JointZ.position + axis, Color.blue, 60);
        Debug.Log(angle);
        Debug.Log(axis.ToString("F4"));
        //JointZ.Rotate(axis, -angle);


        Quaternion relY = Quaternion.Inverse(JointY.parent.rotation) *
            JointY.rotation;
        relY.ToAngleAxis(out angle, out axis);
        //Debug.Log(angle);
        //Debug.Log(axis);

        Quaternion tY, sY;
        Quat.TwistSwingY(relY, out tY, out sY);

        Quaternion twistY = Quat.Normalize(new Quaternion(0, relY.y, 0, relY.w));
        Quaternion swingY = relY * Quaternion.Inverse(twistY);

        swingY.ToAngleAxis(out angle, out axis);
        axis = JointY.TransformDirection(axis);
        Debug.DrawLine(JointY.position, JointY.position + axis, Color.green, 60);
        Debug.Log(angle);
        Debug.Log(axis.ToString("F4"));
        //JointY.Rotate(axis, -angle);


        Quaternion relX = Quaternion.Inverse(JointX.parent.rotation) *
            JointX.rotation;
        relX.ToAngleAxis(out angle, out axis);
        //Debug.Log(angle);
        //Debug.Log(axis);
        

        Quaternion tX, sX;
        Quat.TwistSwingX(relX, out tX, out sX);

        Quaternion twistX = Quat.Normalize(new Quaternion(relX.x, 0, 0, relX.w));
        Quaternion swingX = relX * Quaternion.Inverse(twistX);

        swingX.ToAngleAxis(out angle, out axis);
        axis = JointX.TransformDirection(axis);
        Debug.DrawLine(JointX.position, JointX.position + axis, Color.red, 60);
        Debug.Log(angle);
        Debug.Log(axis.ToString("F4"));
        //JointX.Rotate(axis, -angle);
    }

    // Update is called once per frame
    void Update ()
    {
		
	}
}
