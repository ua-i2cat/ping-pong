// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bone
{
    private Joint baseJoint;
    private Joint tipJoint;

    public Bone(Joint b, Joint t)
    {
        baseJoint = b;
        tipJoint = t;
    }

    public Joint Base
    {
        get
        {
            return baseJoint;
        }
    }

    public Joint Tip
    {
        get
        {
            return tipJoint;
        }
    }

    public float Length
    {
        get
        {
            return Vector3.Distance(baseJoint.Position, tipJoint.Position);
        }
    }

    public Vector3 Direction
    {
        get
        {
            return (tipJoint.Position - baseJoint.Position).normalized;
        }
    }

    public void Move(Vector3 translation)
    {
        baseJoint.Move(translation);
        tipJoint.Move(translation);
    }

    // Move base to position (folow with tip)
    public void MoveTo(Vector3 position)
    {
        Vector3 translation = position - baseJoint.Position;
        Move(translation);
    }

    // The pivot is the bone Base
    public void Rotate(Quaternion rot)
    {
        Vector3 newDir = rot * Direction;
        Vector3 tipOldPos = tipJoint.Position;
        Vector3 tipNewPos = baseJoint.Position + newDir * Length;

        baseJoint.Rotate(rot);
        tipJoint.Rotate(rot);
        tipJoint.Move(tipNewPos - tipOldPos);
    }

    public void Restore()
    {
        baseJoint.Restore();
        tipJoint.Restore();
    }

    public Bone Copy()
    {
        return new Bone(Base.Copy(), Tip.Copy());
    }

    public void Destroy()
    {
        Base.Destroy();
        Tip.Destroy();
    }
}
