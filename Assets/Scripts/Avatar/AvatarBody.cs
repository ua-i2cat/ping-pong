// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AvatarBody : MonoBehaviour
{
    private Animator animator;
    private Action<Animator> onIKAction;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public Transform GetBodyEye()
    {
        return transform.Find("Eye");
    }

    public void Translate(Vector3 v)
    {
        transform.Translate(v);
    }
    public void Rotate(Vector3 euler)
    {
        transform.Rotate(euler);
    }

    public void SetIKAction(Action<Animator> onIKAction)
    {
        Debug.Log("Setting IK Action");
        this.onIKAction = onIKAction;
    }
    private void OnAnimatorIK(int layerIndex)
    {
        if(onIKAction != null)
        {
            onIKAction(animator);
        }
    }
}
