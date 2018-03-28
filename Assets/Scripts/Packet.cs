// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

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
        Objects,        // [S->C]

        Benchmark,
    };

    private List<byte> data = new List<byte>();

    public Packet(List<byte> data)
    {
        this.data = data;
        Debug.Assert(Size == BitConverter.ToInt16(data.ToArray(), 0), "Incorrect Packet format. Check size of data");
    }

    public short Size { get { return (short)data.Count; } }

    public PacketType Type { get { return (PacketType)data[2]; } }

    public byte[] ToArray() { return data.ToArray(); }

    public override string ToString()
    {
        object content = null;
        switch (Type)
        {
            case Packet.PacketType.Text:
                content = Encoding.ASCII.GetString(data.ToArray(), 3, data.Count - 3);
                break;

            case Packet.PacketType.Spawn:
                float x = BitConverter.ToSingle(data.ToArray(), 3 + 0);
                float y = BitConverter.ToSingle(data.ToArray(), 3 + 4);
                float z = BitConverter.ToSingle(data.ToArray(), 3 + 8);
                content = new Vector3(x, y, z);
                break;
        }

        return Type + ": " + content + " (" + Size + " bytes)";
    }

    public void Send(Socket socket, AsyncCallback callback = null)
    {
        if (socket.Connected && this.Size > 0)
        {
            socket.BeginSend(this.ToArray(), 0, this.Size, SocketFlags.None,
                    callback, socket);
        }
    }
}

public class PacketText
{
    private Packet p;

    private PacketText(Packet p)
    {
        this.p = p;
    }

    public static explicit operator PacketText(Packet p)
    {
        return new PacketText(p);
    }

    public string Data
    {
        get
        {
            return Encoding.ASCII.GetString(p.ToArray(), 3, p.Size - 3);
        }
    }
}

public class PacketSensors
{
    private Packet p;

    private PacketSensors(Packet p)
    {
        this.p = p;
    }

    public static explicit operator PacketSensors(Packet p)
    {
        return new PacketSensors(p);
    }

    public List<Trans> Data
    {
        get
        {
            List<Trans> data = new List<Trans>();

            int dataIndex = 3;
            int transformCount = p.ToArray()[dataIndex++];

            for(int i = 0; i < transformCount; i++)
            {
                string name = Encoding.ASCII.GetString(p.ToArray(), dataIndex, 4); dataIndex += 4;

                float x = BitConverter.ToSingle(p.ToArray(), dataIndex); dataIndex += 4;
                float y = BitConverter.ToSingle(p.ToArray(), dataIndex); dataIndex += 4;
                float z = BitConverter.ToSingle(p.ToArray(), dataIndex); dataIndex += 4;

                float qx = BitConverter.ToSingle(p.ToArray(), dataIndex); dataIndex += 4;
                float qy = BitConverter.ToSingle(p.ToArray(), dataIndex); dataIndex += 4;
                float qz = BitConverter.ToSingle(p.ToArray(), dataIndex); dataIndex += 4;
                float qw = BitConverter.ToSingle(p.ToArray(), dataIndex); dataIndex += 4;

                Vector3 pos = new Vector3(x, y, z);
                Quaternion rot = new Quaternion(qx, qy, qz, qw);
                data.Add(new Trans(pos, rot, name));
            }

            return data;
        }
    }
}

public class PacketSpawn
{
    private Packet p;

    private PacketSpawn(Packet p)
    {
        this.p = p;
    }

    public static explicit operator PacketSpawn(Packet p)
    {
        return new PacketSpawn(p);
    }

    public Trans Data
    {
        get
        {
            int dataIndex = 3;
            string name = Encoding.ASCII.GetString(p.ToArray(), dataIndex, 4); dataIndex += 4;

            float x = BitConverter.ToSingle(p.ToArray(), dataIndex); dataIndex += 4;
            float y = BitConverter.ToSingle(p.ToArray(), dataIndex); dataIndex += 4;
            float z = BitConverter.ToSingle(p.ToArray(), dataIndex); dataIndex += 4;

            float qx = BitConverter.ToSingle(p.ToArray(), dataIndex); dataIndex += 4;
            float qy = BitConverter.ToSingle(p.ToArray(), dataIndex); dataIndex += 4;
            float qz = BitConverter.ToSingle(p.ToArray(), dataIndex); dataIndex += 4;
            float qw = BitConverter.ToSingle(p.ToArray(), dataIndex); dataIndex += 4;

            Vector3 pos = new Vector3(x, y, z);
            Quaternion rot = new Quaternion(qx, qy, qz, qw);
            Trans t = new Trans(pos, rot, name);
            return t;
        }
    }
}

public class PacketOtherClients
{
    private Packet p;

    private PacketOtherClients(Packet p)
    {
        this.p = p;
    }

    public static explicit operator PacketOtherClients(Packet p)
    {
        return new PacketOtherClients(p);
    }

    public List<Oponent> Data
    {
        get
        {
            List<Oponent> clients = new List<Oponent>();

            int dataIndex = 3;
            int clientCount = p.ToArray()[dataIndex++];
            for(int i = 0; i < clientCount; i++)
            {
                int clientId = BitConverter.ToInt32(p.ToArray(), dataIndex); dataIndex += 4;
                Oponent oponent = new Oponent(clientId);
                int transformCount = p.ToArray()[dataIndex++];
                for(int j = 0; j < transformCount; j++)
                {
                    string name = Encoding.ASCII.GetString(p.ToArray(), dataIndex, 4); dataIndex += 4;

                    float x = BitConverter.ToSingle(p.ToArray(), dataIndex); dataIndex += 4;
                    float y = BitConverter.ToSingle(p.ToArray(), dataIndex); dataIndex += 4;
                    float z = BitConverter.ToSingle(p.ToArray(), dataIndex); dataIndex += 4;

                    float qx = BitConverter.ToSingle(p.ToArray(), dataIndex); dataIndex += 4;
                    float qy = BitConverter.ToSingle(p.ToArray(), dataIndex); dataIndex += 4;
                    float qz = BitConverter.ToSingle(p.ToArray(), dataIndex); dataIndex += 4;
                    float qw = BitConverter.ToSingle(p.ToArray(), dataIndex); dataIndex += 4;

                    Vector3 pos = new Vector3(x, y, z);
                    Quaternion rot = new Quaternion(qx, qy, qz, qw);
                    Trans t = new Trans(pos, rot, name);
                    oponent.AddTransform(t);
                }
                clients.Add(oponent);
            }

            return clients;
        }
    }
}

public class PacketObjects
{
    private Packet p;

    private PacketObjects(Packet p)
    {
        this.p = p;
    }

    public static explicit operator PacketObjects(Packet p)
    {
        return new PacketObjects(p);
    }

    public List<Trans> Data
    {
        get
        {
            List<Trans> objects = new List<Trans>();

            int dataIndex = 3;
            int objCount = p.ToArray()[dataIndex++];
            for (int i = 0; i < objCount; i++)
            {
                string name = Encoding.ASCII.GetString(p.ToArray(), dataIndex, 4); dataIndex += 4;

                float x = BitConverter.ToSingle(p.ToArray(), dataIndex); dataIndex += 4;
                float y = BitConverter.ToSingle(p.ToArray(), dataIndex); dataIndex += 4;
                float z = BitConverter.ToSingle(p.ToArray(), dataIndex); dataIndex += 4;

                float qx = BitConverter.ToSingle(p.ToArray(), dataIndex); dataIndex += 4;
                float qy = BitConverter.ToSingle(p.ToArray(), dataIndex); dataIndex += 4;
                float qz = BitConverter.ToSingle(p.ToArray(), dataIndex); dataIndex += 4;
                float qw = BitConverter.ToSingle(p.ToArray(), dataIndex); dataIndex += 4;

                Vector3 pos = new Vector3(x, y, z);
                Quaternion rot = new Quaternion(qx, qy, qz, qw);

                objects.Add(new Trans(pos, rot, name));
            }

            return objects;
        }
    }
}

public class PacketBenchmark
{
    private Packet p;

    private PacketBenchmark(Packet p)
    {
        this.p = p;
    }

    public static explicit operator PacketBenchmark(Packet p)
    {
        return new PacketBenchmark(p);
    }

    public NetBenchmarks Data
    {
        get
        {
            long sendTimeStamp = BitConverter.ToInt64(p.ToArray(), 3);
            long recvTimeStamp = BitConverter.ToInt64(p.ToArray(), 11);
            return new NetBenchmarks(sendTimeStamp, recvTimeStamp);
        }
    }
}
