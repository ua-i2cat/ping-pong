// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

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

public class Trans
{
    private Vector3 pos;
    private Quaternion rot;
    private string id; // 4 chars max

    private System.Object transLock = new System.Object();

    public Trans(Vector3 pos, Quaternion rot, string id = "")
    {
        this.pos = pos;
        this.rot = rot;
        this.id = id;
    }

    public string Id { get { return id; } }

    public Vector3 Pos
    {
        get { lock (transLock) { return pos; } }
        set { lock (transLock) { pos = value; } }
    }

    public Quaternion Rot
    {
        get { lock (transLock) { return rot; } }
        set { lock (transLock) { rot = value; } }
    }

    public static int Size
    {
        get
        {
            int size = sizeof(int);
            size += Marshal.SizeOf(typeof(Vector3));
            size += Marshal.SizeOf(typeof(Quaternion));
            return size;
        }
    }
}
