// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

// This class defines how an avatar is controlled
public abstract class AvatarController
{
    public AvatarController(AvatarBody body)
    {
        this.body = body;
    }

    public virtual void Update() { Rig.Update(); }

    public AvatarRig Rig { get { return rig; } }

    protected AvatarBody body;
    protected AvatarRig rig;
}

public enum AvatarControllerType
{
    Keyboard,
    VR,
    Network,
}
