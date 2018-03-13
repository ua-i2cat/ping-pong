// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class constrain_twist_v2 : MonoBehaviour
{
    public bool active;


    [SerializeField]
    Transform parent;

    public enum ForwardDir { X, Y, Z };
    public ForwardDir localForward;

    //    public enum ForwardDir { X, Y, Z };
    //    public ForwardDir localForward;
    //public bool negateForward = false;


    Quaternion getSwing()
    {
        Quaternion localRotation = Quaternion.Inverse(parent.rotation) * transform.rotation;


        Quaternion twist = localRotation;

        switch (localForward)
        {
            case ForwardDir.X:
                twist.y = 0;
                twist.z = 0;
                break;

            case ForwardDir.Y:
                twist.x = 0;
                twist.z = 0;
                break;

            case ForwardDir.Z:
                twist.x = 0;
                twist.y = 0;
                break;


        }

        twist = norm(twist);

        Quaternion swing = localRotation * Quaternion.Inverse(twist);
        return swing;

    }

    void LateUpdate()
    {
        if(active)
            transform.rotation = getSwing() * parent.rotation;
    

	}

    public Quaternion norm(Quaternion q)
    {
        float module = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
        Quaternion q2 = new Quaternion(q.x/module,q.y/module,q.z/module,q.w/module) ; 
        return q2;


    }

}
