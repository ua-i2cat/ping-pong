// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using UnityEngine;

public class Constrainer_angle : Constrainer
{
    public bool active;
    public float maxAngle;

    public Transform transform;
    public Transform parent;
    public Transform child;

    public override void Constrain()
    {
        if (active)
        {
            Debug.Assert(maxAngle >= 0 && maxAngle <= 180);

            Vector3 ToParent = (parent.position - transform.position).normalized;
            Debug.DrawLine(parent.position, transform.position, Color.red);
            Vector3 ToChild = (child.position - transform.position).normalized;
            Debug.DrawLine(child.position, transform.position, Color.blue);
            Vector3 axis = Vector3.Cross(ToParent, ToChild).normalized;
            Debug.DrawLine(transform.position, transform.position + axis, Color.green);

            float angle = ComputeAngle(ToParent, ToChild);
            if (angle == -1.0f)
            {
                transform.Translate(0.01f, 0.0f, 0.0f);
                return;
            }

            Debug.Log("angle " + angle);

            if (angle > maxAngle)
            {
                // Extract twist
                Quaternion twist, swing;
                QuaternionUtils.TwistSwingY(transform.localRotation, out twist, out swing);

                // Restore parent rotation before applying the constraint
                transform.rotation = parent.rotation;

                // Re-apply twist rotation
                transform.rotation *= twist;

                // Contrain rotation to only angle_degrees
                transform.Rotate(axis, 180 + maxAngle, Space.World);
            }
        }
    }
}
