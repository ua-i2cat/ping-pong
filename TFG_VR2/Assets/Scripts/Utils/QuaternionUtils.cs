// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuaternionUtils
{
    // Given two orientations, this function returns the rotation from one to the other
    public static Quaternion RelativeRotation(Quaternion a, Quaternion b)
    {
        return Quaternion.Inverse(a) * b;
    }

    public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion q)
    {
        var dir = point - pivot;    // get point direction relative to pivot
        dir = q * dir;              // rotate it
        point = dir + pivot;        // calculate rotated point
        return point;               // return it
    }

    public static void Rotate(Transform current, Vector3 nextOldPos, Vector3 nextNewPos)
    {
        Vector3 oldDir = (nextOldPos - current.position).normalized;
        Vector3 newDir = (nextNewPos - current.position).normalized;
        Quaternion q = Quaternion.FromToRotation(oldDir, newDir);
        current.rotation = q * current.rotation;
    }

    public static Quaternion Rotation(Transform current, Vector3 nextOldPos, Vector3 nextNewPos)
    {
        Vector3 oldDir = (nextOldPos - current.position).normalized;
        Vector3 newDir = (nextNewPos - current.position).normalized;
        Quaternion q = Quaternion.FromToRotation(oldDir, newDir);
        return q;
    }

    // Converts an angle to the range -180, 180
    public static double NormalizeAngle(double theta)
    {
        theta = theta % (360);
        if (theta < -180)
            theta += 360;
        else if (theta > 180)
            theta -= 360;
        return theta;
    }

    public static float NormalizeAngle(float theta)
    {
        return (float)NormalizeAngle((double)theta);
    }

    public static Quaternion Normalize(Quaternion q)
    {
        float d = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
        return new Quaternion(q.x / d, q.y / d, q.z / d, q.w / d);
    }

    //paints rotation axis of b relative to a
    public static void DrawRotationAxisFromPos(Transform a, Transform b, Transform c, Color co)
    {
        Vector3 axis = Vector3.Cross(a.position - b.position, c.position - b.position).normalized;
        Debug.DrawLine(b.position, b.position + axis, new Color(co.r, co.g, co.b));
    }

    public static void DrawRotationAxisFromQuat(Quaternion q, Transform b)
    {
        float angle;
        Vector3 axis;
        q.ToAngleAxis(out angle,out  axis);
        Debug.DrawLine(b.position, b.position + axis, Color.red);
    }

    public static void TwistSwingX(Quaternion q, out Quaternion twist, out Quaternion swing)
    {
        float denom = Mathf.Sqrt(q.w*q.w + q.x*q.x);
        twist = new Quaternion(q.x/denom, 0, 0, q.w/denom);
        swing = new Quaternion(0, (q.w*q.y - q.x*q.z)/denom, (q.w*q.z + q.x*q.y)/denom, denom);
        Debug.Assert(q == swing * twist);
    }

    public static void TwistSwingY(Quaternion q, out Quaternion twist, out Quaternion swing)
    {
        float denom = Mathf.Sqrt(q.w*q.w + q.y*q.y);
        twist = new Quaternion(0, q.y/denom, 0, q.w/denom);
        swing = new Quaternion((q.w * q.x + q.y * q.z) / denom, 0, (q.w*q.z - q.x*q.y)/denom, denom);
        Debug.Assert(q == swing * twist);
    }

    public static void TwistSwingZ(Quaternion q, out Quaternion twist, out Quaternion swing)
    {
        float denom = Mathf.Sqrt(q.w*q.w + q.z*q.z);
        twist = new Quaternion(0, 0, q.z/denom, q.w/denom);
        swing = new Quaternion((q.w * q.x - q.y * q.z) / denom, (q.w*q.y - q.x*q.z)/denom, 0, denom);
        Debug.Assert(q == swing * twist);
    }
}

