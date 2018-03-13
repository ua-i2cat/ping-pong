// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constraints : MonoBehaviour
{
    public Transform reference;

    private float[] constraints = new float[] { 360.0f, 90.0f, 360.0f };
    private Quaternion[] rotations;
    private Quaternion offset;

	// Use this for initialization
	void Start ()
    {
        // Store initial rotation of joints (rest pose)
        rotations = new Quaternion[constraints.Length];
        int i = 0;
        rotations[i++] = reference.localRotation;
        for(Transform t = reference.GetChild(1); t != null; t = t.GetChild(1))
        {
            rotations[i++] = t.localRotation;
            if (t.childCount != 2)
                break;
        }
	}
	
	// Apply constraints after Update
	void LateUpdate ()
    {
        int jointLevel = 0;
        Transform T = transform;
        Transform refT = reference;
        Quaternion q = QuaternionUtils.RelativeRotation(rotations[jointLevel], refT.localRotation);
        Vector3 axis;
        float angle;
        q.ToAngleAxis(out angle, out axis);
        //Debug.Log("Angle: " + angle + " Axis: " + axis);
        if (angle <= constraints[jointLevel])
        {
            T.localRotation = rotations[0] * Quaternion.AngleAxis(angle, axis);
            //T.localRotation = refT.localRotation;
        }
        else
        {
            angle = Mathf.Clamp(angle, 0, constraints[jointLevel]);
            T.localRotation = rotations[0] * Quaternion.AngleAxis(angle, axis);
        }

        T = T.GetChild(1);
        refT = refT.GetChild(1);
        jointLevel++;

        while (T != null && T.childCount == 2)
        {
            Vector3 p0 = T.position;
            Vector3 p1old = T.GetChild(1).position;
            Vector3 p1new = refT.GetChild(1).position;

            q = QuaternionUtils.RelativeRotation(rotations[jointLevel], refT.localRotation);
            q.ToAngleAxis(out angle, out axis);
            angle = (float)QuaternionUtils.NormalizeAngle(angle);
            //Debug.Log("Angle: " + angle + " Axis: " + axis);

            // if the rotation cumplies with the constraints, apply it
            if (angle <= constraints[jointLevel] && angle >= -constraints[jointLevel])
            {
                //T.localRotation = Quaternion.AngleAxis(angle, axis);
                T.localRotation = refT.localRotation;
                //T.rotation = r * T.rotation;
            }

            // else clamp before aplying
            else
            {
                angle = Mathf.Clamp(angle, -constraints[jointLevel], constraints[jointLevel]);
                float x = Mathf.Clamp((float)QuaternionUtils.NormalizeAngle(q.eulerAngles.x), 
                    -constraints[jointLevel], constraints[jointLevel]);
                float y = Mathf.Clamp((float)QuaternionUtils.NormalizeAngle(q.eulerAngles.y), 
                    -constraints[jointLevel], constraints[jointLevel]);
                float z = Mathf.Clamp((float)QuaternionUtils.NormalizeAngle(q.eulerAngles.z), 
                    -constraints[jointLevel], constraints[jointLevel]);
                T.localEulerAngles = new Vector3(x, y, z);
                //T.localRotation = Quaternion.AngleAxis(angle, axis);

                //r = Quaternion.AngleAxis(diff, axis);
                //T.rotation = r * T.rotation;
            }

            T = T.GetChild(1);
            refT = refT.GetChild(1);
            jointLevel++;
        }        
	}
}
