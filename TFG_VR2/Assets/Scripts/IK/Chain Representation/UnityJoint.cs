// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class UnityJoint : MonoBehaviour
{
    public Joint joint;
    public bool HasConstraint = false;

    protected enum Dir_Enum { Right, Left, Up, Down, Forward, Back, Any };
    public enum AxisRotState_Enum { Free, Constrained, Disabled };

    [SerializeField]
    protected Dir_Enum upAxis;

    [Range(0, 180)]
    public float MaxAngleCW;
    [Range(0, 180)]
    public float MaxAngleCCW;


    public AxisRotState_Enum XAxisRotation;
    public AxisRotState_Enum YAxisRotation;
    public AxisRotState_Enum ZAxisRotation;

    [Range(0, 180)]
    public float XMinAngle, XMaxAngle, YMinAngle, YMaxAngle, ZMinAngle, ZMaxAngle;

    public Vector3 UpAxis{
        get
        {
            switch(upAxis)
            {
                case Dir_Enum.Right:
                    return Vector3.right;

                case Dir_Enum.Left:
                    return Vector3.left;

                case Dir_Enum.Up:
                    return Vector3.up;

                case Dir_Enum.Down:
                    return Vector3.down;

                case Dir_Enum.Forward:
                    return Vector3.forward;

                case Dir_Enum.Back:
                    return Vector3.back;

                case Dir_Enum.Any:
                default:
                    return Vector3.zero;
            }
        }
    }

	void Start ()
    {
        joint = new Joint(this);
    }

    void Update ()
    {
        //joint.Position      = transform.position;
        //joint.Orientation   = transform.rotation;

        // By Reflection
        //typeof(Joint).GetProperty("Position").SetValue(joint, transform.position, null);
        //typeof(Joint).GetProperty("Orientation").SetValue(joint, transform.rotation, null);
    }
}
