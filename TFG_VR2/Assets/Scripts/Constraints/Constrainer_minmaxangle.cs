// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using UnityEngine;

public class Constrainer_minmaxangle : Constrainer
{
    public bool active;
    public float minAngle;  // must be between 0 and 180, and smaller than maxAngle
    public float maxAngle;  // must be between 0 and 180 and greater than minAngle

    public Transform transform;
    public Transform parent;
    public Transform child;

    public override void Constrain()
    {
        if (active)
        {
            Vector3 ToParent = (parent.position - transform.position).normalized;
            Vector3 ToChild = (child.position - transform.position).normalized;
            Vector3 axis = Vector3.Cross(ToParent, ToChild).normalized;

            float angle = ComputeAngle(ToParent, ToChild);
            if (angle == -1.0f)
            {
                transform.Translate(0.01f, 0.0f, 0.0f);
                return;
            }

            // 360 - minAngle < angle < minAngle
            if (angle < minAngle || angle > 360 - minAngle)
            {
                transform.rotation = parent.rotation;
                if (angle > 180)
                    transform.Rotate(-axis, 180 - minAngle, Space.World);
                else
                    transform.Rotate(axis, 180 + minAngle, Space.World);
            }
            // maxAngle < angle < 360 - maxAngle
            else if (angle > maxAngle && angle < 369 - maxAngle)
            {
                transform.rotation = parent.rotation;
                if (angle > 180)
                    transform.Rotate(-axis, 180 - maxAngle, Space.World);
                else
                    transform.Rotate(axis, 180 + maxAngle, Space.World);
            }
        }
    }
}
