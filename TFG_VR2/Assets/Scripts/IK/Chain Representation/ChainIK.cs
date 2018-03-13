// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainIK : MonoBehaviour
{
    public Transform ChainRoot;
    public Transform Target;

    public bool runIK = false;

    void Start ()
    {
	}

    public void TurnIK(bool turnOn)
    {
        Debug.Log("Switching IK");
        runIK = turnOn;
    }

    void LateUpdate ()
    {
        if (runIK)
        {
            runIK = false;
            if (Target == null)
                return;

            // Get the chain and assert that it is not broken
            Chain chain = ChainManager.BuildChain(ChainRoot);
            Debug.Assert(!chain.IsBroken());

            var chainCopy = chain.Copy();
            chainCopy.Destroy();

            chain.Restore();
            Chain_FABRIK.Solve(chain, Target);
            ChainManager.ApplyJointTransforms(ChainRoot, chain.GetJoints());

            chain = ChainManager.BuildChain(ChainRoot);
            ConstraintsUtil.Constrain(chain, Target);
            ChainManager.ApplyJointTransforms(ChainRoot, chain.GetJoints());
        }

        if(Input.GetKeyDown(KeyCode.I))
        {
            runIK = false;
            Chain chain = ChainManager.BuildChain(ChainRoot);
            chain.Restore();
            Chain_FABRIK.Solve(chain, Target);
            ChainManager.ApplyJointTransforms(ChainRoot, chain.GetJoints());
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            runIK = false;
            Chain chain = ChainManager.BuildChain(ChainRoot);
            chain.Restore();
            ChainManager.ApplyJointTransforms(ChainRoot, chain.GetJoints());
        }

        if(Input.GetKeyDown(KeyCode.C))
        {
            Chain chain = ChainManager.BuildChain(ChainRoot);
            ConstraintsUtil.Constrain(chain, Target);
            ChainManager.ApplyJointTransforms(ChainRoot, chain.GetJoints());
        }

        if(Input.GetKeyDown(KeyCode.D))
        {
            Chain chain = ChainManager.BuildChain(ChainRoot);
            DisplayChainInfo(chain);
        }
    }

    struct ConstraintInfo
    {
        public int jointIndex;
        public Quaternion relRot;
        public float relAngle;
        public float desiredAngle;

        public ConstraintInfo(int jointIndex, Quaternion relRot, float relAngle,
            float desiredAngle)
        {
            this.jointIndex = jointIndex;
            this.relRot = relRot;
            this.relAngle = relAngle;
            this.desiredAngle = desiredAngle;
        }
    }

    private void DisplayChainInfo(Chain chain)
    {
        IList<Joint> joints = chain.GetJoints();
        for (int i = 0; i < joints.Count - 1; i++)
        {
            Debug.Log("JOINT[" + i + "]");
            Quaternion relative = Quat.Relative(joints[i].Orientation, joints[i].RestOrientation);
            if(i > 0)
            {
                relative = Quat.Relative(joints[i].Orientation, joints[i - 1].Orientation);
            }
           
            DisplayQuatInfo(relative);
        }
    }

    private void DisplayQuatInfo(Quaternion q)
    {
        float angle;
        Vector3 axis;
        q.ToAngleAxis(out angle, out axis);
        if (axis.y < 0)
        {
            Debug.Log("NEGATING");
            q = Quat.Negate(q);
            q.ToAngleAxis(out angle, out axis);
        }

        Debug.Log("AXIS: " + axis.ToString("F4"));

        if (-180 > angle || angle > 180)
            Debug.Log("ANGLE: " + angle + " NORMALIZED: " + Quat.NormalizeAngle(angle));
        else
            Debug.Log("ANGLE: " + angle);
    }
}
