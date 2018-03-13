// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class gIKControl : MonoBehaviour
{
    protected Animator animator;

    public bool ikActive = false;

    public bool footTrackingEnabled = false;
        
    public Transform hmd                = null;
    public Transform leftCtrl           = null;
    public Transform rightCtrl          = null;
    public Transform trackedHip         = null;
    public Transform trackedLeftFoot    = null;
    public Transform trackedRightFoot   = null;

    private Vector3 lookPos;
    private Vector3 hmdOffset;

    void Start()
    {
        animator = GetComponent<Animator>();
        //hmdOffset = transform.Find("hmdOffset").position - transform.position;
    }

    private void Update()
    {        
        //transform.position = new Vector3(hmd.position.x, hmd.position.y - hmdOffset.y, hmd.position.z - hmdOffset.z);
        lookPos = hmd.position + hmd.forward;

        if(trackedHip != null)
        {
            //Debug.Log("TrackedHip " + trackedHip.position.ToString());

            // Only rotate on the Y axis
            transform.rotation = Quaternion.Euler(0, trackedHip.rotation.eulerAngles.y, 0);
            transform.position = new Vector3(trackedHip.position.x, 0, trackedHip.position.z) - transform.forward * 0.15f;
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, hmd.rotation.eulerAngles.y, 0);
            transform.position = new Vector3(hmd.position.x, 0, hmd.position.z) - transform.forward * 0.15f;
        }
    }

    void OnAnimatorIK()
    {
        if (animator)
        {
            //if the IK is active, set the position and rotation directly to the goal. 
            if (ikActive)
            {
                // HEAD -- HTC Vive HMD
                // Set the look target position for the HEAD, if one has been assigned
                // the look-at target is an empty GO which extends out from the HTC Vive CenterEye
                if (hmd != null)
                {
                    animator.SetLookAtWeight(1);
                    animator.SetLookAtPosition(lookPos);
                }
                // HAND -- Right / HTC Vive Controller
                // Set the right hand target position and rotation, if one has been assigned
                if (rightCtrl != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKPosition(AvatarIKGoal.RightHand, rightCtrl.position);
                    animator.SetIKRotation(AvatarIKGoal.RightHand, rightCtrl.rotation);
                }
                // HAND -- Left / HTC Vive Controller
                // Set the LEFT hand target position and rotation, if one has been assigned
                if (leftCtrl != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                    animator.SetIKPosition(AvatarIKGoal.LeftHand, leftCtrl.position);
                    animator.SetIKRotation(AvatarIKGoal.LeftHand, leftCtrl.rotation);
                }

                if(footTrackingEnabled)
                {
                    if (trackedLeftFoot != null)
                    {
                        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
                        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
                        animator.SetIKPosition(AvatarIKGoal.LeftFoot, trackedLeftFoot.position);
                        animator.SetIKRotation(AvatarIKGoal.LeftFoot, trackedLeftFoot.rotation);
                    }
                    if (trackedRightFoot != null)
                    {
                        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
                        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);
                        animator.SetIKPosition(AvatarIKGoal.RightFoot, trackedRightFoot.position);
                        animator.SetIKRotation(AvatarIKGoal.RightFoot, trackedRightFoot.rotation);
                    }
                }                
            }

            // if the IK is not active, set the position and rotation 
            // of the hands and head back to the original position
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);

                animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0);
                animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0);

                animator.SetLookAtWeight(0);
            }
        }
    }
}