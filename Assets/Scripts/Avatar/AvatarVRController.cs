// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections.Generic;
using UnityEngine;

// Controlls an avatar using the VR headset, trackers and controllers
public class AvatarVRController : AvatarController
{
    public AvatarVRController(AvatarBody body) : base(body)
    {
        Debug.Log("AvatarVRController constructor");
        rig = new AvatarRigVR(body);
        body.SetIKAction(OnIKAction);
    }

    // Move the body to follow the Rig
    public override void Update()
    {
        base.Update();

        // Rotate the body in the same direction as the Camera
        body.transform.rotation = Quaternion.Euler(new Vector3(0, rig.GetRigEye().eulerAngles.y, 0));

        if(rig.GetTransform(Constants.Hip).Key != null)
        {
            //throw new System.NotImplementedException();
        }
        else
        {
            body.transform.position += rig.GetRigEye().position - body.GetBodyEye().position;
        }

        if(rig.GetTransform(Constants.RightFoot).Key != null)
        {
            //throw new System.NotImplementedException();
        }
    }

    private void OnIKAction(Animator animator)
    {
        Transform eye = rig.GetRigEye();

        animator.SetLookAtWeight(1);
        animator.SetLookAtPosition(eye.position + eye.forward);

        SetIKGoal(animator, AvatarIKGoal.LeftHand, rig.GetTransform(Constants.LeftHand));
        SetIKGoal(animator, AvatarIKGoal.RightHand, rig.GetTransform(Constants.RightHand));
    }

    private void SetIKGoal(Animator animator, AvatarIKGoal goal, KeyValuePair<string, Transform> pair)
    {
        if (pair.Key != null)
        {
            animator.SetIKPositionWeight(goal, 1);
            animator.SetIKPosition(goal, pair.Value.position);

            animator.SetIKRotationWeight(goal, 1);
            animator.SetIKRotation(goal, pair.Value.rotation);
        }
    }
}
