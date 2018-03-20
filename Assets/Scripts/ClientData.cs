// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class ClientData
{
    public Socket socket;
    private Dictionary<string, Trans> transforms = new Dictionary<string, Trans>();
    public GameObject instance;
    public Trans spawn;

    public const int BUFF_SIZE = 8192;
    public byte[] recvBuffer = new byte[BUFF_SIZE];

    private System.Object transformLock = new System.Object();

    public ClientData(Socket socket)
    {
        this.socket = socket;
    }

    public void SetTransform(string name, Trans value)
    {
        lock (transformLock)
        {
            transforms[name] = value;
            //if(transforms.Count > 7)
            //    Debug.Log(transforms.Count + " name: " + name + " Id: " + value.Id);
        }
    }

    public Trans GetTransform(string name)
    {
        lock (transformLock)
        {
            if (transforms.ContainsKey(name))
                return transforms[name];
            else
                Debug.LogWarning("Trying to GetTransform with invalid key");
            return null;
        }
    }

    public int TransformCount
    {
        get
        {
            lock (transformLock)
            {
                return transforms.Count;
            }
        }
    }

    public List<string> TransformKeys
    {
        get
        {
            lock (transformLock)
            {
                List<string> keys = new List<string>();
                foreach (var key in transforms.Keys)
                    keys.Add(key);
                return keys;
            }
        }
    }

    public List<byte> Serialize()
    {
        List<byte> data = new List<byte>();
        data.AddRange(BitConverter.GetBytes(socket.GetHashCode())); // Client id
        data.Add((byte)transforms.Count);                           // Transform Count
        if (instance == null)
        {
            Debug.LogWarning("Null instance in Serialize");
            return new List<byte>();
        }
        foreach (Transform t in instance.transform /*transforms*/)
            //data.AddRange(t.Value.Serialize());
            data.AddRange(new Trans(t.position, t.rotation, t.name).Serialize());
        return data;
    }
}
