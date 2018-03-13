// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Joint
{
    //private Vector3 position;
    //private Quaternion orientation;

    private Vector3 restPosition;    
    private Quaternion restOrientation;

    // TODO: Test Code
    public bool HasConstraint;
    public Vector3 UpAxis;
    public float MaxAngleCW, MaxAngleCCW;
    //

    public Transform unityJoint;

    private Joint(Vector3 pos, Quaternion rot)
    {
        Debug.DebugBreak();
        Debug.Break();
        //position = pos;
        //orientation = rot;

        //restPosition = pos;
        //restOrientation = rot;
    }

    public Joint(UnityJoint unityJoint)
    {
        this.unityJoint = unityJoint.transform;

        //position = unityJoint.transform.position;
        //orientation = unityJoint.transform.rotation;

        restPosition = Position;
        restOrientation = Orientation;
        if (unityJoint.joint != null)
        {
            restPosition = unityJoint.joint.restPosition;
            restOrientation = unityJoint.joint.restOrientation;
        }

        HasConstraint = unityJoint.HasConstraint;
        UpAxis = unityJoint.UpAxis;
        MaxAngleCW = unityJoint.MaxAngleCW;
        MaxAngleCCW = unityJoint.MaxAngleCCW;
    }

    public Vector3 Position
    {
        get
        {
            return unityJoint.position;
        }
        set
        {
            unityJoint.position = value;
        }
    }

    public Quaternion Orientation
    {
        get
        {
            return unityJoint.rotation;
        }
        set
        {
            unityJoint.rotation = value;
        }
    }

    public Vector3 RestPosition
    {
        get { return restPosition; }
    }

    public Quaternion RestOrientation
    {
        get { return restOrientation; }
    }

    public void Move(Vector3 translation)
    {
        Position += translation;
    }

    public void Rotate(Quaternion rot)
    {
        Orientation = rot * Orientation;
    }

    public void Restore(bool o = true, bool p = true)
    {
        if(o)
            Orientation = restOrientation;

        if(p)
            Position = restPosition;
    }

    public Joint Copy()
    {
        var joint = (Joint)MemberwiseClone();
        joint.unityJoint = new GameObject("TempJoint").transform;
        joint.unityJoint.position = unityJoint.position;
        joint.unityJoint.rotation = unityJoint.rotation;
        
        return joint;
    }

    public void Destroy()
    {
        if(unityJoint.name == "TempJoint")
        {
            GameObject.Destroy(unityJoint.gameObject);
        }
    }
}
