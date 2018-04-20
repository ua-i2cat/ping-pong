// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class ServerManagerUDP : MonoBehaviour
{
    private Socket socket;
    private int packetRate = 60;

    private System.Object clientsLock = new System.Object();
    private List<ClientDataUDP> clients = new List<ClientDataUDP>();

    public List<Transform> spawnTransforms = new List<Transform>();
    private List<Trans> spawnTrans = new List<Trans>();

    public List<Transform> objectsToSend = new List<Transform>();

    private BallController ballController;

    public bool benchmarkEnabled = false;

    private void Start()
    {
        // Fix the target framerate for standalone platforms
        Application.targetFrameRate = 60;

        // Get spawn transforms so that we can access them from another thread
        spawnTrans = spawnTransforms.Select(x => new Trans(x.position, x.rotation, x.name)).ToList();

        // Start listening
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Blocking = false;
        socket.Bind(new IPEndPoint(IPAddress.Any, Constants.PORT));

        // Fetch the ball controller from the scene
        ballController = GameObject.Find(Constants.Ball).GetComponent<BallController>();
    }

    private void Update()
    {
        HandleInput();
        RemoveDisconnectedClients();

        byte[] data = new byte[socket.Available];
        while(socket.Available > 0)
        {
            try
            {
                EndPoint ep = new IPEndPoint(IPAddress.Any, 0);
                int bytes_received = socket.ReceiveFrom(data, ref ep);
                ClientDataUDP client = new ClientDataUDP((IPEndPoint)ep);
                HandleClientPacket(client, data, bytes_received);
            }
            catch
            {

            }
        }

        foreach (var client in clients)
        {
            // Share transforms between Clients
            if (clients.Count > 1)
            {
                var others = clients.Where(x => !x.endPoint.Equals(client.endPoint)).ToList();
                //Packet packet = PacketBuilder.Build(Packet.PacketType.OtherClients, others);
                // WORKAROUND, build the packet manually
                List<byte> content = new List<byte>();
                int size = Constants.HEADER_SIZE;
                size += 1; // Number of clients (0-255)
                foreach(var c in others)
                {
                    size += sizeof(int) + sizeof(byte) + c.TransformCount * Trans.Size;
                }
                content.AddRange(BitConverter.GetBytes((short)size));   // 2 bytes for the size
                content.Add((byte)Packet.PacketType.OtherClients);      // 1 byte for the type
                content.Add((byte)others.Count);
                foreach(var c in others)
                {
                    content.AddRange(c.Serialize());
                }
                Packet packet = new Packet(content);

                socket.SendTo(packet.ToArray(), client.endPoint);
            }

            // Send the objectsToSend if any
            if (objectsToSend.Count > 0)
            {
                List<Trans> objects = objectsToSend.Select(x => new Trans(x.position, x.rotation, x.name)).ToList();
                Packet packet = PacketBuilder.Build(Packet.PacketType.Objects, objects);
                socket.SendTo(packet.ToArray(), client.endPoint);
            }
        }
    }

    private void HandleClientPacket(ClientDataUDP client, byte[] data, int size)
    {
        var existingClient = clients.Where(x => x.endPoint.Equals(client.endPoint)).FirstOrDefault();
        if (existingClient == null)
        {
            clients.Add(client);

            Packet p = PacketBuilder.Build(Packet.PacketType.Text, Constants.WelcomeMsg);
            socket.SendTo(p.ToArray(), client.endPoint);

            Trans spawn = spawnTrans[(clients.Count - 1) % spawnTrans.Count];
            clients.Last().spawn = spawn;
            p = PacketBuilder.Build(Packet.PacketType.Spawn, spawn);
            socket.SendTo(p.ToArray(), client.endPoint);

            existingClient = client;
        }
        existingClient.TTL = 100;

        int dataIndex = 0;
        Packet packet = PacketBuilder.Parse(data, ref dataIndex);
        switch(packet.Type)
        {
            case Packet.PacketType.Text:
                string text = ((PacketText)packet).Data;
                if (text == Constants.ServeRequest)
                {
                    Debug.Log("Serving Ball");
                    ballController.serve = true;
                }
                Debug.Log("[C(" + existingClient.endPoint + ")->S]: " + text
                    + " (" + packet.Size + " of " + size + " bytes)");
                foreach (var c in clients)
                {
                    if (c.endPoint == existingClient.endPoint)
                        continue;

                    socket.SendTo(packet.ToArray(), c.endPoint);
                }
                break;

            case Packet.PacketType.Sensors:
                List<Trans> transforms = ((PacketSensors)packet).Data;
                existingClient.Update(transforms);
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

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log(clients.Count + " clients online");
            //foreach (var client in clients)
            //{
            //    Debug.Log(client.socket.RemoteEndPoint.ToString());
            //    Debug.Log(client.TransformCount + " transforms");
            //    foreach (var key in client.TransformKeys)
            //    {
            //        Debug.Log("Key: " + key + " Id: " + client.GetTransform(key).Id);
            //    }
            //}
        }

        // Benchmarks
        if (benchmarkEnabled && clients.Count > 0)
        {
            Packet packet = PacketBuilder.Build(Packet.PacketType.Benchmark, new NetBenchmarks());
            socket.SendTo(packet.ToArray(), clients.First().endPoint);
        }
    }

    private void RemoveDisconnectedClients()
    {
        for (int i = clients.Count - 1; i >= 0; i--)
        {
            --clients[i].TTL;
            if (clients[i].TTL < 0)
            {
                clients[i].Destroy();
                clients.Remove(clients[i]);
            }
        }
    }

    public void OnSendBtn_Click()
    {
        // Get text in the input field and build a packet with it
        GameObject sendText = GameObject.Find(Constants.SendInputField);
        string text = sendText.GetComponent<InputField>().text;
        Packet packet = PacketBuilder.Build(Packet.PacketType.Text, text);

        // Send the packet to all the connected clients
        lock (clientsLock)
        {
            foreach (var client in clients)
            {
                socket.SendTo(packet.ToArray(), client.endPoint);
            }
        }
    }
}
