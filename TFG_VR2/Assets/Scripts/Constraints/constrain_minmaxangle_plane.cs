// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constrain_minmaxangle_plane : MonoBehaviour
{
    public bool active;
    public bool drawProjection;
    public float minAngle = 0.0f;  // must be between 0 and 180, and smaller than maxAngle
    public float maxAngle = 360.0f;  // must be between 0 and 180 and greater than minAngle

    //for debug purposes:
    public Transform parent;
    public Transform child;

    // Normal of the plane in which we allow rotation (up vector of the plane transform)
    public Transform plane;
    private Vector3 rotAxis;

    public float threshold = 0.0f;
    public float mag;

    void LateUpdate ()
    {
        if (active)
        {
            ConstrainToPlane();
            ConstrainToAngle();
        }
    }

    private void ConstrainToPlane()
    {
        rotAxis = plane.up;

        Vector3 ToParent = (parent.position - transform.position).normalized;
        Vector3 ToChild = (child.position - transform.position).normalized;
        Vector3 axis = Vector3.Cross(ToParent, ToChild).normalized;

        //float angle = ComputeAngle(ToParent, ToChild, rotAxis);
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

    private void ConstrainToAngle()
    {
        Vector3 ToParent = (parent.position - transform.position).normalized;
        Vector3 ToChild = (child.position - transform.position).normalized;
        Vector3 axis = Vector3.Cross(ToParent, ToChild).normalized;

        float angle = ComputeAngle(ToParent, ToChild, rotAxis);

        float half = FindHalfAngle(minAngle, maxAngle);

        if (half < 180)
        {
            if (angle <= minAngle && angle >= half)
            {
                SnapRotation(axis, angle, minAngle);
            }
            else if (angle >= maxAngle || angle <= half)
            {
                SnapRotation(axis, angle, maxAngle);
            }
        }
        else
        {
            if (angle >= maxAngle && angle <= half)
            {
                SnapRotation(axis, angle, maxAngle);
            }
            else if (angle <= minAngle || angle >= half)
            {
                SnapRotation(axis, angle, minAngle);
            }
        }
    }

    private void SnapRotation(Vector3 axis, float angle, float limitAngle)
    {
        transform.rotation = parent.rotation;
        if (angle > 180)
            transform.Rotate(-axis, 180 + limitAngle, Space.World);
        else
            transform.Rotate(axis, 180 + limitAngle, Space.World);
    }

    // Pre: minAngle and maxAngles are between 0 and 360
    //      minAngle < maxAngle
    private float FindHalfAngle(float minAngle, float maxAngle)
    {
        float half = (minAngle + maxAngle) / 2 + 180;
        if (minAngle > 180)
            half = (minAngle + maxAngle) / 2 - 180;

        return half % 360;
    }

    private float ComputeAngle(Vector3 ToParent, Vector3 ToChild, Vector3 up)
    {
        Vector3 axis = Vector3.Cross(ToParent, ToChild);
        if (Mathf.Abs(axis.magnitude) <= Mathf.Epsilon)
        {
            return 0.0f;
        }

        float num = Mathf.Acos(Mathf.Clamp(Vector3.Dot(ToParent, ToChild), -1f, 1f)) * 57.29578f;
        float num2 = Mathf.Sign(Vector3.Dot(-up, Vector3.Cross(ToParent, ToChild)));
        float angle = (num * num2); // -180 to 180
        if (angle < 0)
            angle = 360 + angle;
        return angle; // 0 to 360
    }
}
