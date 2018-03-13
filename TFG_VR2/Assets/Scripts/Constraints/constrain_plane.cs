// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class constrain_plane : MonoBehaviour
{
    public bool active;
    public bool drawProjection = false;

    public Transform parent;
    public Transform child;

    // Normal of the plane in which we allow rotation (up vector of the plane transform)
    public Transform plane;
    private Vector3 rotAxis;

    // To define how "strict" we want to be
    public float threshold = 0.0f;
    public float mag;

    void LateUpdate()
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
                if(drawProjection)
                    Debug.DrawLine(transform.position, transform.position + 5 * projected.normalized, Color.red);

                axis = Vector3.Cross(ToParent, projected).normalized;
                //float sign = Mathf.Sign(Vector3.Dot(axis, Vector3.Cross(ToParent, projected)));
                //angle = sign * Vector3.Dot(-ToParent, projected.normalized) * Mathf.Rad2Deg;

                transform.rotation = parent.rotation;
                QuaternionUtils.Rotate(transform, child.position, transform.position + projected);
            }
        }
    }

    private float ComputeAngle(Vector3 ToParent, Vector3 ToChild)
    {
        Vector3 axis = Vector3.Cross(ToParent, ToChild).normalized;

        float num = Mathf.Acos(Mathf.Clamp(Vector3.Dot(ToParent, ToChild), -1f, 1f)) * 57.29578f;
        float num2 = Mathf.Sign(Vector3.Dot(-Vector3.up, Vector3.Cross(ToParent, ToChild)));

        float angle = (num * num2); // 180 to -180
        if (angle > 0)
            angle = 180 - angle;
        else
            angle = -180 - angle;

        return angle;
    }
}
