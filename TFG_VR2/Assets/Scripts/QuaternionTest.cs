// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuaternionTest : MonoBehaviour
{
    public Transform reference;
    public Transform joint;

    // Use this for initialization
    void Start()
    {
        // Where the joint is pointing in world coordinates (0.3, 0, 1.0)
        Vector3 jointForward = (joint.GetChild(1).position - joint.position).normalized;

        // Where the joint is poiting in local coordinates (0.0, 1.0, 0.0)
        Vector3 jointForwardLocal = (joint.GetChild(1).localPosition - joint.localPosition).normalized;

        // Z-axis of the joint in world coordinates (0.0, -1.0, 0.0)
        Vector3 forward = joint.forward;

        // The orientation (local = world in the parent)
        Quaternion jointOrientation = joint.rotation;
        Quaternion jointLocalOrientation = joint.localRotation;

        //Quaternion RefToJ = Quaternion.FromToRotation(reference.forward, jointForwardLocal);
        //Quaternion JToRef = Quaternion.FromToRotation(jointForwardLocal, reference.forward);
        //Quaternion Q = Quaternion.FromToRotation(reference.forward, jointForward);
        //reference.rotation = Q;

        // Quaternion to rotate from the identity orientation to the joint orientation
        Quaternion offset = Quaternion.FromToRotation(Vector3.forward, jointForward);
        Debug.Assert(offset * Vector3.forward == (joint.GetChild(1).position - joint.position).normalized);

        //Vector3 localForward = jointOrientation * reference.forward;
        //Vector3 f = Q * reference.forward;

        //Quaternion look = Quaternion.LookRotation(jointForward, );
    }
}
