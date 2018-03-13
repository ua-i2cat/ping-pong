// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IK_CCD2: MonoBehaviour
{
    // Array to hold all the joints
    // index 0 - root
    // index END - End Effector
    public GameObject[] joints;
    
    // The target for the IK system
    public GameObject target;


    // Array of angles to rotate by (for each joint) and their cos & sin components
    private float[] theta;
    private float[] sin;   
    private float[] cos;

    // To check if the target is reached at any point
    public bool done = false;
    // To store the position of the target
    private Vector3 tpos;

    // Max number of tries before the system gives up (Maybe 10 is too high?)
    [SerializeField]
    private int Mtries = 10;

    // The number of tries the system is at now
    private int tries = 0;

    // the range within which the target will be assumed to be reached
    private float epsilon = 0.1f;

    [SerializeField]
    public bool ready = false;

    // Initializing the variables
    void Start()
    {
        theta = new float[joints.Length];
        sin = new float[joints.Length];
        cos = new float[joints.Length];
        tpos = target.transform.position;
    }

    // Running the solver - all the joints are iterated through once every frame
    void Update()
    {
        if (ready)
        {
            // if the target hasn't been reached
            if (!done)
            {
                // if the Max number of tries hasn't been reached
                if (tries <= Mtries && Vector3.Distance(joints[joints.Length - 1].transform.position, target.transform.position) > epsilon)
                {
                    // starting from the second last joint (the last being the end effector)
                    // going back up to the root
                    for (int i = joints.Length - 2; i >= 0; i--)
                    {
                        // The vector from the ith joint to the end effector
                        Vector3 r1 = joints[joints.Length - 1].transform.position - joints[i].transform.position;
                        // The vector from the ith joint to the target
                        Vector3 r2 = target.transform.position - joints[i].transform.position;

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
                        joints[i].transform.Rotate(axis, theta[i], Space.World);
                    }

                    // increment tries
                    tries++;
                }
            }

            // find the difference in the positions of the end effector and the target
            float dist = Vector3.Distance(joints[joints.Length - 1].transform.position, target.transform.position);

            // if target is within reach (within epsilon) then the process is done
            done = dist < epsilon;

            // the target has moved, reset tries to 0 and change tpos
            if (target.transform.position != tpos)
            {
                tries = 0;
                tpos = target.transform.position;
            }
        }
    }

    public void Solve()
    {
        ready = true;       
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
