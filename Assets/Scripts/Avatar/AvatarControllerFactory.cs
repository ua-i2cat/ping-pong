// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

class AvatarControllerFactory
{
    public static AvatarController Create(AvatarControllerType type, AvatarBody body)
    {
        switch(type)
        {
            case AvatarControllerType.Network:
                return new AvatarNetworkController(body, "");

            case AvatarControllerType.VR:
                return new AvatarVRController(body);

            case AvatarControllerType.Keyboard:
            default:
                return new AvatarKeyboardController(body);
        }
    }
}
