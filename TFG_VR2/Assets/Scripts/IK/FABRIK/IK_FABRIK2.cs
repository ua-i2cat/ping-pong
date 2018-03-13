// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IK_FABRIK2 : MonoBehaviour
{
    public Transform[] joints;
    public Transform target;

    private Vector3[] copy;
    private float[] distances;
    private bool done;

    void Start()
    {
        distances = new float[joints.Length - 1];
        copy = new Vector3[joints.Length];
    }

    void Update()
    {
        // Copy the joints positions to work with
        copy[0] = joints[0].position;
        for (int i = 0; i < copy.Length - 1; i++)
        {
            copy[i + 1] = joints[i + 1].position;
            distances[i] = Vector3.Distance(copy[i + 1], copy[i]);
        }

        done = Vector3.Distance(copy[copy.Length - 1], target.position) < 0.1f;
        if (!done)
        {
            float targetRootDist = Vector3.Distance(copy[0], target.position);

            // Update joint positions
            if (targetRootDist > distances.Sum())
            {
                // The target is unreachable
                for (int i = 0; i <= copy.Length - 2; i++)
                {
                    float r = Vector3.Distance(target.position, copy[i]);
                    float lambda = distances[i] / r;
                    copy[i + 1] = (1 - lambda) * copy[i] + lambda * target.position;
                }
            }
            else
            {
                // The target is reachable
                Vector3 b = copy[0];
                float difA = Vector3.Distance(copy[joints.Length - 1], target.position);
                while (difA > 0.1)
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
                }
            }

            // Update original joint rotations
            for (int i = 0; i <= joints.Length - 2; i++)
            {
                Rotate(joints[i], joints[i + 1].position, copy[i + 1]);
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
}
