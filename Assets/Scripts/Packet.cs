// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

public class Packet
{
    public enum PacketType
    {
        Text,
        Sensors,
        Spawn,
        OtherClients,
    };

    private short size = 0;
    private PacketType type = PacketType.Text;

    public short Size { get { return size; } }
    public PacketType Type { get { return type; } }
}
