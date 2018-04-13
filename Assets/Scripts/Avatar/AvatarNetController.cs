// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections.Generic;

namespace AvatarSystem
{
    public class AvatarNetController : AvatarController
    {
        public AvatarNetController(AvatarBody body)
        {
            this.type = AvatarControllerType.NETWORK;
            this.body = body;
        }

        public override List<Trans> GetTransforms()
        {
            throw new System.NotImplementedException();
        }

        public override void SetTransforms(List<Trans> transforms)
        {
            throw new System.NotImplementedException();
        }

        public override void Update()
        {
            //throw new System.NotImplementedException();
        }
    }
}