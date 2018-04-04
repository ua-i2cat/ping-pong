// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using System.Collections.Generic;

namespace AvatarSystem
{
    public enum AvatarControllerType
    {
        KEYBOARD,
        SENSORS,
        NETWORK,
    }

    public abstract class AvatarController
    {
        protected AvatarBody body;

        protected AvatarControllerType type;

        public abstract void Update();

        // Returns the Transforms required to control the avatar
        public abstract List<Trans> GetTransforms();

        public abstract void SetTransforms(List<Trans> transforms);
    }

    public static class AvatarFactory
    {
        public static AvatarController Create(AvatarControllerType type, AvatarBody body, 
            bool isClient, int minimumSensors)
        {
            switch(type)
            {
                case AvatarControllerType.KEYBOARD:
                    return new AvatarKeyboardController(body);

                case AvatarControllerType.SENSORS:
                    return new AvatarSensorsController(body, isClient, minimumSensors);

                case AvatarControllerType.NETWORK:
                    return new AvatarNetController(body);

                default:
                    throw new ArgumentException("Invalid AvatarControllerType");
            }
        }
    }
}