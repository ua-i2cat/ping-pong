// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class Trans
{
    private Vector3 pos;
    private Quaternion rot;
    private string id; // 4 chars max

    private System.Object transLock = new System.Object();

    public Trans(Vector3 pos, Quaternion rot, string id = "")
    {
        Debug.Assert(id.Length <= 4, "id.Length is too long (4 chars MAX)");
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
            int size = 4;
            size += Marshal.SizeOf(typeof(Vector3));
            size += Marshal.SizeOf(typeof(Quaternion));
            return size;
        }
    }

    public List<byte> Serialize()
    {
        List<byte> data = new List<byte>();

        byte[] name = new byte[4];
        Encoding.ASCII.GetBytes(id, 0, id.Length, name, 0);
        data.AddRange(name);
        //data.AddRange(Encoding.ASCII.GetBytes(id));

        data.AddRange(BitConverter.GetBytes(Pos.x));
        data.AddRange(BitConverter.GetBytes(Pos.y));
        data.AddRange(BitConverter.GetBytes(Pos.z));

        data.AddRange(BitConverter.GetBytes(Rot.x));
        data.AddRange(BitConverter.GetBytes(Rot.y));
        data.AddRange(BitConverter.GetBytes(Rot.z));
        data.AddRange(BitConverter.GetBytes(Rot.w));

        return data;
    }

    public static Trans Deserialize(byte[] data, ref int dataIndex)
    {
        string name = Encoding.ASCII.GetString(data, dataIndex, 4); dataIndex += 4;

        float x = BitConverter.ToSingle(data, dataIndex); dataIndex += 4;
        float y = BitConverter.ToSingle(data, dataIndex); dataIndex += 4;
        float z = BitConverter.ToSingle(data, dataIndex); dataIndex += 4;

        float qx = BitConverter.ToSingle(data, dataIndex); dataIndex += 4;
        float qy = BitConverter.ToSingle(data, dataIndex); dataIndex += 4;
        float qz = BitConverter.ToSingle(data, dataIndex); dataIndex += 4;
        float qw = BitConverter.ToSingle(data, dataIndex); dataIndex += 4;

        Vector3 pos = new Vector3(x, y, z);
        Quaternion rot = new Quaternion(qx, qy, qz, qw);
        return new Trans(pos, rot, name);
    }
}