// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using UnityEngine;

[RequireComponent(typeof(Animator))]
public class IKController : MonoBehaviour
{
    private Animator animator;

    public Transform Head;
    public Transform RHand;
    public Transform LHand;
    public Transform RFoot;
    public Transform LFoot;

    Vector3 rootToHead;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        Transform hmdOffset = transform.Find("hmdOffset");
        if (hmdOffset)
        {
            rootToHead = hmdOffset.position - transform.position;
        }

        //transform.position = Head.position - rootToHead;
        transform.rotation = Quaternion.Euler(0, Head.rotation.eulerAngles.y, 0);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (Head)
        {
            animator.SetLookAtWeight(1);
            animator.SetLookAtPosition(Head.position + Head.forward);
        }

        SetIKGoal(AvatarIKGoal.RightHand, RHand);
        SetIKGoal(AvatarIKGoal.LeftHand, LHand);

        SetIKGoal(AvatarIKGoal.RightFoot, RFoot);
        SetIKGoal(AvatarIKGoal.LeftFoot, LFoot);
    }

    private void SetIKGoal(AvatarIKGoal goal, Transform t)
    {
        if (t)
        {
            animator.SetIKPositionWeight(goal, 1);
            animator.SetIKRotationWeight(goal, 1);
            animator.SetIKPosition(goal, t.position);
            animator.SetIKRotation(goal, t.rotation);
        }
    }
}
