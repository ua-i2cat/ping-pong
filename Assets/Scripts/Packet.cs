// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections.Generic;

public class Packet
{
    public enum PacketType
    {
        // Both Client and Server can send:
        Text,           // [X->X]

        // Only Client can send:
        Sensors,        // [C->S]

        // Only Server can send:
        Spawn,          // [S->C]
        OtherClients,   // [S->C]
    };

    private PacketType type = PacketType.Text;
    private List<byte> data = new List<byte>();

    public Packet(List<byte> data)
    {
        this.data = data;
    }

    public short Size
    {
        get
        {

            return (short)data.Count;
        }
    }

    public PacketType Type { get { return type; } }
}
