// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstraintsUtil
{
    public class DetectionInfo
    {
        public int index;
        public float angle;
        public Quaternion relative;        
    }

    // Pre: The end effector NEVER defines a constraint!!!
    // Pre: c is a "solved" chain, where the endEffector is at the Target pos
    public static void Constrain(Chain chain, Transform target, int startingJoint = 0)
    {
DetectConstraint:
        // Detect the first constraint not satisfied, starting from startJoint
        var detection = Detect(chain, startingJoint);

        // If we don't detect anything, we are done
        if (detection == null)
        {
            return;
        }

        // Verify that the detection is on a correct Joint
        Debug.Assert(detection.index != chain.BoneCount);

        // Apply the constraint using the detection info
        Apply(chain, detection);
        // At this point detection.jointIndex satisfies the constraint
        
        if(detection.index + 1 >= chain.BoneCount)
        {
            //Debug.Log("DEADLOCK");
            detection.index = 0;

            IList<Joint> joints = chain.GetJoints();
            Quaternion relative = Quat.Relative(joints[0].Orientation, joints[0].RestOrientation);
            chain.GetBone(0).Rotate(relative);
            chain.GetBone(0).Rotate(relative);

            //float randAngle = Random.Range(-joints[0].MaxAngleCW, joints[0].MaxAngleCCW);
            //float randAngle = -joints[0].MaxAngleCW;
            //chain.GetBone(0).Rotate(relative);
            //chain.GetBone(0).Rotate(Quaternion.AngleAxis(randAngle, Quat.GetAxis(relative)));

            for (int i = 1; i < chain.BoneCount; i++)
            {
                chain.GetBone(i).MoveTo(chain.GetBone(i - 1).Tip.Position);
            }

            goto DetectConstraint;
        }

        // Apply FABRIK starting from the next joint after the constrained joint
        Chain_FABRIK.Solve(chain, target, detection.index + 1);
        
        startingJoint = detection.index + 1;    
        goto DetectConstraint;
    }

    private static DetectionInfo Detect(Chain chain, int startingJoint = 0)
    {
        IList<Joint> joints = chain.GetJoints();
        for(int i = startingJoint; i < joints.Count; i++)
        {
            if (!joints[i].HasConstraint)
                continue;

            Quaternion relative = i > 0 ? Quat.Relative(joints[i].Orientation, joints[i - 1].Orientation) : 
                Quat.Relative(joints[i].Orientation, joints[i].RestOrientation);
                
            float angle = Quat.GetAngle(relative);          // [-180, 180)
            float min = -Mathf.Abs(joints[i].MaxAngleCW);   // (-180, 0]
            float max = Mathf.Abs(joints[i].MaxAngleCCW);   // [0, 180)

            //Debug.Log("ANGLE: " + angle + " MIN: " + min + " MAX: " + max);

            DetectionInfo di = new DetectionInfo() { index = i, relative = relative };
            if (angle < min)
            {
                //Debug.Log("Angle BELOW the minimum");
                di.angle = min;
                return di;
            }
            else if (angle > max)
            {
                //Debug.Log("Angle ABOVE the maximum");
                di.angle = max;
                return di;
            }
        }

        return null;
    }

    private static void Apply(Chain chain, DetectionInfo detection)
    {    
        int index = detection.index;

        // Undo relative rotation
        chain.GetBone(index).Rotate(detection.relative);

        // Rotate back but only by detection.angle degrees
        Vector3 axis = Quat.GetAxis(detection.relative);
        chain.GetBone(index).Rotate(Quaternion.AngleAxis(detection.angle, axis));
        
        // Keep the chain linked
        for (int i = index + 1; i < chain.BoneCount; i++)
        {
            chain.GetBone(i).MoveTo(chain.GetBone(i - 1).Tip.Position);
        }
    }
}
