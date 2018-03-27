// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class ServerManager : MonoBehaviour
{
    private TcpListener listener;
    private int packetRate = 60;

    private System.Object clientsLock = new System.Object();
    private List<ClientData> clients = new List<ClientData>();

    public List<Transform> spawnTransforms = new List<Transform>();
    private List<Trans> spawnTrans = new List<Trans>();

    public bool benchmarkEnabled = false;

    private void Awake()
    {
        // Fix the target framerate for standalone platforms
        Application.targetFrameRate = 60;

        // Get spawn transforms so that we can access them from another thread
        spawnTrans = spawnTransforms.Select(x => new Trans(x.position, x.rotation, x.name)).ToList();

        // Start listening
        listener = new TcpListener(IPAddress.Any, Constants.PORT);
        listener.Start(/*MAX_PENDING_CONNECTIONS*/);

        // Start accepting connections
        listener.BeginAcceptSocket(new AsyncCallback(AcceptCallback), null);
        Debug.Log("Server listening on port " + Constants.PORT);
    }

    private void Update()
    {
        HandleInput();
        RemoveDisconnectedClients();
        ProcessClients();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log(clients.Count + " clients online");
            foreach (var client in clients)
            {
                Debug.Log(client.socket.RemoteEndPoint.ToString());
                Debug.Log(client.TransformCount + " transforms");
                foreach (var key in client.TransformKeys)
                {
                    Debug.Log("Key: " + key + " Id: " + client.GetTransform(key).Id);
                }
            }
        }

        // Benchmarks
        if(benchmarkEnabled && clients.Count > 0)
        {
            Packet packet = PacketBuilder.Build(Packet.PacketType.Benchmark, new NetBenchmarks());
            packet.Send(clients[0].socket, new AsyncCallback(SendCallback));
        }
    }

    private void OnApplicationQuit()
    {
        Debug.Log("Stopping the server...");

        // Close active connections
        lock (clientsLock)
        {
            foreach (ClientData client in clients)
                client.socket.Close();
        }

        // Stop listening for client connections
        listener.Stop();
    }

    private void ProcessClients()
    {
        lock (clientsLock)
        {
            foreach (var client in clients)
            {
                // Instantiate if a client is connected and doesn't have an instance yet
                if (client.instance == null && client.TransformCount > 0)
                {
                    client.instance = new GameObject(client.socket.RemoteEndPoint.ToString());
                    foreach (var key in client.TransformKeys)
                    {
                        GameObject obj;
                        if (key.Contains(/*Constants.RightHand*/"TODO"))
                        {
                            obj = Instantiate(Resources.Load("RH")) as GameObject;
                        }
                        else
                        {
                            obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            obj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                        }
                        obj.name = key;
                        obj.transform.parent = client.instance.transform;
                        Trans t = client.GetTransform(key);
                        obj.transform.position = t.Pos;
                        obj.transform.rotation = t.Rot;
                    }
                }
                // Update already instanced clients
                else if (client.instance != null && client.TransformCount > 0)
                {
                    foreach (var key in client.TransformKeys)
                    {
                        Trans t = client.GetTransform(key);
                        Transform current = client.instance.transform.Find(key);
                        if (current != null)
                        {
                            current.position = t.Pos;
                            current.rotation = t.Rot;
                        }
                    }
                }

                // Share transforms between Clients
                if (clients.Count >= 1)
                {
                    var others = clients.Where(x => x.socket.Handle != client.socket.Handle
                        && x.TransformCount > 0 && x.instance != null).ToList();
                    Packet packet = PacketBuilder.Build(Packet.PacketType.OtherClients, others);
                    packet.Send(client.socket, new AsyncCallback(SendCallback));
                }
            }
        }
    }

    // Check for disconnected clients and remove them
    private void RemoveDisconnectedClients()
    {
        lock (clientsLock)
        {
            for (int i = clients.Count - 1; i >= 0; i--)
            {
                if (!clients[i].socket.Connected)
                {
                    if (clients[i].instance != null)
                    {
                        Debug.Log("Client has disconnected " + clients[i].instance.name);
                        Destroy(clients[i].instance);
                    }
                    clients.Remove(clients[i]);
                }
            }
        }
    }

    #region Callbacks
    // Triggered when accepting an incoming connection
    private void AcceptCallback(IAsyncResult AR)
    {
        try
        {
            lock (clientsLock)
            {
                // Accept the connection and set Socket attributes
                Socket socket = listener.EndAcceptSocket(AR);
                Debug.Log("Connection accepted " + socket.RemoteEndPoint);
                socket.NoDelay = true;
                socket.DontFragment = true;
                socket.ReceiveBufferSize = ClientData.BUFF_SIZE;
                socket.SendBufferSize = ClientData.BUFF_SIZE;
                ClientData client = new ClientData(socket);
                clients.Add(client);

                // Begin sending data
                Packet packet = PacketBuilder.Build(Packet.PacketType.Text, "Welcome\n");
                packet.Send(socket, new AsyncCallback(SendCallback));

                Trans spawn = spawnTrans[(clients.Count - 1) % spawnTrans.Count];
                clients.Last().spawn = spawn;

                packet = PacketBuilder.Build(Packet.PacketType.Spawn, spawn);
                packet.Send(socket, new AsyncCallback(SendCallback));

                // Begin receiving data
                socket.BeginReceive(client.recvBuffer, 0, client.recvBuffer.Length, SocketFlags.None,
                    new AsyncCallback(ReceiveCallback), clients[clients.Count - 1]);

                // Keep accepting connections
                listener.BeginAcceptSocket(new AsyncCallback(AcceptCallback), null);
            }
        }
        catch
        {
            Debug.Log("Socket closed");
        }
    }

    // Triggered when sending a packet
    private void SendCallback(IAsyncResult AR)
    {
        Socket socket = (Socket)AR.AsyncState;
        try
        { 
            int bytes_sent = socket.EndSend(AR);
            //Debug.Log(bytes_sent + " bytes sent");

            Thread.Sleep(1000 / packetRate);
        }
        catch
        {
            Debug.Log("Client has disconnected " + socket.RemoteEndPoint);
        }
    }

    // Triggered when a packet is received
    private void ReceiveCallback(IAsyncResult AR)
    {
        lock (clientsLock)
        {
            ClientData client = (ClientData)AR.AsyncState;
            int bytes_recv = client.socket.EndReceive(AR);
            if (HandleClientPacket(client, client.recvBuffer, bytes_recv))
            {
                // Keep receiving data
                Thread.Sleep(1000 / packetRate);
                client.socket.BeginReceive(client.recvBuffer, 0, client.recvBuffer.Length, SocketFlags.None,
                    new AsyncCallback(ReceiveCallback), client);
            }
            else
            {
                // Close the connection of the client
                client.socket.Close();
            }
        }
    }
    #endregion

    // Discard invalid packets, and process the valid ones
    private bool HandleClientPacket(ClientData client, byte[] data, int size)
    {
        lock (clientsLock)
        {
            if (size == 0)
            {
                // The client has disconnected
                return false;
            }

            // The data received can contain more than one packet!
            int dataIndex = 0;
            while (dataIndex < size)
            {
                // Parse a packet from the data buffer
                Packet packet = PacketBuilder.Parse(data, ref dataIndex);

                // Process the packet
                switch(packet.Type)
                {
                    case Packet.PacketType.Text:
                        string text = ((PacketText)packet).Data;
                        Debug.Log("[C(" + client.socket.RemoteEndPoint + ")->S]: " + text
                            + " (" + packet.Size + " of " + size + " bytes)");
                        foreach (var c in clients)
                        {
                            if (c.socket.Handle == client.socket.Handle)
                                continue;
                            packet.Send(c.socket, new AsyncCallback(SendCallback));
                        }
                        break;

                    case Packet.PacketType.Sensors:
                        List<Trans> transforms = ((PacketSensors)packet).Data;
                        foreach(Trans t in transforms)
                            client.SetTransform(t.Id, t);
                        break;

                    case Packet.PacketType.Benchmark:
                        NetBenchmarks b = ((PacketBenchmark)packet).Data;
                        Debug.Log("RTT: " + (b.recvTimeStamp - b.sendTimeStamp) + " ms.");
                        break;

                    default:
                        Debug.Assert(false);
                        Debug.LogError("Invalid PacketType" + " (" + packet.Size + " of " + size + " bytes)");
                        break;
                }
            }

            return true;
        }
    }

    public void OnSendBtn_Click()
    {
        // Get text in the input field and build a packet with it
        GameObject sendText = GameObject.Find("SendInputField");
        string text = sendText.GetComponent<InputField>().text;
        Packet packet = PacketBuilder.Build(Packet.PacketType.Text, text);

        // Send the packet to all the connected clients
        lock (clientsLock)
        {
            foreach (ClientData client in clients)
            {
                packet.Send(client.socket, new AsyncCallback(SendCallback));
            }
        }
    }

    #region Utils
    private void DisplaySocketInfo(Socket socket)
    {
        Debug.Log("AddressFamily: " + socket.AddressFamily);
        Debug.Log("Available: " + socket.Available);
        Debug.Log("Blocking: " + socket.Blocking);
        Debug.Log("Connected: " + socket.Connected);
        Debug.Log("DontFragment: " + socket.DontFragment);
        //Debug.Log("EnableBroadcast: " + socket.EnableBroadcast);
        Debug.Log("ExclusiveAddressUse: " + socket.ExclusiveAddressUse);
        Debug.Log("Handle: " + socket.Handle);
        Debug.Log("IsBound: " + socket.IsBound);
        Debug.Log("LingerState: " + socket.LingerState);
        Debug.Log("LocalEndPoint: " + socket.LocalEndPoint);
        //Debug.Log("MulticastLoopback: " + socket.MulticastLoopback);
        Debug.Log("NoDelay: " + socket.NoDelay);
        Debug.Log("ProtocolType: " + socket.ProtocolType);
        Debug.Log("ReceiveBufferSize: " + socket.ReceiveBufferSize);
        Debug.Log("ReceiveTimeout: " + socket.ReceiveTimeout);
        Debug.Log("RemoteEndPoint: " + socket.RemoteEndPoint);
        Debug.Log("SendBufferSize: " + socket.SendBufferSize);
        Debug.Log("SendTimeout: " + socket.SendTimeout);
        Debug.Log("SocketType: " + socket.SocketType);
        Debug.Log("Ttl: " + socket.Ttl);
        Debug.Log("UseOnlyOverlappedIO: " + socket.UseOnlyOverlappedIO);
    }
    #endregion
}
