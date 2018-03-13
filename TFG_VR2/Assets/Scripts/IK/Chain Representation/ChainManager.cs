// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainManager
{
    private Transform rootJoint;

    private GameObject chainObject;
    private Chain chain;
    private static float scale = 0.0f;

    public static Chain BuildChain(Transform root)
    {
        Chain chain = new Chain();

        List<UnityJoint> jointsList = new List<UnityJoint>();
        AddJoints(root, jointsList);

        for (int i = 0; i < jointsList.Count - 1; i++)
        {
            Joint baseJoint = new Joint(jointsList[i].GetComponent<UnityJoint>());
            Joint tipJoint = new Joint(jointsList[i + 1].GetComponent<UnityJoint>());
            Bone bone = new Bone(baseJoint, tipJoint);
            chain.AddBone(bone);
        }

        return chain;
    }

    public ChainManager(Transform root)
    {
        rootJoint = root;
    }

    public void Show()
    {
        if (HasChangedRecursive(rootJoint))
        {
            chain = null;   // mark the chain to be recreated
        }

        // check if the chain is not created
        if (chain == null)
        {
            // Create or re-create the chain
            chain = BuildChain(rootJoint);

            // Destroy chainObject if it already existed
            if (chainObject != null)
            {
                Object.Destroy(chainObject);
                chainObject = null;
            }

            //Debug.Log("Creating Debug Chain");
            chainObject = CreateChainObject(chain);
        }

        if(chainObject == null)
        {
            chainObject = CreateChainObject(chain);
        }
    }

    public void Hide()
    {
        if (chainObject != null)
        {
            Object.Destroy(chainObject);
            chainObject = null;
            chain = null;
        }
    }

    public Chain GetChain()
    {
        return chain;
    }

    public void SetChain(Chain chain)
    {
        this.chain = chain;
        Object.Destroy(chainObject);
        chainObject = null;
        Show();
    }

#region Private Methods
    private GameObject CreateChainObject(Chain chain)
    {
        GameObject chainObject = new GameObject("chain_debug");
        if(scale == 0.0f)
            scale = chain.MaxBoneLength();

        for (int i = 0; i < chain.BoneCount; i++)
        {
            var boneObject = CreateBoneObject(chain.GetBone(i), scale);
            if(i == 0)
            {
                // Set the chain position to the first bone (root)
                chainObject.transform.position = boneObject.transform.position;
            }

            boneObject.transform.parent = chainObject.transform;
        }

        return chainObject;
    }

    private GameObject CreateBoneObject(Bone bone, float scale = 1.0f)
    {
        Debug.Assert(scale >= Mathf.Epsilon);   // ensure scale is bigger than "0"

        GameObject boneObject = new GameObject("bone_debug");
        boneObject.transform.position = bone.Base.Position;

        GameObject baseJoint = Object.Instantiate((GameObject)Resources.Load("joint_debug"), bone.Base.Position, bone.Base.Orientation);
        baseJoint.transform.localScale = new Vector3(scale / 5, scale / 5, scale / 5);
        baseJoint.transform.parent = boneObject.transform;

        GameObject tipJoint = Object.Instantiate((GameObject)Resources.Load("joint_debug"), bone.Tip.Position, bone.Tip.Orientation);
        tipJoint.transform.localScale = new Vector3(scale / 5, scale / 5, scale / 5);
        tipJoint.transform.parent = boneObject.transform;

        GameObject link = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Vector3 between = tipJoint.transform.position - baseJoint.transform.position;
        link.transform.localScale = new Vector3(scale / 10, scale / 10, (between).magnitude);
        link.transform.position = baseJoint.transform.position + (between / 2.0f);
        link.transform.LookAt(tipJoint.transform.position);
        link.transform.parent = boneObject.transform;

        return boneObject;
    }
#endregion

    public void ApplyChain(Transform root)
    {
        
    }

    private static void AddJoints(Transform parent, IList<UnityJoint> jointsList)
    {
        UnityJoint jointComponent = parent.GetComponent<UnityJoint>();
        if (jointComponent != null && jointComponent.enabled)
            jointsList.Add(jointComponent);

        foreach (Transform child in parent)
        {
            AddJoints(child, jointsList);
            break; // Only 1 child per joint allowed for now!!
        }
    }

    private bool HasChangedRecursive(Transform t)
    {
        bool hasChanged = t.hasChanged;
        t.hasChanged = false;

        for(int i = 0; i < t.childCount; i++)
        {
            hasChanged = hasChanged || HasChangedRecursive(t.GetChild(i));
        }

        return hasChanged;
    }




    public static void ApplyJointTransforms(Transform root, IList<Joint> joints, int i = 0)
    {
        UnityJoint jointComponent = root.GetComponent<UnityJoint>();
        if (jointComponent != null && jointComponent.enabled)
        {
            jointComponent.transform.position = joints[i].Position;
            jointComponent.transform.rotation = joints[i].Orientation;
            i++;
        }

        foreach (Transform child in root)
        {
            ApplyJointTransforms(child, joints, i);
            break; // Only 1 child per joint allowed for now!!
        }
    }
}
