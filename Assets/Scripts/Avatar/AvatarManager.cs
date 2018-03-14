﻿// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using UnityEngine;

public class AvatarManager : MonoBehaviour
{
    public AvatarControllerType controllerType;
    private AvatarController controller;

    [SerializeField]
    private AvatarBody body;

    private void Start()
    {
        controller = AvatarControllerFactory.Create(controllerType, body);
    }

    private void Update()
    {
        controller.Update();
    }

    public AvatarRig ControllerRig { get { return controller.Rig; } }
    public AvatarBody Body { get { return body; } }
}