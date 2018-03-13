// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IK_GradientDescent : MonoBehaviour
{

    //THIS NEEDS MORE WORK

        /*
    public Transform target;

    private float[] angles;
    private bool finished = false;

    public Transform[] Joints;
    public float LearningRate;
    public float SamplingDistance;

    public float DistanceThreshold;


    // Use this for initialization
    void Start()
    {
        //Debug.Log("Target " + target.position);
        //Debug.Log("Current end effector " + endEffector.position);

        angles = new float[Joints.Length];
        for (int i = 0; i < Joints.Length; i++)
        {
            Debug.Log(Joints[i].transform.position);
            float angle;
            Vector3 axis;
            Joints[i].transform.rotation.ToAngleAxis(out angle, out axis);
            Debug.Log("Joint " + i + " axis: " + axis + " angle: " + angle);

            angles[i] = angle;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!finished)
        {
            InverseKinematics(target.position, angles);
            for (int i = 0; i < Joints.Length; i++)
            {
                Joints[i].transform.Rotate(Vector3.right, angles[i], Space.Self);

                Debug.Log(Joints[i].transform.position);
                float angle;
                Vector3 axis;
                Joints[i].transform.rotation.ToAngleAxis(out angle, out axis);
                Debug.Log("Joint " + i + " axis: " + axis + " angle: " + angle);

                angles[i] = angle;
            }

            float temp = (target.position - Joints[Joints.Length - 1].transform.position).magnitude;
            if (temp < DistanceThreshold)
                finished = true;
        }
    }

    private void LateUpdate()
    {
        foreach (var B in Joints)
        {
            if (B.transform.parent == null)
                continue;
            Debug.DrawLine(B.transform.position, B.transform.parent.position, Color.red);
        }
    }

    public void InverseKinematics(Vector3 target, float[] angles)
    {
        for (int i = 0; i < Joints.Length; i++)
        {
            // Gradient descent
            // Update: Solution -= LearningRate * Gradient
            float gradient = PartialGradient(target, angles, i);
            angles[i] -= LearningRate * gradient;
            Joints[i].transform.Rotate(angles[i], 0, 0);

            // Clamp
            //angles[i] = Mathf.Clamp(angles[i], Joints[i].MinAngle, Joints[i].MaxAngle);
        }
    }

    public Vector3 ForwardKinematics(float[] angles)
    {
        Vector3 prevPoint = Joints[0].transform.position;
        Quaternion rotation = Quaternion.identity;
        for (int i = 1; i < Joints.Length; i++)
        {
            rotation *= Quaternion.AngleAxis(angles[i - 1], Joints[i - 1].Axis);
            Vector3 nextPoint = prevPoint + rotation * Joints[i].StartOffset;
            prevPoint = nextPoint;
        }
        //STEP1: MAKE SURE YOUR JOINTS "WOULD BE" WHERE YOUR FORWARD KINEMATICS SAY THEY ARE

        return prevPoint;
    }

    public float DistanceFromTarget(Vector3 target, float[] angles)
    {
        Vector3 point = ForwardKinematics(angles);
        return Vector3.Distance(point, target);
    }

    public float PartialGradient(Vector3 target, float[] angles, int i)
    {
        float angle = angles[i];

        float f_x = DistanceFromTarget(target, angles);
        angles[i] += SamplingDistance;
        float f_x_plus_d = DistanceFromTarget(target, angles);
        float gradient = (f_x_plus_d - f_x) / SamplingDistance;

        angles[i] = angle;

        return gradient;
    }*/
}