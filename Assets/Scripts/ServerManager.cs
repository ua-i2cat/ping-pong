// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using SharpConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

class ServerManager : MonoBehaviour
{
    public Net.Protocol protocol;
    private Server server = null;

    private int port = Constants.PORT;

    public List<Transform> spawnTransforms = new List<Transform>();
    private List<Trans> spawnTrans = new List<Trans>();

    public List<Transform> objectsToSend = new List<Transform>();

    private BallController ballController;

    public bool benchmarkEnabled = false;

    private ConnectionManager connectionManager = new ConnectionManager(100);

    private void Start()
    {
        // Fix the target framerate for standalone platforms
        Application.targetFrameRate = 60;

        // Get spawn transforms so that we can access them from another thread
        spawnTrans = spawnTransforms.Select(x => new Trans(x.position, x.rotation, x.name)).ToList();

        // Fetch the ball controller from the scene
        ballController = GameObject.Find(Constants.Ball).GetComponent<BallController>();

        // Get port and protocol from config file
        try
        {
            Configuration serverConfig = Configuration.LoadFromFile("ServerConfig.cfg");
            port = serverConfig["Config"]["Port"].IntValue;
            string protoString = serverConfig["Config"]["Protocol"].StringValue.ToUpper();
            if (protoString.Equals("UDP")) protocol = Net.Protocol.Udp;
            else if (protoString.Equals("TCP")) protocol = Net.Protocol.Tcp;
            else throw new InvalidOperationException();
        }
        catch
        {
            Debug.LogWarning("Failed to load Configuration file!");
            Debug.LogWarning("Using the default values: [" + port + "] (TCP)");
            protocol = Net.Protocol.Tcp;
        }

        // Create the server
        server = Net.ServerFactory.Create(protocol);

        // Start the server
        server.OnRecv += OnMsgRecv;
        server.Start(Constants.PORT);
    }

    private void OnApplicationQuit()
    {
        server.Stop();
        server.OnRecv -= OnMsgRecv;
    }

    private void Update()
    {
        HandleInput();
        connectionManager.Tick();

        var clients = connectionManager.Connections;
        foreach(var client in clients)
        {
            client.clientData.Update();

            if(clients.Count > 1)
            {
                var others = clients.Where(x => !x.endPoint.Equals(client.endPoint) && x.clientData.TransformCount > 0).ToList();
                //Packet packet = PacketBuilder.Build(Packet.PacketType.OtherClients, others);
                // WORKAROUND, build the packet manually
                List<byte> content = new List<byte>();
                int size = Constants.HEADER_SIZE;
                size += 1; // Number of clients (0-255)
                foreach (var c in others)
                {
                    size += sizeof(int) + sizeof(byte) + c.clientData.TransformCount * Trans.Size;
                }
                content.AddRange(BitConverter.GetBytes((short)size));   // 2 bytes for the size
                content.Add((byte)Packet.PacketType.OtherClients);      // 1 byte for the type
                content.Add((byte)others.Count);
                foreach (var c in others)
                {
                    content.AddRange(c.clientData.Serialize());
                }
                Packet packet = new Packet(content);
                server.Send(client.endPoint, packet.ToArray(), packet.Size);
            }

            // Send the objectsToSend if any
            if (objectsToSend.Count > 0)
            {
                List<Trans> objects = objectsToSend.Select(x => new Trans(x.position, x.rotation, x.name)).ToList();
                Packet packet = PacketBuilder.Build(Packet.PacketType.Objects, objects);
                server.Send(client.endPoint, packet.ToArray(), packet.Size);
            }
        }
    }

    private void OnMsgRecv(object sender, Server.ServerMsgEventArgs e)
    {
        if(!connectionManager.Contains(e.Client))
        {
            // Do this only if it is a new connection
            Packet p = PacketBuilder.Build(Packet.PacketType.Text, Constants.WelcomeMsg);
            server.Send(e.Client, p.ToArray(), p.Size);
            //Debug.Log("[S->C]: Welcome");

            Trans spawn = spawnTrans[connectionManager.Count % spawnTrans.Count];
            p = PacketBuilder.Build(Packet.PacketType.Spawn, spawn);
            server.Send(e.Client, p.ToArray(), p.Size);
            //Debug.Log("[S->C]: Spawn");
        }
        Connection connection = connectionManager.AddOrUpdateConnection(e.Client);

        int dataIndex = 0;
        Packet packet = PacketBuilder.Parse(e.Buffer, ref dataIndex);
        switch(packet.Type)
        {
            case Packet.PacketType.Text:
                string text = ((PacketText)packet).Data;
                if(text == Constants.ServeRequest)
                {
                    Debug.Log("Serving Ball");
                    ballController.serve = true;
                }
                Debug.Log("[C(" + e.Client + ")->S]: " + text
                    + " (" + packet.Size + " of " + e.Len + " bytes)");
                foreach (var c in connectionManager.Connections)
                {
                    if (c.endPoint == e.Client)
                        continue;

                    server.Send(c.endPoint, packet.ToArray(), packet.Size);
                }
                break;

            case Packet.PacketType.Sensors:
                List<Trans> transforms = ((PacketSensors)packet).Data;
                connection.clientData.Update(transforms);
                break;

            case Packet.PacketType.Benchmark:
                NetBenchmarks b = ((PacketBenchmark)packet).Data;
                Debug.Log("RTT: " + (b.recvTimeStamp - b.sendTimeStamp) + " ms.");
                break;

            default:
                Debug.Assert(false);
                Debug.LogError("Invalid PacketType" + " (" + packet.Size + " of " + e.Len + " bytes)");
                break;
        }
    }


    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log(connectionManager.Count + " clients online");
        }

        // Benchmarks
        if (benchmarkEnabled && connectionManager.Count > 0)
        {
            Packet packet = PacketBuilder.Build(Packet.PacketType.Benchmark, new NetBenchmarks());
            server.Send(connectionManager.Connections.First().endPoint, packet.ToArray(), packet.Size);
        }
    }

    public void OnSendBtn_Click()
    {
        // Get text in the input field and build a packet with it
        GameObject sendText = GameObject.Find(Constants.SendInputField);
        string text = sendText.GetComponent<InputField>().text;
        Packet packet = PacketBuilder.Build(Packet.PacketType.Text, text);

        // Send the packet to all the connected clients
        foreach (var client in connectionManager.Connections)
        {
            server.Send(client.endPoint, packet.ToArray(), packet.Size);
        }
    }
}
