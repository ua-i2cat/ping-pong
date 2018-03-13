// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using UnityEngine;

public abstract class Constrainer
{
    public abstract void Constrain();

    public static float ComputeAngle(Vector3 ToParent, Vector3 ToChild)
    {
        Vector3 axis = Vector3.Cross(ToParent, ToChild);
        if (Mathf.Abs(axis.magnitude) <= Mathf.Epsilon)
        {
            return -1.0f;
        }

        float num = Mathf.Acos(Mathf.Clamp(Vector3.Dot(ToParent, ToChild), -1f, 1f)) * 57.29578f;
        float num2 = Mathf.Sign(Vector3.Dot(axis.normalized, Vector3.Cross(ToParent, ToChild)));
        return (num * num2); // 0 to 360
    }
}
