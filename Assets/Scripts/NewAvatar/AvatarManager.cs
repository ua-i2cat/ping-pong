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
        public int minimumSensors = 3;
        public bool isClient;

        private void Start()
        {
            controller = AvatarFactory.Create(type, body, isClient, minimumSensors);
        }

        private void Update()
        {
            controller.Update();
        }
    }
}