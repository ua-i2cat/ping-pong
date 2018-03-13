// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using UnityEngine;

public class Constrainer_plane : Constrainer
{
    public bool active;
    public bool drawProjection = false;

    public Transform transform;
    public Transform parent;
    public Transform child;

    // Normal of the plane in which we allow rotation (up vector of the plane transform)
    public Transform plane;
    private Vector3 rotAxis;

    // To define how "strict" we want to be
    public float threshold = 0.0f;
    public float mag;

    public override void Constrain()
    {
        if (active)
        {
            rotAxis = plane.up;

            Vector3 ToParent = (parent.position - transform.position).normalized;
            Vector3 ToChild = (child.position - transform.position).normalized;
            Vector3 axis = Vector3.Cross(ToParent, ToChild).normalized;

            //float angle = ComputeAngle(ToParent, ToChild);
            mag = (axis - rotAxis).magnitude;
            if (axis != Vector3.zero && (axis - rotAxis).magnitude > threshold && (-axis - rotAxis).magnitude > threshold)
            {
                Vector3 projected = Vector3.ProjectOnPlane(ToChild, rotAxis);
                if (drawProjection)
                    Debug.DrawLine(transform.position, transform.position + 5 * projected.normalized, Color.red);

                axis = Vector3.Cross(ToParent, projected).normalized;
                //float sign = Mathf.Sign(Vector3.Dot(axis, Vector3.Cross(ToParent, projected)));
                //angle = sign * Vector3.Dot(-ToParent, projected.normalized) * Mathf.Rad2Deg;

                transform.rotation = parent.rotation;
                QuaternionUtils.Rotate(transform, child.position, transform.position + projected);
            }
        }
    }
}
