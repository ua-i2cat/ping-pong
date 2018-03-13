// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constrain_minmaxangle : MonoBehaviour
{
    public bool active;
    public float minAngle;  // must be between 0 and 180, and smaller than maxAngle
    public float maxAngle;  // must be between 0 and 180 and greater than minAngle

    //for debug purposes:
    public Transform parent;
    public Transform child;

    void Start()
    {
        bool run = minAngle >= 0 && minAngle <= 180;
        run = run &&maxAngle >= 0 && maxAngle <= 180;
        run = run && minAngle < maxAngle;
        Debug.Assert(run, "Invalid min or max angle!");
        if(!run)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }

    void LateUpdate ()
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
        //Debug.DrawLine(transform.position, parent.position, Color.red, 30);
        //Debug.DrawLine(transform.position, child.position, Color.blue, 30);
        Vector3 axis = Vector3.Cross(ToParent, ToChild).normalized;

        float angle = ComputeAngle(ToParent, ToChild);
        if (angle == -1.0f)
        {
            //transform.Translate(0.01f, 0.0f, 0.0f);
            return;
        }

        Debug.Log("the angle is: " + angle);

        // 360 - minAngle < angle < minAngle
        if (angle < minAngle || angle > 360 - minAngle)
        {
            transform.rotation = parent.rotation;
            if (angle > 180)
            {
                transform.Rotate(-axis, 180 - minAngle, Space.World);
            }
            else
            {
                transform.Rotate(axis, 180 + minAngle, Space.World);
            }
        }
        // maxAngle < angle < 360 - maxAngle
        else if (angle > maxAngle && angle < 369 - maxAngle)
        {
            transform.rotation = parent.rotation;
            if (angle > 180)
            {
                transform.Rotate(-axis, 180 - maxAngle, Space.World);
            }
            else
            {
                transform.Rotate(axis, 180 + maxAngle, Space.World);
            }
        }
    }

    private float ComputeAngle(Vector3 ToParent, Vector3 ToChild)
    {
        Vector3 axis = Vector3.Cross(ToParent, ToChild);
        if(Mathf.Abs(axis.magnitude) <= Mathf.Epsilon)
        {
            return 0.0f;
        }

        float num = Mathf.Acos(Mathf.Clamp(Vector3.Dot(ToParent, ToChild), -1f, 1f)) * 57.29578f;
        float num2 = Mathf.Sign(Vector3.Dot(axis.normalized, Vector3.Cross(ToParent, ToChild)));
        return (num * num2); // 0 to 360
    }
}
