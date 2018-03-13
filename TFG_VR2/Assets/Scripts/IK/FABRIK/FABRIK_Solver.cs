// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using System.Linq;
using UnityEngine;

public class FABRIK_Solver : IK_Solver
{
    public override void Solve(Transform[] joints, Transform target, float threshold = 0.1f, Constrain_minmaxangle[] constraints = null)
    {
        // Compute distances and store a copy of positions
        float[] distances = new float[joints.Length - 1];
        Vector3[] copy = new Vector3[joints.Length];

        copy[0] = joints[0].position;
        for (int i = 0; i < copy.Length - 1; i++)
        {
            copy[i + 1] = joints[i + 1].position;
            distances[i] = Vector3.Distance(copy[i + 1], copy[i]);
        }

        float targetRootDist = Vector3.Distance(copy[0], target.position);

        // Update joint positions
        if (targetRootDist > distances.Sum())
        {
            // The target is not reachable
            ImmediateReach(copy, joints, distances, target);
            //if (constrain != null)
            //    constrain();
        }
        else
        {
            // The target is reachable
            Vector3 b = copy[0];
            float difA = Vector3.Distance(copy[joints.Length - 1], target.position);
            int it = 0;
            while (difA > threshold && it++ < 10)
            {
                // STAGE 1: FORWARD REACHING
                copy[copy.Length - 1] = target.position;
                for (int i = copy.Length - 2; i >= 0; i--)
                {
                    float r = Vector3.Distance(copy[i], copy[i + 1]);
                    float lambda = distances[i] / r;
                    copy[i] = (1 - lambda) * copy[i + 1] + lambda * copy[i];
                }

                // STAGE 2: BACKWARD REACHING
                copy[0] = b;
                for (int i = 0; i <= copy.Length - 3; i++)
                {
                    float r = Vector3.Distance(copy[i], copy[i + 1]);
                    float lambda = distances[i] / r;
                    copy[i + 1] = (1 - lambda) * copy[i] + lambda * copy[i + 1];
                }
                difA = Vector3.Distance(copy[copy.Length - 1], target.position);

                // Apply rotations
                for (int i = 0; i <= joints.Length - 2; i++)
                {
                    joints[i].rotation = Rotate(joints[i].rotation, joints[i].position,
                        joints[i + 1].position, copy[i + 1]);

                    copy[i] = joints[i].position;
                }
                copy[joints.Length - 1] = copy[joints.Length - 1];

                // Apply constraints
                //if (constrain != null)
                //{
                //    constrain();
                //}
            }
        }
    }

    private void Rotate(Transform current, Vector3 nextOldPos, Vector3 nextNewPos)
    {
        Vector3 oldDir = (nextOldPos - current.position).normalized;
        Vector3 newDir = (nextNewPos - current.position).normalized;
        Quaternion q = Quaternion.FromToRotation(oldDir, newDir);
        current.rotation = q * current.rotation;
    }

    private Quaternion Rotate(Quaternion currRot, Vector3 currPos, Vector3 nextOldPos, Vector3 nextNewPos)
    {
        Vector3 oldDir = (nextOldPos - currPos).normalized;
        Vector3 newDir = (nextNewPos - currPos).normalized;
        Quaternion q = Quaternion.FromToRotation(oldDir, newDir);
        return q * currRot;
    }

    private void ImmediateReach(Vector3[] copy, Transform[] joints, float[] distances, Transform target)
    {
        for (int i = 0; i <= copy.Length - 2; i++)
        {
            float r = Vector3.Distance(target.position, copy[i]);
            float lambda = distances[i] / r;
            copy[i + 1] = (1 - lambda) * copy[i] + lambda * target.position;
        }

        // Apply rotations
        for (int i = 0; i <= joints.Length - 2; i++)
        {
            joints[i].rotation = Rotate(joints[i].rotation, joints[i].position,
                joints[i + 1].position, copy[i + 1]);
        }
    }
}
