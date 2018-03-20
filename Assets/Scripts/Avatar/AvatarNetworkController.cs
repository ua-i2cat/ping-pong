// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using UnityEngine;

// Controlls an avatar using the message received over the network
public class AvatarNetworkController : AvatarController
{
    public AvatarNetworkController(AvatarBody body, string name) : base(body)
    {
        Debug.Log("AvatarNetworkController constructor");
        rig = new AvatarRigNet(body, name);
    }

    public override void Update()
    {
        base.Update();
    }
}