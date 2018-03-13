// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: This class can be static? Check
public class Chain_FABRIK
{
    private static int maxIterations = 60;

    public static void Solve(Chain chain, Transform target, int startJoint = 0, float threshold = 0.001f)
    {
        if (startJoint >= chain.BoneCount)
            return;

        float dist, minDist;
        dist = minDist = Vector3.Distance(chain.EndEffector.Position, target.position);
        //Debug.Log("Initial distance to target: " + minDist);

        Joint currJ = chain.GetJoints()[startJoint];
        Joint nextJ = chain.GetJoints()[startJoint + 1];

        if (Vector3.Distance(currJ.Position, target.position) > chain.TotalLength(startJoint))
        {
            chain.Restore(startJoint);
            if(startJoint != 0)
            {
                for (int i = startJoint; i < chain.BoneCount; i++)
                {
                    chain.GetBone(i).MoveTo(chain.GetBone(i - 1).Tip.Position);
                }
            }
            
            Vector3 currToNext = (nextJ.Position - currJ.Position).normalized;
            Vector3 currToTarget = (target.position - currJ.Position).normalized;
            //Debug.DrawRay(chain.Root.Position, rootToEnd * 5, Color.red, 10);
            //Debug.DrawRay(chain.Root.Position, rootToTarget * 5, Color.blue, 10);
            Debug.unityLogger.logEnabled = true;
            float angle = Vector3.Dot(currToNext, currToTarget) * Mathf.Rad2Deg;
            Vector3 axis = Vector3.Cross(currToNext, currToTarget).normalized;
            //Debug.Log("angle: " + angle + " axis: " + axis);
            if (Mathf.Abs(angle) < 0.005f)
                return;

            Quaternion fromTo = Quaternion.FromToRotation(currToNext, currToTarget);
            chain.RotateJoint(startJoint, fromTo);
            chain.MoveJointTo(startJoint + 1, chain.GetBone(startJoint).Tip.Position);
            return;
        }

        for (int it = 0; it < maxIterations; ++it)
        { 
            dist = PerformOneIteration(chain, target, startJoint);

            // Check finish conditions
            if (dist < threshold)
            {
                Debug.Log("FINISHING(" + (it + 1) + " it.): dist(" + dist + ") < threshold(" + threshold + ")");
                break;
            }
            if (dist > minDist)
            {
                Debug.Log("FINISHING(" + (it + 1) + " it.): dist(" + dist + ") > minDist(" + minDist + ")");
                break;
            }
            if(Mathf.Abs(minDist - dist) < Mathf.Epsilon)
            {
                Debug.Log("FINISHING(" + (it + 1) + " it.): Distance reduced from last iteration = " + Mathf.Abs(minDist - dist));
                break;
            }

            if (dist < minDist)
                minDist = dist;
        }

        Debug.Log("Final distance to target: " + dist);
    }

    private static float PerformOneIteration(Chain chain, Transform target, int startJoint)
    {
        Vector3 origStartPos = ForwardStep(chain, target, startJoint);
        BackwardStep(chain, target, startJoint, origStartPos);
        return Vector3.Distance(chain.EndEffector.Position, target.position);
    }

    private static Vector3 ForwardStep(Chain chain, Transform target, int startJoint)
    {
        Vector3 startJointPos = chain.GetJoints()[startJoint].Position;
        chain.AttachBoneToChildPos(chain.BoneCount - 1, target.position, false);

        int i = chain.BoneCount - 2;
        while (i >= startJoint)
        {
            chain.AttachBoneToChild(i, false);
            i--;
        }

        return startJointPos;
    }

    private static void BackwardStep(Chain chain, Transform target, int startJoint, Vector3 startJointPos)
    {
        chain.AttachBoneToParentPos(startJoint, startJointPos, false);

        int i = startJoint + 1;
        while (i < chain.BoneCount)
        {
            chain.AttachBoneToParent(i, false);
            i++;
        }
    }
}
