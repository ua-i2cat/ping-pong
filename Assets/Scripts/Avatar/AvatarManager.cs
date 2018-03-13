// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using UnityEngine;

public class AvatarManager : MonoBehaviour
{
    public AvatarControllerType controllerType;
    private AvatarController controller;

    public AvatarBody body;

    private void Start()
    {
        controller = AvatarControllerFactory.Create(controllerType, body);
    }

    private void Update()
    {
        controller.Update();
    }
}
