// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FABRIK_Constrained_Solver : IK_Solver
{
    private float[] distances;
    private Vector3[] distanceDir;

    private Transform[] joints;
    private Constrain_minmaxangle[] constraints = null;
    private Transform target;

    int maxIterations = 1;

    public override void Solve(Transform[] joints, Transform target, float threshold = 0.1f, Constrain_minmaxangle[] constraints = null)
    {
        this.joints = joints;
        this.constraints = constraints;
        this.target = target;

        //1. we find the distances
        distances = new float[joints.Length - 1];
        distanceDir = new Vector3[joints.Length - 1];
        for (int i = 0; i < joints.Length - 1; i++)
        {
            distances[i] = Vector3.Distance(joints[i + 1].position, joints[i].position);

            distanceDir[i] = joints[i + 1].position - joints[i].position;
        }
        
        float targetRootDist = Vector3.Distance(joints[0].position, target.position);


        //2. we check if the target is reachable. Otherwise:

        // Update joint positions
        if (targetRootDist > distances.Sum())
        {
            Vector3 root = joints[0].position;

            ForwardReachingEndEffector();
            for (int i = joints.Length - 3; i >= 0; i--)
                ForwardReachingJoint(i);

            BackwardReachingBase(root);
            for (int i = 1; i < joints.Length - 1; i++)
            {
                BackwardReachingJoint(i);
            }

            Vector3 d = joints[joints.Length - 1].position - joints[joints.Length - 2].position;
            joints[joints.Length - 1].position = joints[joints.Length - 2].position + d.normalized * distances[joints.Length - 2];
            distanceDir[joints.Length - 2] = joints[joints.Length - 2].position - joints[joints.Length - 1].position;
        }
        else
        {
            // The target is reachable
            float bestDist = Vector3.Distance(joints[joints.Length - 1].position, target.position);
            // TODO: Store current chain config. (joint positions + rotations)
            int it = 0;
            Vector3 root = joints[0].position;
            while (bestDist > threshold && (it++ < maxIterations))
            {
                // STAGE 1: FORWARD REACHING

                // Moves the end effector (joints[n - 1]) to the target and the previous joint in the line
                // between its old position and the target
                ForwardReachingEndEffector();
                //ForwardReachingJoint(1);

                // Moves joints[i] to the line between its old position and the next joint, joints[i + 1]
                // and applies any joint constraint
                for (int i = joints.Length - 3; i >= 0; i--)
                    ForwardReachingJoint(i);


                // STAGE 2: BACKWARD REACHING
                BackwardReachingBase(root);

                for(int i = 1; i < joints.Length - 1; i++)
                {
                    BackwardReachingJoint(i);
                }

                BackwardReachingEndEffector();


                // Check if we are improving the results
                float dist = Vector3.Distance(joints[joints.Length - 1].position, target.position);
                if (dist < bestDist)
                {
                    // TODO: Store current chain config as best chain config.
                    bestDist = dist;
                }
                else
                {
                    // TODO: Restore best chain config.
                    break;
                }
            }
        }
    }

    // Apply FABRIK forward step on the end effector
    private void ForwardReachingEndEffector()
    {
        // Number of joints
        int n = joints.Length;

        // Position of the enf effector, joints[n - 1]
        Vector3 CurrentPos = joints[n - 1].position;

        // Position of the parent of the end effector, joints[n - 2]
        Vector3 ParentPos = joints[n - 2].position;

        // Target we want to move the end effector to
        Vector3 TargetPos = target.position;        

        // Distance between end effector and its parent
        float ParentToChildDist = distances[n - 2];

        // Distance between the parent of the end effector and the target
        float ParentToTargetDist = Vector3.Distance(ParentPos, TargetPos);

        // Ratio between the 2 distances above (used to linearly interpolate)
        float Ratio = ParentToChildDist / ParentToTargetDist;

        // Linear interpolation between the end effector's parent and the target
        Vector3 ParentNewPos = (1 - Ratio) * TargetPos + Ratio * ParentPos;

        // Translation required to move the end effector's parent to its new position
        Vector3 Translation = ParentNewPos - ParentPos;

        // Rotation from the old end effector position to the new one
        Vector3 OldDir = CurrentPos - ParentPos;
        Vector3 NewDir = TargetPos - ParentPos;      
        Quaternion Rotation = Quaternion.FromToRotation(OldDir, NewDir);
        
        // Translate and Rotate the end effector's parent
        joints[n - 2].position += Translation;
        joints[n - 2].rotation = Rotation * joints[n - 2].rotation;
        
        // Re-position the end effector to the target position
        joints[n - 1].position = TargetPos;

        distanceDir[n - 2] = joints[n - 2].position - joints[n - 1].position;
    }

    // Apply FABRIK forward setp on the joints[i] (with constraints)
    private void ForwardReachingJoint(int i)
    {
        // Position of the current joint
        Vector3 CurrentPos = joints[i].position;

        // (Imaginary) Position of where the current joint's child should be if it wasn't moved
        Vector3 CurrentEndPos = CurrentPos + distanceDir[i];

        Debug.Assert(Vector3.Distance(CurrentEndPos, CurrentPos) - distanceDir[i].magnitude < Mathf.Epsilon);

        // (Real) Position of the current joint's child
        Vector3 ChildPos = joints[i + 1].position;

        // Offset between the real and the imaginary positions
        //Vector3 Offset = ChildPos - CurrentEndPos;

        // Distance between current joint and its child real position
        float CurrentToChildDist = Vector3.Distance(CurrentPos, ChildPos);

        // Distance between current joint and its imaginary child (we already stored it, and should never change!)
        float RealCurrentToChildDist = distanceDir[i].magnitude;
        Debug.Assert(Vector3.Distance(CurrentPos, CurrentEndPos) - RealCurrentToChildDist < Mathf.Epsilon);

        // Ratio between the 2 distances above (used to linearly interpolate)
        float Ratio = RealCurrentToChildDist / CurrentToChildDist;

        // Linear interpolation between the current position and its child
        Vector3 CurrentNewPos = (1 - Ratio) * ChildPos + Ratio * CurrentPos;

        // Rotation from the old joint orientation (towards Imaginary) to the new one (towards Real)
        // Ref. (0)
        Vector3 OldDir = CurrentEndPos - CurrentPos;
        Vector3 NewDir = ChildPos - CurrentPos;
        Quaternion Rotation = Quaternion.identity;
        if(Vector3.Dot(OldDir.normalized, NewDir.normalized) < 1)
            Rotation = Quaternion.FromToRotation(OldDir.normalized, NewDir.normalized);

        // Ref. (1)
        // Move child position (Real) to where the joint thinks it should be (Imaginary)
        joints[i + 1].position = CurrentEndPos;

        // Store current child's rotation, to restore it later
        Quaternion ChildRot = joints[i + 1].rotation;

        // Ref. (2)
        // Rotate current joint so that the link is looking in the direction of the child position
        joints[i].rotation = Rotation * joints[i].rotation;

        // Position joint in its appropriate position (on the line pointing to the child)
        joints[i].position = CurrentNewPos;

        // Ref. (3)
        // Restore child rotation
        joints[i + 1].rotation = ChildRot;

        distanceDir[i] = joints[i + 1].position - joints[i].position;

        if (constraints[i + 1] != null)
        {
            // Store child rotation
            ChildRot = joints[i + 1].rotation;

            // Ref. (4)
            // Apply constraint (on the child!)
            constraints[i + 1].Constrain();

            // Compute constraint quaternion from the current child's orientation and its previous orientation
            Quaternion constraint = QuaternionUtils.RelativeRotation(ChildRot, joints[i + 1].rotation);

            // Ref. (5)
            // Restore child orientation, since we want to apply the constraint on the current joint
            joints[i + 1].rotation = ChildRot;

            // Apply constraint (on the parent), it moves the children positions away from the target
            joints[i].rotation = joints[i].rotation * Quaternion.Inverse(constraint);
            joints[i + 1].rotation = joints[i + 1].rotation * constraint;

            // Ref. (6)
            // Reposition current joint so that the end effector still matches the target position
            Vector3 offset = joints[joints.Length - 1].position - target.position;
            joints[i].position -= offset;
        }
    }

    private void BackwardReachingBase(Vector3 root)
    {
        Vector3 CurrentPos = joints[0].position;
        Vector3 CurrentChildPos = joints[1].position;

        joints[0].position = root;
        joints[1].position = CurrentChildPos;

        distanceDir[0] = joints[1].position - joints[0].position;
    }

    private void BackwardReachingJoint(int i)
    {
        Vector3 CurrentPos = joints[i].position;
        Vector3 ParentPos = joints[i - 1].position;
        Vector3 ChildPos = joints[i + 1].position;
        Quaternion ChildRot = joints[i + 1].rotation;

        Vector3 ParentToCurrent = CurrentPos - ParentPos;

        Vector3 CurrentExpectedPos = ParentPos + ParentToCurrent.normalized * distances[i - 1];

        // Distance between current joint expected position and its child position
        float CurrentToChildDist = Vector3.Distance(CurrentExpectedPos, ChildPos);

        // Distance between current joint and its imaginary child (we already stored it, and should never change!)
        float RealCurrentToChildDist = distanceDir[i].magnitude;

        // Ratio between the 2 distances above (used to linearly interpolate)
        float Ratio = RealCurrentToChildDist / CurrentToChildDist;

        Vector3 ChildNewPos = (1 - Ratio) * CurrentExpectedPos + Ratio * ChildPos;

        joints[i].position = CurrentExpectedPos;

        Vector3 OldDir = joints[i + 1].position - joints[i].position;
        Vector3 NewDir = ChildNewPos - joints[i].position;
        Quaternion Rotation = Quaternion.FromToRotation(OldDir, NewDir);
        joints[i].rotation = Rotation * joints[i].rotation;

        joints[i + 1].rotation = ChildRot;
        joints[i + 1].position = ChildPos;

        distanceDir[i] = joints[i + 1].position - joints[i].position;

        if(constraints[i] != null)
        {
            constraints[i].Constrain();
        }
    }

    private void BackwardReachingEndEffector()
    {
        Vector3 d = joints[joints.Length - 1].position - joints[joints.Length - 2].position;
        joints[joints.Length - 1].position = joints[joints.Length - 2].position + d.normalized * distances[joints.Length - 2];
        distanceDir[joints.Length - 2] = joints[joints.Length - 2].position - joints[joints.Length - 1].position;
    }

    private void Rotate(Transform current, Vector3 nextOldPos, Vector3 nextNewPos)
    {
        Vector3 oldDir = (nextOldPos - current.position).normalized;
        Vector3 newDir = (nextNewPos - current.position).normalized;
        Quaternion q = Quaternion.FromToRotation(oldDir, newDir);
        current.rotation = q * current.rotation;
    }

    private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion q)
    {
        var dir = point - pivot; // get point direction relative to pivot
        dir = q * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }
}
