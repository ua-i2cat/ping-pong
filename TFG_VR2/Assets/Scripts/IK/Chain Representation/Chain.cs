// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chain
{
    // Internal representation of the bones forming the chain
    // TODO: Change representation into a Tree to allow multiple sub-chains
    private List<Bone> bones = new List<Bone>();
    private Vector3 originalRootPos;

    public enum BoneRef { Base, Tip };

    // Creates an empty chain
    public Chain() { }

    // Adds a bone to the chain. 
    // TODO: check that if the chain is not empty, the new bone's base matches
    // one (and only one) tip of the bones already in the chain
    public void AddBone(Bone bone)
    {
        if(bones.Count == 0)
        {
            originalRootPos = bone.Base.Position;
        }

        bones.Add(bone);
    }

    // Maximum length of the chain, starting from joint index
    public float TotalLength(int index = 0)
    {
        float totalLength = 0.0f;
        for(int i = index; i < bones.Count; i++)
        {
            totalLength += bones[i].Length;
        }
        return totalLength;
    }

    // Number of bones in the chain
    public int BoneCount
    {
        get
        {
            return bones.Count;
        }
    }

    public Vector3 OriginalRootPos
    {
        get { return originalRootPos; }
    }

    public Joint Root
    {
        get
        {
            return bones[0].Base;
        }
    }

    public Bone GetBone(int i)
    {
        return bones[i];
    }

    public Joint EndEffector
    {
        get
        {
            return bones[bones.Count - 1].Tip;
        }
    }

    // Move the ith bone, so that the bone's base or tip matches pos. 
    public void MoveBone(int i, BoneRef reference, Vector3 pos)
    {
        if(reference == BoneRef.Base)
        {
            bones[i].Move(pos - bones[i].Base.Position);
        }
        else
        {
            bones[i].Move(pos - bones[i].Tip.Position);
        }

        // At this point the chain may be broken
    }

    // Keep the ith bone fixed, and re-attach its children
    public void LinkChildren(int i)
    {
        throw new NotImplementedException();
    }

    // Check if any bone is not attached
    public bool IsBroken()
    {
        for(int i = 0; i < BoneCount - 1; i++)
        {
            if (!AreAttached(i, i + 1))
                return true;
        }

        return false;
    }

    // Check if two CONSECUTIVE bones are attached
    public bool AreAttached(int i, int j)
    {
        Debug.Assert(Mathf.Abs(i - j) == 1);
        return Vector3.Distance(bones[i].Tip.Position, bones[j].Base.Position) < Mathf.Epsilon;
    }

    public void AttachBoneToParentPos(int i, Vector3 pos, bool keepOrientation = true)
    {
        if (keepOrientation)
        {
            MoveBone(i, BoneRef.Base, pos);
        }
        else
        {
            // Rotate
            Vector3 newDir = bones[i].Tip.Position - pos;
            Vector3 oldDir = bones[i].Direction;
            Quaternion q = Quaternion.FromToRotation(oldDir, newDir);
            bones[i].Rotate(q);

            // Move
            MoveBone(i, BoneRef.Base, pos);
        }
    }

    // Move ith bone (base) to the tip of its parent
    public void AttachBoneToParent(int i, bool keepOrientation = true)
    {
        AttachBoneToParentPos(i, bones[i - 1].Tip.Position, keepOrientation);
    }

    public void AttachBoneToChildPos(int i, Vector3 pos, bool keepOrientation = true)
    {
        if (keepOrientation)
        {
            MoveBone(i, BoneRef.Tip, pos);
        }
        else
        {
            // Rotate
            Vector3 newDir = pos - bones[i].Base.Position;
            Vector3 oldDir = bones[i].Direction;
            Quaternion q = Quaternion.FromToRotation(oldDir, newDir);
            bones[i].Rotate(q);

            // Move
            MoveBone(i, BoneRef.Tip, pos);
        }
    }

    // Move ith bone (tip) to the base of its child
    public void AttachBoneToChild(int i, bool keepOrientation = true)
    {
        AttachBoneToChildPos(i, bones[i + 1].Base.Position, keepOrientation);
    }

    public float MaxBoneLength()
    {
        float maxBoneLength = 0.0f;

        foreach(var bone in bones)
        {
            if(bone.Length > maxBoneLength)
            {
                maxBoneLength = bone.Length;
            }
        }

        return maxBoneLength;
    }

    // Returns a read-only List of the joints in the chain
    public IList<Joint> GetJoints()
    {
        List<Joint> joints = new List<Joint>();
        for (int i = 0; i < BoneCount; i++)
        {
            joints.Add(GetBone(i).Base);
        }
        joints.Add(EndEffector);

        return joints.AsReadOnly();
    }

    public void Restore(int index = 0)
    {
        for(int i = index; i < bones.Count; i++)
        {
            bones[i].Restore();
        }
    }

    // fix and moved are consecutive indices
    public void AlignBones(int fix, int moved)
    {
        if (moved == fix + 1)
        {
            var relativeRot = Quat.Relative(GetBone(fix).Base.Orientation,
                    GetBone(moved).Base.Orientation);

            GetBone(moved).Rotate(Quaternion.Inverse(relativeRot));
        }
        else if(moved + 1 == fix)
        {
            Vector3 movedBasePos = bones[moved].Base.Position;
            bones[moved] = bones[fix].Copy();

            // Move
            bones[moved].Move(bones[moved].Base.Position - bones[moved].Tip.Position);
        }
        else
        {
            Debug.Break();
            Debug.DebugBreak();
        }
    }

    public void RotateBase(Quaternion q)
    {
        bones[0].Rotate(q);
        for(int i = 1; i < bones.Count; i++)
        {
            bones[i].MoveTo(bones[i - 1].Tip.Position);
            bones[i].Rotate(q);
        }
    }

    public void RotateJoint(int index, Quaternion q, bool recursive = true)
    {
        if (index == bones.Count)
        {
            return;
        }

        bones[index].Rotate(q);

        if (recursive)
            RotateJoint(index + 1, q, recursive);

        //for (int i = index + 1; i < bones.Count; i++)
        //{
        //    bones[i].MoveTo(bones[i - 1].Tip.Position);
        //    bones[i].Rotate(q);
        //}
    }

    public void MoveJointTo(int index, Vector3 pos, bool recursive = true)
    {
        if (index == bones.Count)
        {
            return;
        }

        bones[index].MoveTo(pos);

        if (recursive)
            MoveJointTo(index + 1, pos, recursive);

        //for (int i = index + 1; i < bones.Count; i++)
        //{
        //    bones[i].MoveTo(bones[i - 1].Tip.Position);
        //}
    }


    public Chain Copy()
    {
        Chain copy = new Chain();
        foreach(Bone b in bones)
        {
            copy.AddBone(b.Copy());
        }
        return copy;
    }

    public void Destroy()
    {
        foreach(Bone b in bones)
        {
            b.Destroy();
        }
    }
}
