// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using UnityEngine;

public class CCD_Solver : IK_Solver
{
    public override void Solve(Transform[] joints, Transform target, float threshold = 0.1f, Constrain_minmaxangle[] constraints = null)
    {
        float[] theta = new float[joints.Length];
        float[] sin = new float[joints.Length];
        float[] cos = new float[joints.Length];

        float iterations = 0;
        float maxIterations = 10;
        float errorDist = Vector3.Distance(joints[joints.Length - 1].position, target.position);
        while (iterations < maxIterations && errorDist > threshold)
        {
            // starting from the second last joint (the last being the end effector)
            // going back up to the root
            for (int i = joints.Length - 2; i >= 0; i--)
            {
                // The vector from the ith joint to the end effector
                Vector3 r1 = joints[joints.Length - 1].position - joints[i].position;
                // The vector from the ith joint to the target
                Vector3 r2 = target.position - joints[i].position;

                // to avoid dividing by tiny numbers
                if (r1.magnitude * r2.magnitude <= 0.001f)
                {
                    // cos component will be 1 and sin will be 0
                    cos[i] = 1;
                    sin[i] = 0;
                }
                else
                {
                    // find the components using dot and cross product
                    cos[i] = Vector3.Dot(r1, r2) / (r1.magnitude * r2.magnitude);
                    sin[i] = (Vector3.Cross(r1, r2)).magnitude / (r1.magnitude * r2.magnitude);
                }

                // The axis of rotation is basically the 
                // unit vector along the cross product 
                Vector3 axis = (Vector3.Cross(r1, r2)) / (r1.magnitude * r2.magnitude);

                // find the angle between r1 and r2 (and clamp values of cos to avoid errors)
                theta[i] = Mathf.Acos(Mathf.Max(-1, Mathf.Min(1, cos[i])));
                // invert angle if sin component is negative
                if (sin[i] < 0.0f)
                    theta[i] = -theta[i];
                // obtain an angle value between -pi and pi, and then convert to degrees
                theta[i] = (float)SimpleAngle(theta[i]) * Mathf.Rad2Deg;
                // rotate the ith joint along the axis by theta degrees in the world space.
                joints[i].Rotate(axis, theta[i], Space.World);
            }

            // re-calculate error and increment iterations
            errorDist = Vector3.Distance(joints[joints.Length - 1].position, target.position);
            iterations++;
        }
    }

    // function to convert an angle to its simplest form (between -pi to pi radians)
    double SimpleAngle(double theta)
    {
        theta = theta % (2.0 * Mathf.PI);
        if (theta < -Mathf.PI)
            theta += 2.0 * Mathf.PI;
        else if (theta > Mathf.PI)
            theta -= 2.0 * Mathf.PI;
        return theta;
    }
}
