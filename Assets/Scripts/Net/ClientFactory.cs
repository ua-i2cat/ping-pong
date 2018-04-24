// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;

namespace Net
{
    public static class ClientFactory
    {
        public static Client Create(Protocol protocol)
        {
            switch(protocol)
            {
                case Protocol.Tcp:
                    return new ClientTCP();
                case Protocol.Udp:
                    return new ClientUDP();
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}