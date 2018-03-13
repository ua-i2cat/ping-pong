// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IK_FABRIK : MonoBehaviour
{
    public Transform[] joints;
    public Transform target;

    private float[] distances;
    private bool done;

    void Start ()
    {
        distances = new float[joints.Length];
        for(int i = 0; i < joints.Length - 1; i++)
        {
            distances[i] = Vector3.Distance(joints[i + 1].position, joints[i].position);
        }
	}

	void Update ()
    {
        bool reachable;
        done = Vector3.Distance(joints[joints.Length - 1].position, target.position) < 0.1;
        for (int i = 0; i < joints.Length - 2; i++)
        {
            distances[i] = Vector3.Distance(joints[i + 1].position, joints[i].position);
        }

        if (!done)
        {
            float targetRootDist = Vector3.Distance(joints[0].position, target.position);

            // Update joint positions
            if (targetRootDist > distances.Sum())
            {
                Debug.Log("The target is unreachable");
                reachable = false;
                // The target is unreachable
                for (int i = 0; i < joints.Length - 1; i++)
                {
                    float r = Vector3.Distance(target.position, joints[i].position);
                    float lambda = distances[i] / r;
                    joints[i + 1].position = (1 - lambda) * joints[i].position + lambda * target.position;
                    joints[i].rotation = Quaternion.LookRotation(joints[i + 1].position - joints[i].position);
                }
            }
            else
            {
                //Debug.Log("The target is reachable");
                reachable = true;
                // The target is reachable
                Vector3 b = joints[0].position;
                float difA = Vector3.Distance(joints[joints.Length - 1].position, target.position);
                while (difA > 0.1)
                {
                    // STAGE 1: FORWARD REACHING
                    joints[joints.Length - 1].position = target.position;
                    for (int i = joints.Length - 2; i >= 0; i--)
                    {
                        float r = Vector3.Distance(joints[i].position, joints[i + 1].position);
                        float lambda = distances[i] / r;
                        joints[i].position = (1 - lambda) * joints[i + 1].position + lambda * joints[i].position;
                    }

                    // STAGE 2: BACKWARD REACHING
                    joints[0].position = b;
                    for (int i = 0; i < joints.Length - 2; i++)
                    {
                        float r = Vector3.Distance(joints[i].position, joints[i + 1].position);
                        float lambda = distances[i] / r;
                        joints[i + 1].position = (1 - lambda) * joints[i].position + lambda * joints[i + 1].position;
                        joints[i].rotation = Quaternion.LookRotation(joints[i + 1].position - joints[i].position);
                    }
                    difA = Vector3.Distance(joints[joints.Length - 1].position, target.position);
                }
            }

            if (reachable)
            {
                // Update joint rotations
                for (int i = 0; i < joints.Length - 1; i++)
                {
                    joints[i].rotation = Quaternion.LookRotation(joints[i + 1].position - joints[i].position);
                }
            }
        }
	}
}
