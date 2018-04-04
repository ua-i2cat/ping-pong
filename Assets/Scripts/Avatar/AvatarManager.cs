// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using UnityEngine;

namespace AvatarSystem
{
    public class AvatarManager : MonoBehaviour
    {
        public AvatarBody body;
        private AvatarController controller;
        public AvatarControllerType type;
        public int minimumSensors = 3;      // 6 for full-body tracking
        public bool isClient;

        private void Start()
        {
            controller = AvatarFactory.Create(type, body, isClient, minimumSensors);
        }

        private void Update()
        {
            controller.Update();
        }

        public AvatarController GetController()
        {
            return controller;
        }
    }
}