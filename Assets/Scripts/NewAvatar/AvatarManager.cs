// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using UnityEngine;

namespace avatar
{
    public class AvatarManager : MonoBehaviour
    {
        public AvatarBody body;
        public AvatarController controller;
        public AvatarControllerType type;

        private void Start()
        {
            controller = AvatarFactory.Create(type, body);
        }

        private void Update()
        {
            controller.Update();
        }
    }
}