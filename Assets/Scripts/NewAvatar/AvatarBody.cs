// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using UnityEngine;

namespace avatar
{
    [RequireComponent(typeof(Animator))]
    public class AvatarBody : MonoBehaviour
    {
        private Animator animator;
        private Action<Animator> IKAction = null;

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
    }
}