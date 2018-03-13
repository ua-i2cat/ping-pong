// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class constrain_angle : MonoBehaviour
{
    public bool active;
    public float maxAngle;

    //for debug purposes:
    public Transform parent;
    public Transform child;

    void Start()
    {
        bool run = maxAngle >= 0 && maxAngle <= 180;
        Debug.Assert(run, "Invalid max angle!");
        if (!run)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }

    void LateUpdate()
    {
        if (active)
        {
            Constrain();
        }
    }


    public void Constrain()
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

    private float ComputeAngle(Vector3 ToParent, Vector3 ToChild)
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
