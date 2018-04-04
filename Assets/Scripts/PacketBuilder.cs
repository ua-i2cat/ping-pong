// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class PacketBuilder
{
    public static Packet Build(Packet.PacketType type, object data = null)
    {
        List<byte> content = new List<byte>();
        content.AddRange(GetPacketSize(type, data));    // 2 bytes for the size
        content.Add((byte)type);                        // 1 byte for the type
        content.AddRange(GetPacketData(type, data));    // X bytes for the data

        Packet packet = new Packet(content);
        return packet;
    }

    public static Packet Parse(byte[] data, ref int dataIndex)
    {
        int size = BitConverter.ToInt16(data, dataIndex);
        List<byte> content = data.ToList().GetRange(dataIndex, size);
        dataIndex += size;

        Packet packet = new Packet(content);
        return packet;
    }

    private static IEnumerable<byte> GetPacketSize(Packet.PacketType type, object data = null)
    {
        int size = Constants.HEADER_SIZE; // 2 bytes for the size + 1 byte for the type

        switch (type)
        {
            case Packet.PacketType.Text:
                string text = (string)data;
                size += text.Length;
                break;

            case Packet.PacketType.Sensors:
                List<Trans> transforms = (List<Trans>)data;
                size += 1; // Number of transforms
                size += transforms.Count * Trans.Size;
                break;

            case Packet.PacketType.Spawn:
                size += Trans.Size;
                break;

            case Packet.PacketType.OtherClients:
                List<ClientData> clients = (List<ClientData>)data;
                size += 1; // Number of clients (0-255)
                foreach (var client in clients)
                    size += sizeof(int) + sizeof(byte) + client.TransformCount * Trans.Size;
                break;

            case Packet.PacketType.Objects:
                List<Trans> objects = (List<Trans>)data;
                size += 1; // Number of objects (0-255)
                foreach (var obj in objects)
                    size += Trans.Size;
                break;

            case Packet.PacketType.Benchmark:
                size += NetBenchmarks.Size;
                break;

            default:
                throw new ArgumentException("Invalid PacketType!");
        }

        Debug.Assert(size <= short.MaxValue);
        return BitConverter.GetBytes((short)size);
    }

    private static IEnumerable<byte> GetPacketData(Packet.PacketType type, object data = null)
    {
        List<byte> content = new List<byte>();

        switch (type)
        {
            case Packet.PacketType.Text:
                string text = (string)data;
                content.AddRange(Encoding.ASCII.GetBytes(text));
                break;

            case Packet.PacketType.Sensors:
                List<Trans> transforms = (List<Trans>)data;
                content.Add((byte)transforms.Count);
                foreach (var transform in transforms)
                    content.AddRange(transform.Serialize());
                break;

            case Packet.PacketType.Spawn:
                Trans spawn = (Trans)data;
                content.AddRange(spawn.Serialize());
                break;

            case Packet.PacketType.OtherClients:
                List<ClientData> clients = (List<ClientData>)data;
                content.Add((byte)clients.Count);
                foreach (var client in clients)
                    content.AddRange(client.Serialize());
                break;

            case Packet.PacketType.Objects:
                List<Trans> objects = (List<Trans>)data;
                content.Add((byte)objects.Count);
                foreach (var trans in objects)
                    content.AddRange(trans.Serialize());
                break;

            case Packet.PacketType.Benchmark:
                NetBenchmarks benchmarks = (NetBenchmarks)data;
                content.AddRange(BitConverter.GetBytes(benchmarks.sendTimeStamp));
                content.AddRange(BitConverter.GetBytes(benchmarks.recvTimeStamp));
                break;

            default:
                throw new ArgumentException("Invalid PacketType!");
        }

        return content;
    }
}
