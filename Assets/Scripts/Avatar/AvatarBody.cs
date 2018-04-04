// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using UnityEngine;

namespace AvatarSystem
{
    [RequireComponent(typeof(Animator))]
    public class AvatarBody : MonoBehaviour
    {
        private Animator animator;
        private Action<Animator> IKAction = null;

        public bool isColliding = false;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public void Translate(Vector3 v)
        {
            transform.Translate(v);
        }

        public void Rotate(Vector3 euler)
        {
            transform.Rotate(euler);
        }
        
        public void SetIK(Action<Animator> IKAction)
        {
            this.IKAction = IKAction;
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (this.IKAction != null)
                IKAction(animator);
        }

        // Move the body back a little on collision
        private void OnCollisionEnter(Collision collision)
        {
            Vector3 v = new Vector3(collision.contacts[0].point.x, 0, collision.contacts[0].point.z) - transform.position;
            transform.position -= v.normalized * 0.1f;
            isColliding = true;
        }

        // In case the player stays stuck
        private void OnCollisionStay(Collision collision)
        {
            isColliding = false;
        }

        private void OnCollisionExit(Collision collision)
        {
            isColliding = false;
        }
    }
}