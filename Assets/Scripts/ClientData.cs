// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
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
            if (!transforms.ContainsKey(name))
                Debug.Log("Adding transform " + name + " for the first time");

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

public class ClientDataUDP
{
    public IPEndPoint endPoint;
    public Trans spawn;
    public int TTL = 100;

    private GameObject instance;

    public ClientDataUDP(IPEndPoint ep)
    {
        endPoint = ep;
    }

    private int hasTransforms = 0;
    private List<Trans> transforms;
    public void Update()
    {
        if(hasTransforms > 0 && transforms != null)
        {
            // Instantiate if a client is connected and doesn't have an instance yet
            if (instance == null && transforms.Count > 0)
            {
                instance = new GameObject(endPoint.ToString());
                foreach (Trans t in transforms)
                {
                    GameObject obj;
                    if (t.Id.Contains(Constants.RightHand))
                    {
                        // @TODO: Create a prefab for this! Hardcoded collider for now.
                        obj = new GameObject(Constants.RightHand);
                        obj.transform.localScale = Constants.RightHandScale;
                        var collider = obj.AddComponent<BoxCollider>();
                        collider.center = Constants.ColliderCenter;
                        collider.size = Constants.ColliderSize;
                    }
                    else
                    {
                        obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        GameObject.Destroy(obj.GetComponent<SphereCollider>());
                        obj.transform.localScale = Constants.SphereScale;
                    }
                    obj.name = t.Id;
                    obj.transform.parent = instance.transform;
                    obj.transform.position = t.Pos;
                    obj.transform.rotation = t.Rot;
                }
            }
            // Update already instanced clients
            else if (instance != null && transforms.Count > 0)
            {
                foreach (Trans t in transforms)
                {
                    Transform current = instance.transform.Find(t.Id);
                    if (current != null)
                    {
                        current.position = t.Pos;
                        current.rotation = t.Rot;
                    }
                }
            }

            Interlocked.Decrement(ref hasTransforms);
        }
    }

    public void Update(List<Trans> transforms)
    {
        this.transforms = transforms;
        if (hasTransforms <= 0)
            Interlocked.Increment(ref hasTransforms);
    }

    public void Destroy()
    {
        if (instance != null/* && TTL == 0*/)
        {
            GameObject.Destroy(instance);
        }
    }

    public int TransformCount
    {
        get
        {
            if(instance != null)
                return instance.transform.childCount;

            return 0;
        }
    }

    public List<byte> Serialize()
    {
        List<byte> data = new List<byte>();
        data.AddRange(BitConverter.GetBytes(endPoint.GetHashCode())); // Client id
        data.Add((byte)TransformCount);                           // Transform Count
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