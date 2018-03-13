// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quat
{
    // Returns the quaternion that rotates the orientation "from" to the orientation "to"
    public static Quaternion Relative(Quaternion from, Quaternion to)
    {
        return to * Quaternion.Inverse(from);
    }

    // Returns the quaternion q normalized
    public static Quaternion Normalize(Quaternion q)
    {
        float d = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
        return new Quaternion(q.x / d, q.y / d, q.z / d, q.w / d);
    }

    public static Quaternion Negate(Quaternion q)
    {
        return new Quaternion(-q.x, -q.y, -q.z, -q.w);
    }

    // Returns the axis of rotation of a given quaternion rotation q
    public static Vector3 GetAxis(Quaternion q, bool getUpperHalfSphere = false)
    {
        float angle;
        Vector3 axis;
        q.ToAngleAxis(out angle, out axis);
        if (axis.y < 0 && getUpperHalfSphere)
        {
            q = Negate(q);
            q.ToAngleAxis(out angle, out axis);
        }

        return axis;
    }

    // Returns the normalized angle [-180, 180) of a given quaternion rotation q
    public static float GetAngle(Quaternion q)
    {
        float angle;
        Vector3 axis;
        q.ToAngleAxis(out angle, out axis);
        if (axis.y < 0)
        {
            q = Quat.Negate(q);
            q.ToAngleAxis(out angle, out axis);
        }

        if (-180 > angle || angle > 180)
            angle = Quat.NormalizeAngle(angle);

        return angle;
    }

    // Returns the float angle normalized to the range [start, end) by default [-180, 180)
    public static float NormalizeAngle(float value, float start = -180.0f, float end = 180.0f)
    {
        float width = end - start;
        float offsetValue = value - start;   // value relative to 0

        float ret = (offsetValue - (Mathf.Floor(offsetValue / width) * width)) + start;
        // + start to reset back to start of original range

        return ret;
    }

    public static float GetSignedAngle(Quaternion A, Quaternion B, Vector3 axis)
    {
        float angle = 0f;
        Vector3 angleAxis = Vector3.zero;
        (B * Quaternion.Inverse(A)).ToAngleAxis(out angle, out angleAxis);
        if (Vector3.Angle(axis, angleAxis) > 90f)
        {
            angle = -angle;
        }
        return Mathf.DeltaAngle(0f, angle);
    }

    // Draws a frame with right, up and forward axis of the orientation represented 
    // by the quaternion q at the position pos
    public static void DrawFrameAt(Quaternion q, Vector3 pos, float scale = 1.0f, float time = 0.0f)
    {
        var x = q * Vector3.right;
        Debug.DrawRay(pos, x * scale, Color.red, time);

        var y = q * Vector3.up;
        Debug.DrawRay(pos, y * scale, Color.green, time);

        var z = q * Vector3.forward;
        Debug.DrawRay(pos, z * scale, Color.blue, time);
    }

    // Assuming the forward vector is pointing in the Z axis
    public static void TwistSwingZ(Quaternion q, out Quaternion twist, out Quaternion swing)
    {
        float denom = Mathf.Sqrt(q.w * q.w + q.z * q.z);
        twist = new Quaternion(0, 0, q.z / denom, q.w / denom);
        swing = new Quaternion((q.w * q.x - q.y * q.z) / denom, (q.w * q.y - q.x * q.z) / denom, 0, denom);
        Debug.Assert(q == swing * twist);
    }

    // Assuming the forward vector is pointing in the Y axis
    public static void TwistSwingY(Quaternion q, out Quaternion twist, out Quaternion swing)
    {
        float denom = Mathf.Sqrt(q.w * q.w + q.y * q.y);
        twist = new Quaternion(0, q.y / denom, 0, q.w / denom);
        swing = new Quaternion((q.w * q.x + q.y * q.z) / denom, 0, (q.w * q.z - q.x * q.y) / denom, denom);
        Debug.Assert(q == swing * twist);
    }

    // Assuming the forward vector is pointing in the X axis
    public static void TwistSwingX(Quaternion q, out Quaternion twist, out Quaternion swing)
    {
        float denom = Mathf.Sqrt(q.w * q.w + q.x * q.x);
        twist = new Quaternion(q.x / denom, 0, 0, q.w / denom);
        swing = new Quaternion(0, (q.w * q.y - q.x * q.z) / denom, (q.w * q.z + q.x * q.y) / denom, denom);
        Debug.Assert(q == swing * twist);
    }
}
