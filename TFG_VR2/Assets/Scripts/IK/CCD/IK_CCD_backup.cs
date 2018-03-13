// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IK_CCD_backup : MonoBehaviour
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

    private bool finished = false;
    private int iterations = 0;

    // Use this for initialization
    void Start()
    {
		E = joints[joints.Length - 1].transform.position;

        sin = new float[joints.Length];
        cos = new float[joints.Length];
        theta = new float[joints.Length];
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        
        for (int i = 0; i < joints.Length; i++)
        {
            Gizmos.DrawSphere(joints[i].transform.position, 0.05f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(finished)
        {
            T = target.position;
            finished = false;
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

        if (!finished)
        {
            for (int i = joints.Length - 2; i >= 0; i--)
            {
                Vector3 J = joints[i].transform.position;
                //Debug.Log("Joint " + i + " at: " + joints[i].transform.position);

                // Vector from the ith joint to the end effector
                Vector3 r1 = (E - J).normalized;
                //Debug.Log("From Joint " + i + " to End effector: " + r1);

                // Vector from the ith joint to the target
                Vector3 r2 = (T - J).normalized;
                //Debug.Log("From Joint " + i + " to Target: " + r2);

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
                    //Debug.Log("cos[" + i + "]: " + cos[i]);

                    // |a x b| = |a||b|sin(theta)
                    sin[i] = (Vector3.Cross(r1, r2)).magnitude;
                    //Debug.Log("sin[" + i + "]: " + sin[i]);
                }

                // Axis of rotation
                Vector3 axis = Vector3.Cross(r1, r2);
                //Debug.Log("axis: " + axis);

                // find the angle between r1 and r2 (and clamp values of cos to avoid errors)
                theta[i] = Mathf.Acos(Mathf.Max(-1, Mathf.Min(1, cos[i])));
                // invert angle if sin component is negative
                if (sin[i] < 0.0f)
                    theta[i] = -theta[i];
                // obtain an angle value between -pi and pi, and then convert to degrees
                theta[i] = (float)SimpleAngle(theta[i]) * Mathf.Rad2Deg;
                //theta[i] = Mathf.Clamp(theta[i], 5, -5);
                //Debug.Log("theta[" + i + "]: " + theta[i]);

                joints[i].transform.rotation *= Quaternion.AngleAxis(dampingFactor * theta[i], axis);

                /*
                // Angle Constraint
                if (joints[i].MaxAngle > 0 && i > 0)
                {
                    // Compute the angle we will have if we apply the rotation
                    float angle = Quaternion.Angle(joints[i - 1].transform.rotation, 
                        joints[i].transform.rotation * Quaternion.AngleAxis(theta[i], axis));

                    // Only apply the rotation if the angle is in range
                    if (angle < joints[i].MaxAngle)
                    {
                        joints[i].transform.rotation *= Quaternion.AngleAxis(theta[i], axis);
                    }
                }
                else
                {
                    // Apply rotation without constraints
                    joints[i].transform.rotation *= Quaternion.AngleAxis(theta[i], axis);
                    //joints[i].transform.Rotate(axis, theta[i], Space.World);
                }

                // Axis Constraint (buggy)
                if (joints[i].Axis.magnitude == 1 && joints[i].Axis != axis)
                {
                    Vector3 eulerAngles = joints[i].transform.rotation.eulerAngles;
                    eulerAngles = new Vector3(eulerAngles.x * axis.x, 
                        eulerAngles.y * axis.y, eulerAngles.z * axis.z);
                    joints[i].transform.rotation = Quaternion.Euler(eulerAngles);

                    //theta[i] = 0;
                    Transform t = joints[i].transform;
                    //axis = t.InverseTransformDirection(joints[i].Axis);
                    //axis = joints[i].Axis;
                }
                */

                E = joints[joints.Length - 1].transform.position;

                iterations++;
            }

            //Debug.Log("theta[" + 2 + "]: " + theta[2]);

            if (Vector3.Distance(T, E) < distanceThreshold /*|| iterations >= maxIterations*/)
            {
                Debug.Log("Target Reached!");
                Debug.Log(iterations + " iterations");

                finished = true;
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
