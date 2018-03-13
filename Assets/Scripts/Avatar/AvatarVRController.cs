// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using UnityEngine;

// Controlls an avatar using the VR headset, trackers and controllers
public class AvatarVRController : AvatarController
{
    public AvatarVRController(AvatarBody body) : base(body)
    {
        rig = new AvatarRig(body);
        body.SetIKAction(OnIKAction);
    }

    private void OnIKAction(Animator animator)
    {
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
        animator.SetIKPosition(AvatarIKGoal.RightHand, new Vector3(0, 0, 0));
    }
}
