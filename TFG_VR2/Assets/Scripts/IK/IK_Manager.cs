// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IK_Manager : MonoBehaviour
{
    public Transform[] joints;
    private Constrain_minmaxangle[] joint_constraints_angle = null;

    public Transform target;
    public bool solveIK = false;
    public IK_Solver.SolverId solverType;
    public float threshold = 0.1f;

    private IK_Solver solver;

    private Quaternion q;

    private void Start()
    {
        Quaternion offset = Quaternion.Inverse(joints[joints.Length - 1].rotation) * target.rotation;
        q = Quaternion.Inverse(offset);

        // Find constraints in the joints hierarchy
        joint_constraints_angle = FindConstraints(joints);
       // Constrain_angle a = joints[0].gameObject.GetComponent<Constrain_angle>();
    }

    private Constrain_minmaxangle[] FindConstraints(Transform[] joints)
    {
        if(joint_constraints_angle == null)
            joint_constraints_angle = new Constrain_minmaxangle[joints.Length];

        for(int i = 0; i < joints.Length; i++)
        {
            Constrain_minmaxangle constraint = joints[i].GetComponent<Constrain_minmaxangle>();
            if (constraint != null)
                joint_constraints_angle[i] = constraint;
        }

        return joint_constraints_angle;
    }

    void Update ()
    {     
        if(target.hasChanged)
        {
            solveIK = true;
            target.hasChanged = false;
        }

		if(solveIK)
        {
            solver.Solve(joints, target, threshold, joint_constraints_angle);

            // Apply rotation of the target to the end effector
            joints[joints.Length - 1].rotation = target.rotation * q;

            solveIK = false;
        }

	}

    // Create solver object from the enum selected in the Editor so we can switch the solver
    // This method is also called at the Start
    void OnValidate()
    {
        solver = (IK_Solver)Activator.CreateInstance(IK_Solver.Solvers[solverType]);
        Debug.Assert(solver != null);
    }

    private Constrainer GetConstraints(Transform[] joints)
    {
        var constrainers = GetComponent<JointConstrainer>();

        if (constrainers)
            return constrainers.constrainer;
        return null;
    }
}
