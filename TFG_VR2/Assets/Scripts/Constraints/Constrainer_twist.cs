// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using UnityEngine;

public class Constrainer_twist : Constrainer
{
    public bool active;
    public float minAngle, maxAngle;

    public Transform transform;

    public enum ForwardDir { X, Y, Z };
    public ForwardDir localForward;

    public override void Constrain()
    {
        if (active)
        {
            float angle;
            Vector3 axis;
            transform.localRotation.ToAngleAxis(out angle, out axis);
            if (angle != 0)
            {
                Quaternion twist, swing;
                switch (localForward)
                {
                    case ForwardDir.X:
                        QuaternionUtils.TwistSwingX(transform.localRotation, out twist, out swing);
                        break;
                    case ForwardDir.Y:
                        QuaternionUtils.TwistSwingY(transform.localRotation, out twist, out swing);
                        break;
                    case ForwardDir.Z:
                    default:
                        QuaternionUtils.TwistSwingZ(transform.localRotation, out twist, out swing);
                        break;
                }

                float angleTwist;
                Vector3 axisTwist;
                twist.ToAngleAxis(out angleTwist, out axisTwist);

                if (!float.IsNaN(axisTwist.magnitude))
                {
                    if (angleTwist > 180)
                    {
                        angleTwist = 360 - angleTwist;
                        axisTwist = -axisTwist;
                    }

                    if (Mathf.Abs(angleTwist) < minAngle)
                        angleTwist = minAngle;
                    else if (Mathf.Abs(angleTwist) > maxAngle)
                        angleTwist = maxAngle;

                    transform.localRotation = swing * Quaternion.AngleAxis(angleTwist, axisTwist);
                }
            }
        }
    }
}
