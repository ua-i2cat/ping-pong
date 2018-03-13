// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxisAlignment : MonoBehaviour
{
    public Transform reference;
    public Transform curr;
    public Transform curr2;

    private Quaternion offsetQ;
    private Quaternion offsetQINV;

    //for option 2
    private Vector3 axisX;
    private float angleX;

    private Vector3 axisY;
    private float angleY;

    public int option = 5;

    // Use this for initialization
    void Start ()
    {
        //for option 2
        axisX = Vector3.Cross(reference.right, curr.right).normalized;
        angleX = -Mathf.Acos(Vector3.Dot(reference.right, curr.right)) * Mathf.Rad2Deg;

        axisY = Vector3.Cross(reference.up, curr.up).normalized;
        angleY = -Mathf.Acos(Vector3.Dot(reference.up, curr.up)) * Mathf.Rad2Deg;

        //for option 4
        offsetQ = Quaternion.Inverse(curr.transform.rotation) * reference.transform.rotation;
        offsetQINV = Quaternion.Inverse(offsetQ);
    }

    void Update()
    {
        switch(option)
        {
            case 1:
            {
                // 1 - Align current to match reference using angle-axis solution
                axisX = Vector3.Cross(reference.right, curr.right).normalized;
                angleX = -Mathf.Acos(Vector3.Dot(reference.right, curr.right)) * Mathf.Rad2Deg;
                curr.Rotate(axisX, angleX, Space.World);

                Vector3 axisY = Vector3.Cross(reference.up, curr.up).normalized;
                float angleY = -Mathf.Acos(Vector3.Dot(reference.up, curr.up)) * Mathf.Rad2Deg;
                curr.Rotate(axisY, angleY, Space.World);
            } break;

            case 2:
            {
                // 2 - Align current to match reference using "cached" angle-axis solution does NOT work!
                curr.Rotate(axisX, angleX, Space.World);
                curr.Rotate(axisY, angleY, Space.World);
            } break;

            case 3:
            {
                // 3 - Align current to match reference copying the orientation Quaternion TRIVIAL!
                curr.transform.rotation = reference.transform.rotation;
            } break;

            case 4:
            {
                // 4 - Keep the orientation offset between current and reference
                curr.transform.rotation = reference.transform.rotation * offsetQINV;
            } break;

            case 5:
            {
                //5 - uses quaternion offset to align second target  (curr2)
                curr.transform.rotation = reference.transform.rotation * offsetQINV;
                curr2.transform.rotation = curr.transform.rotation * offsetQ;
            } break;
        }
    }
}
