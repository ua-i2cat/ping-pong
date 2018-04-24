// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;

namespace Net
{
    public static class ServerFactory
    {
        public static Server Create(Protocol protocol)
        {
            switch(protocol)
            {
                case Protocol.Tcp:
                    return new ServerTCP();
                case Protocol.Udp:
                    return new ServerUDP();
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}