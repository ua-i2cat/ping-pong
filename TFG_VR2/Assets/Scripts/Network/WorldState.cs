// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public struct Trans
{
    public Vector3 pos;
    public Quaternion rot;

    public Trans(Vector3 p, Quaternion q)
    {
        pos = p;
        rot = q;
    }

    public static int Size
    {
        get
        {
            return (3 + 4) * sizeof(float);
        }        
    }

    public void SetPosRot(Transform t)
    {
        pos = t.position;
        rot = t.rotation;
    }
}

public class ClientInfo
{
    private System.Object clientLock = new System.Object();

    private UInt32 id;
    public Socket socket;

    public GameObject instance = null;
    private bool connected;

    // SET on client side
    public Trans hmd;
    public Trans leftController;
    public Trans rightController;
    public Trans trackerHip;
    public Trans trackerLeftFoot;
    public Trans trackerRightFoot;

    // SET on server side
    public List<Trans> transforms = new List<Trans>();

    private static UInt32 lastId = 0;   

    // Only use in the server
    public ClientInfo(Socket s)
    {
        Id = ++lastId;  // Assign ids, starting from 1
        socket = s;
        connected = true;
    }

    // Only use in the clients
    public ClientInfo(uint id)
    {
        Id = id;
        connected = false;
    }

    public UInt32 Id
    {
        get { return id; }
        private set { id = value; }
    }

    public bool Connected
    {
        get { lock (clientLock) { return connected; } }
        set { lock (clientLock) { connected = value; } }
    }
}

public class ServerObject
{
    public int id;
    public Vector3 pos;
    public Quaternion rot;
    public GameObject instance;

    // Used in server
    public ServerObject(GameObject obj)
    {
        Debug.Assert(obj != null);
        id = obj.GetInstanceID();
        pos = obj.transform.position;
        rot = obj.transform.rotation;
        instance = obj;
    }

    // Used in client
    public ServerObject(int id, float x, float y, float z)
    {
        this.id = id;
        pos = new Vector3(x, y, z);
        rot = Quaternion.identity;
    }
}

public enum Authority { Client, Server };

public class WorldState
{    
    public Authority authority;

    private System.Object sharedDataLock = new System.Object();
    public List<ClientInfo> clients = new List<ClientInfo>();
    private List<ServerObject> balls = new List<ServerObject>();

    public GameObject playerPrefab;
    public GameObject ballPrefab;

    public WorldState(Authority authority)
    {
        this.authority = authority;
    }

    public void AddClient(ClientInfo client)
    {
        if (Monitor.TryEnter(sharedDataLock, 5))
        {           
            clients.Add(client);
            Debug.Log("New client added to the World, ID: [" + client.Id + "]");
            Monitor.Exit(sharedDataLock);
        }
        else
        {
            Thread.Sleep(5);
            AddClient(client);
        }
    }

    public void AddBall(ServerObject ball)
    {
        Debug.Assert(Authority.Server == authority);
        if (Monitor.TryEnter(sharedDataLock, 5))
        {
            balls.Add(ball);
            Debug.Log("New ball added to the World, ID: [" + ball.id + "]");
            Monitor.Exit(sharedDataLock);
        }
        else
        {
            Thread.Sleep(5);
            AddBall(ball);
        }
    }

    public void Update()
    {
        // Update clients
        foreach (var client in clients)
        {
            if (Monitor.TryEnter(sharedDataLock, 5))
            {
                if (client.Connected)
                {
                    if (client.instance == null && playerPrefab != null)
                        client.instance = UnityEngine.Object.Instantiate(playerPrefab);

                    if (Authority.Server == authority)
                    {
                        // Update controls
                        gIKControl ik = client.instance.GetComponent<gIKControl>();

                        if (ik != null)
                        {
                            ik.hmd.position              = client.hmd.pos;
                            ik.hmd.rotation              = client.hmd.rot;
                            ik.leftCtrl.position         = client.leftController.pos;
                            ik.leftCtrl.rotation         = client.leftController.rot;
                            ik.rightCtrl.position        = client.rightController.pos;
                            ik.rightCtrl.rotation        = client.rightController.rot;
                            if (client.trackerHip.pos == Vector3.zero)
                            {
                                ik.trackedHip = null;
                            }
                            else
                            {
                                ik.trackedHip.position = client.trackerHip.pos;
                                ik.trackedHip.rotation = client.trackerHip.rot;
                            }
                            ik.trackedLeftFoot.position  = client.trackerLeftFoot.pos;
                            ik.trackedLeftFoot.rotation  = client.trackerLeftFoot.rot;
                            ik.trackedRightFoot.position = client.trackerRightFoot.pos;
                            ik.trackedRightFoot.rotation = client.trackerRightFoot.rot;
                        }

                        // Transfer transforms from the instance to the list
                        client.transforms.Clear();
                        GetInstanceTransforms(client.instance.transform, client.transforms);
                    }
                    else if (Authority.Client == authority && playerPrefab != null)
                    {
                        // Transfer from list to the instance transforms
                        int index = 0;
                        SetInstanceTransforms(client.instance.transform, client.transforms, ref index);
                    }
                }
                else
                {
                    if (client.instance != null)
                        UnityEngine.Object.Destroy(client.instance);
                }

                Monitor.Exit(sharedDataLock);
            }
        }

        // Update balls
        foreach(var ball in balls)
        {
            if (Authority.Server == authority)
            {
                ball.pos = ball.instance.transform.position;
                ball.rot = ball.instance.transform.rotation;
            }
            else if (Authority.Client == authority)
            {
                if (ball.instance == null && ballPrefab != null)
                {
                    ball.instance = UnityEngine.Object.Instantiate(ballPrefab, ball.pos, ball.rot);
                }

                ball.instance.transform.position = ball.pos;
                ball.instance.transform.rotation = ball.rot;
            }
        }

        RemoveDisconnectedClients();
    }

    public List<byte> ServerGetState(uint clientId)
    {
        List<byte> payload = new List<byte>();

        if (Monitor.TryEnter(sharedDataLock, 5))
        {
            int HEADER_SIZE     = sizeof(int) + sizeof(int) + sizeof(int);              // packet size + clients count + balls count
            int CLIENTS_SIZE    = 2 * sizeof(int) * clients.Count + GetClientsSize();   // id + transform count + transforms per client
            int BALLS_SIZE      = balls.Count * (3 * sizeof(float) + sizeof(int));      // 3 floats + 1 int per ball object
            int PACKET_SIZE     = HEADER_SIZE + CLIENTS_SIZE + BALLS_SIZE;

            payload.AddRange(BitConverter.GetBytes(PACKET_SIZE));
            payload.AddRange(BitConverter.GetBytes(clients.Count));
            payload.AddRange(BitConverter.GetBytes(balls.Count));

            // Clients Data
            foreach(var client in clients)
            {
                payload.AddRange(BitConverter.GetBytes(client.Id));
                payload.AddRange(BitConverter.GetBytes(client.transforms.Count));

                foreach(var transform in client.transforms)
                {
                    payload.AddRange(BitConverter.GetBytes(transform.pos.x));
                    payload.AddRange(BitConverter.GetBytes(transform.pos.y));
                    payload.AddRange(BitConverter.GetBytes(transform.pos.z));

                    payload.AddRange(BitConverter.GetBytes(transform.rot.x));
                    payload.AddRange(BitConverter.GetBytes(transform.rot.y));
                    payload.AddRange(BitConverter.GetBytes(transform.rot.z));
                    payload.AddRange(BitConverter.GetBytes(transform.rot.w));
                }           
            }

            // Balls Data
            foreach (var ball in balls)
            {
                payload.AddRange(BitConverter.GetBytes(ball.id));
                payload.AddRange(BitConverter.GetBytes(ball.pos.x));
                payload.AddRange(BitConverter.GetBytes(ball.pos.y));
                payload.AddRange(BitConverter.GetBytes(ball.pos.z));
            }

            Debug.Assert(payload.Count == PACKET_SIZE);
            Monitor.Exit(sharedDataLock);
        }
        else
        {
            Thread.Sleep(5);
            return ServerGetState(clientId);
        }

        return payload;
    }

    public List<byte> ClientGetState(uint clientId)
    {
        List<byte> payload = new List<byte>();

        if (Monitor.TryEnter(sharedDataLock, 5))
        {
            var client = clients.FirstOrDefault(c => c.Id == clientId);
            if(client != null)
            {
                AddTransform(payload, client.hmd);
                AddTransform(payload, client.leftController);
                AddTransform(payload, client.rightController);
            }
            else
            {
                Debug.Log("That client does not exist!");
            }

            Monitor.Exit(sharedDataLock);
        }
        else
        {
            Thread.Sleep(5);
            return ClientGetState(clientId);
        }

        return payload;
    }

    public void ServerUpdateState(byte[] payload, int byteCount, uint clientId)
    {
        if (Monitor.TryEnter(sharedDataLock, 5))
        {
            ClientInfo client = clients.FirstOrDefault(x => x.Id == clientId);

            if(client != null)
            {
                client.Connected        = true;
                client.hmd              = ParseTransform(payload, 0 * Trans.Size);
                client.leftController   = ParseTransform(payload, 1 * Trans.Size);
                client.rightController  = ParseTransform(payload, 2 * Trans.Size);
                client.trackerHip       = ParseTransform(payload, 3 * Trans.Size);
                client.trackerLeftFoot  = ParseTransform(payload, 4 * Trans.Size);
                client.trackerRightFoot = ParseTransform(payload, 5 * Trans.Size);
            }

            Monitor.Exit(sharedDataLock);
        }
    }

    public void ClientUpdateState(byte[] payload, int byteCount)
    {
        int offset = 0;

        int packetSize  = BitConverter.ToInt32(payload, offset + 0);
        if (packetSize != byteCount)
            return;

        int clientCount = BitConverter.ToInt32(payload, offset + 4);
        int ballCount   = BitConverter.ToInt32(payload, offset + 8);

        offset += 12;

        // Parse Client Data
        List<ClientInfo> recvClients = new List<ClientInfo>();
        for (int i = 0; i < clientCount; i++)
        {
            uint id             = BitConverter.ToUInt32(payload, offset + 0);
            int transformCount  = BitConverter.ToInt32(payload, offset + 4);

            ClientInfo c = new ClientInfo(id);
            c.Connected = true;

            offset += 8;

            for (int j = 0; j < transformCount; j++)
            {
                float x = BitConverter.ToSingle(payload, offset + 0);
                float y = BitConverter.ToSingle(payload, offset + 4);
                float z = BitConverter.ToSingle(payload, offset + 8);

                float qx = BitConverter.ToSingle(payload, offset + 12);
                float qy = BitConverter.ToSingle(payload, offset + 16);
                float qz = BitConverter.ToSingle(payload, offset + 20);
                float qw = BitConverter.ToSingle(payload, offset + 24);

                c.transforms.Add(new Trans(new Vector3(x, y, z), new Quaternion(qx, qy, qz, qw)));

                offset += 28;
            }

            recvClients.Add(c);
        }

        // Parse Balls Data
        for (int i = 0; i < ballCount; i++)
        {
            uint id = BitConverter.ToUInt32(payload, offset + 0);
            float x = BitConverter.ToSingle(payload, offset + 4);
            float y = BitConverter.ToSingle(payload, offset + 8);
            float z = BitConverter.ToSingle(payload, offset + 12);

            if (Monitor.TryEnter(sharedDataLock, 5))
            {
                var found = balls.FirstOrDefault(b => b.id == id);
                if(found == null)
                {
                    balls.Add(new ServerObject((int)id, x, y, z));
                }
                else
                {
                    found.pos = new Vector3(x, y, z);
                    found.rot = Quaternion.identity;
                }
                Monitor.Exit(sharedDataLock);
            }

            offset += 16;
        }


        // Update Clients
        if (Monitor.TryEnter(sharedDataLock, 5))
        {
            foreach(var client in clients)
            {
                var found = recvClients.FirstOrDefault(c => c.Id == client.Id);
                if(found == null)
                {
                    client.Connected = false;
                }
            }

            foreach(var client in recvClients)
            {
                var found = clients.FirstOrDefault(c => c.Id == client.Id);
                if(found == null)
                {
                    clients.Add(client);
                }
                else
                {
                    found.transforms = client.transforms.ToList();
                }
            }

            Monitor.Exit(sharedDataLock);
        }
    }

    public void RemoveDisconnectedClients()
    {
        for (int i = clients.Count - 1; i >= 0; i--)
        {
            if (!clients[i].Connected)
                clients.Remove(clients[i]);
        }
    }

    public void Clear()
    {
        foreach(var client in clients)
        {
            client.Connected = false;
            if(client.socket != null)
                client.socket.Close();
        }

        Update();
    }

    private int GetClientsSize()
    {
        int size = 0;
        foreach(var client in clients)
        {
            size += client.transforms.Count * Trans.Size;
        }
        return size;
    }

    private void GetInstanceTransforms(Transform root, List<Trans> transforms)
    {
        if (root.tag == "Serialize")
        {
            transforms.Add(new Trans(root.position, root.rotation));
        }

        for (int i = 0; i < root.childCount; i++)
        {
            var child = root.transform.GetChild(i);
            GetInstanceTransforms(child, transforms);
        }
    }

    private void SetInstanceTransforms(Transform root, List<Trans> transforms, ref int index)
    {
        if (transforms.Count <= 0)
            return;

        if(root.tag == "Serialize")
        {
            root.position = transforms[index].pos;
            root.rotation = transforms[index].rot;
            index++;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            var child = root.transform.GetChild(i);
            SetInstanceTransforms(child, transforms, ref index);
        }
    }

    private Trans ParseTransform(byte[] buffer, int offset = 0)
    {
        float x = BitConverter.ToSingle(buffer, offset + 0);
        float y = BitConverter.ToSingle(buffer, offset + 4);
        float z = BitConverter.ToSingle(buffer, offset + 8);
        float qx = BitConverter.ToSingle(buffer, offset + 12);
        float qy = BitConverter.ToSingle(buffer, offset + 16);
        float qz = BitConverter.ToSingle(buffer, offset + 20);
        float qw = BitConverter.ToSingle(buffer, offset + 24);

        Trans t = new Trans(new Vector3(x, y, z), new Quaternion(qx, qy, qz, qw));
        return t;
    }

    public static void AddTransform(List<byte> buffer, Trans t)
    {
        buffer.AddRange(BitConverter.GetBytes(t.pos.x));
        buffer.AddRange(BitConverter.GetBytes(t.pos.y));
        buffer.AddRange(BitConverter.GetBytes(t.pos.z));

        buffer.AddRange(BitConverter.GetBytes(t.rot.x));
        buffer.AddRange(BitConverter.GetBytes(t.rot.y));
        buffer.AddRange(BitConverter.GetBytes(t.rot.z));
        buffer.AddRange(BitConverter.GetBytes(t.rot.w));
    }
}
