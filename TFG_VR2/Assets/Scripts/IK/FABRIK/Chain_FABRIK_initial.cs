// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chain_FABRIK_initial
{
    private int maxIterations = 20;

    public void Solve(Chain chain, Transform target, float threshold = 0.001f)
    {
        float dist, minDist;
        dist = minDist = Vector3.Distance(chain.EndEffector.Position, target.position);
        Debug.Log("Initial distance to target: " + minDist);
        for (int it = 0; it < maxIterations; ++it)
        { 
            dist = PerformOneIteration(chain, target);

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

        //chain.Bone(chain.BoneCount - 1).Direction
        //chain.EndEffector.Orientation = chain.EndEffector.RestOrientation;
        //chain.EndEffector.Orientation = chain.EndEffector.Orientation * target.rotation;
        //chain.EndEffector.Orientation = target.rotation;

        Debug.Log("Final distance to target: " + dist);
    }

    private float PerformOneIteration(Chain chain, Transform target)
    {
        int boneCount = chain.BoneCount;

        // FORWARD STEP
        chain.AttachBoneToChildPos(boneCount - 1, target.position, false);

        int i = boneCount - 2;
        while (i >= 0)
        {
            chain.AttachBoneToChild(i--, false);
            
            // TODO: Constraints
        }


        //BACKWARD STEP
        chain.AttachBoneToParentPos(0, chain.OriginalRootPos, false);
        i = 1;
        while (i < boneCount)
        {
            chain.AttachBoneToParent(i++, false);

            // TODO: Constraints
        }

        return Vector3.Distance(chain.EndEffector.Position, target.position);
    }
}
