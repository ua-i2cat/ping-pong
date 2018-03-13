// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainDebugger : MonoBehaviour
{
    public Transform ChainRoot;
    public bool ShowChain = false;
    public bool PrintAngles = false;

    private ChainManager chainManager;

    void Start ()
    {
        chainManager = new ChainManager(ChainRoot);
	}
	
	void LateUpdate ()
    {
        if (ShowChain)
            chainManager.Show();
        else
            chainManager.Hide();

        if (PrintAngles)
        {
            PrintBoneAngles();
            PrintAngles = false;
        }
	}

    private void PrintBoneAngles()
    {
        var chain = chainManager.GetChain();

        var baseBone = chain.GetBone(0);
        var baseDir = baseBone.Direction;
        //Debug.Log(baseDir);
        Debug.DrawLine(baseBone.Tip.Position, baseBone.Tip.Position + baseDir, Color.red, 10000);

        for (int i = 1; i < chain.BoneCount; i++)
        {
            var nextBone = chain.GetBone(i);
            var nextDir = nextBone.Direction;
            //Debug.Log(nextDir);

            Quaternion relativeRot = Quaternion.FromToRotation(baseBone.Direction, nextBone.Direction);
            float angle;
            Vector3 axis;
            relativeRot.ToAngleAxis(out angle, out axis);
            Debug.Log("axis: " + axis + " angle: " + angle);

            angle = Vector3.Angle(baseDir, nextDir);
            Debug.Log(angle);

            Debug.DrawLine(nextBone.Base.Position, nextBone.Base.Position + nextDir, Color.red, 10000);

            baseDir = nextDir;
        }
    }
}
