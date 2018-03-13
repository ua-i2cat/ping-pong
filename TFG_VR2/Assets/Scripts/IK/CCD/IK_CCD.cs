// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IK_CCD : MonoBehaviour
{
    public Transform target;
    public Transform[] joints;
    public float distanceThreshold;
    public int maxIterations;
    public float dampingFactor;

    private Vector3 E;
    private Vector3 T;

    private float[] sin;
    private float[] cos;
    private float[] theta;

    private bool done = false;
    private int iterations = 0;

    // Use this for initialization
    void Start()
    {
		E = joints[joints.Length - 1].transform.position;

        sin = new float[joints.Length];
        cos = new float[joints.Length];
        theta = new float[joints.Length];
    }

    // Update is called once per frame
    void Update()
    {
        if(done)
        {
            T = target.position;
            done = false;
        }

        T = target.position;

        //Debug.Log("T: " + T + " E: " + E + " Distance: " + Vector3.Distance(T, E));
        /*if (target.hasChanged iterations < maxIterations && Vector3.Distance(T, E) > distanceThreshold)
        {
            //Debug.Log("changed T: " + T + " E: " + E + " Distance: " + Vector3.Distance(T, E));
            //finished = false;
            target.hasChanged = false;
            iterations = 0;
        }*/

        if (!done)
        {
            for (int i = joints.Length - 2; i >= 0; i--)
            {
                Vector3 J = joints[i].transform.position;

                // Vector from the ith joint to the end effector
                Vector3 r1 = (E - J).normalized;

                // Vector from the ith joint to the target
                Vector3 r2 = (T - J).normalized;

                // Components of the angle between r1 and r2
                if (r1.magnitude * r2.magnitude <= 0.001f)
                {
                    // avoid division by small numbers
                    cos[i] = 1;
                    sin[i] = 0;
                }
                else
                {
                    // a · b = |a||b|cos(theta)
                    cos[i] = Vector3.Dot(r1, r2);

                    // |a x b| = |a||b|sin(theta)
                    sin[i] = (Vector3.Cross(r1, r2)).magnitude;
                }

                // Axis of rotation
                Vector3 axis = Vector3.Cross(r1, r2);

                // find the angle between r1 and r2 (and clamp values of cos to avoid errors)
                theta[i] = Mathf.Acos(Mathf.Max(-1, Mathf.Min(1, cos[i])));
                // invert angle if sin component is negative
                if (sin[i] < 0.0f)
                    theta[i] = -theta[i];
                // obtain an angle value between -pi and pi, and then convert to degrees
                theta[i] = (float)SimpleAngle(theta[i]) * Mathf.Rad2Deg;

                joints[i].transform.rotation *= Quaternion.AngleAxis(dampingFactor * theta[i], axis);

                E = joints[joints.Length - 1].transform.position;

                iterations++;
            }

            if (Vector3.Distance(T, E) < distanceThreshold /*|| iterations >= maxIterations*/)
            {
                Debug.Log("Target Reached!");
                Debug.Log(iterations + " iterations");

                done = true;
                iterations = 0;
            }
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
