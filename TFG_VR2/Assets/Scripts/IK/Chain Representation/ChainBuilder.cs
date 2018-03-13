// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using UnityEditor.Animations;
using UnityEngine;

public class ChainBuilder : MonoBehaviour
{   
    private bool show = false;
    private bool applyTransforms = false;
    private ChainManager visualizer;

    public Transform root = null;

    //
    public Transform target;
    //

    private void Start()
    {
        if (root == null)
            root = transform;

        visualizer = new ChainManager(transform);
    }

    public void StartIK()
    {
        show = true;
        applyTransforms = true;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            show = !show;
        }

        if(Input.GetKeyDown(KeyCode.I))
        {
            // Apply IK
            Chain c = visualizer.GetChain();
            c.GetBone(1).Move(new Vector3(1, 0, 0));
            visualizer.SetChain(c);
        }

        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            Chain c = visualizer.GetChain();
            c.AttachBoneToParent(1);
            visualizer.SetChain(c);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Chain c = visualizer.GetChain();
            c.AttachBoneToParent(1, false);
            visualizer.SetChain(c);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Chain c = visualizer.GetChain();
            c.AttachBoneToChild(0);
            visualizer.SetChain(c);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Chain c = visualizer.GetChain();
            c.AttachBoneToChild(0, false);
            visualizer.SetChain(c);
        }

        if(Input.GetKeyDown(KeyCode.F))
        {
            if (target == null)
                return;

            Chain c = visualizer.GetChain();
            Chain_FABRIK.Solve(c, target);
            visualizer.SetChain(c);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            applyTransforms = !applyTransforms;
        }      

        if (show)
        {
            visualizer.Show();
        }
        else
        {
            visualizer.Hide();
        }
    }

    private void LateUpdate()
    {
        if (applyTransforms)
        {
            if (target == null)
                return;

            // Get the chain and assert that it is not broken
            Chain c = ChainManager.BuildChain(root);
            Debug.Assert(!c.IsBroken());
            
            List<Joint> joints = new List<Joint>();
            for (int i = 0; i < c.BoneCount; i++)
            {
                joints.Add(c.GetBone(i).Base);
            }
            joints.Add(c.EndEffector);

            Chain_FABRIK.Solve(c, target);
            ChainManager.ApplyJointTransforms(root, joints);           

            visualizer.Show();
            visualizer.SetChain(c);
        }
    }
}
