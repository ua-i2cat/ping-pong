// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class ServerTestTCP : ServerTCP
{
    public ServerTestTCP()
    {
        OnRecv += (object s, ServerMsgEventArgs e) =>
        {
            Debug.Log("OnRecv " + e.Len + " bytes received from " + e.Client);
        };

        OnSend += (object s, ServerMsgEventArgs e) =>
        {
            Debug.Log("OnSend " + e.Len + " bytes sent");
        };

        ClientConnect += (object s, ConnectEventArgs e) =>
        {
            lastConnection = e.Socket;
            Debug.Log("OnConnect " + e.Socket.RemoteEndPoint);
        };

        ClientDisconnect += (object s, ConnectEventArgs e) =>
        {
            Debug.Log("OnDisconnect");
        };
    }

    public Socket lastConnection;
}

public class ClientTestUDP : ClientUDP
{
    public ClientTestUDP()
    {
        OnRecv += (object s, ClientMsgEventArgs e) =>
        {
            Debug.Log("OnSend " + e.Len + " bytes sent");
        };

        OnSend += (object s, ClientMsgEventArgs e) =>
        {
            Debug.Log("OnRecv " + e.Len + " bytes received from the server");
        };
    }
}

public class ClientTestTCP : ClientTCP
{
    public ClientTestTCP()
    {
        OnRecv += (object s, ClientMsgEventArgs e) =>
        {
            string str = "";
            for (int i = 0; i < e.Len; i++)
            {
                str += e.Buffer[i].ToString("X2") + ' ';
                if ((i + 1) % 16 == 0)
                    str += '\n';
            }
            Debug.Log(str);
        };
    }
}

public class ServerTestUDP : ServerUDP
{
    private List<EndPoint> clients = new List<EndPoint>();

    public ServerTestUDP()
    {
        OnRecv += (object s, ServerMsgEventArgs e) =>
        {
            HandleClientPacket(e.Buffer, e.Len);

            if (!clients.Contains(e.Client))
                clients.Add(e.Client);
        };
    }

    private void HandleClientPacket(byte[] buffer, int len)
    {
        int dataIndex = 0;
        Packet packet = PacketBuilder.Parse(buffer, ref dataIndex);
        switch(packet.Type)
        {
            case Packet.PacketType.Text:
                string text = ((PacketText)packet).Data;
                Debug.Log("[C->S]: " + text);
                break;
            case Packet.PacketType.Sensors:
                List<Trans> transforms = ((PacketSensors)packet).Data;
                break;
            case Packet.PacketType.Benchmark:
                NetBenchmarks b = ((PacketBenchmark)packet).Data;
                Debug.Log("RTT: " + (b.recvTimeStamp - b.sendTimeStamp) + " ms.");
                break;
            default:
                Debug.Assert(false);
                Debug.LogError("Invalid PacketType" + " (" + packet.Size + " of " + len + " bytes)");
                break;
        }
    }

    public void Update()
    {
        HandleInput();

        // Share Transforms between clients

        // Share objectsToSend if any
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("Sending packet");
            Packet p = PacketBuilder.Build(Packet.PacketType.Text, "Hello");
            Send(clients[0], p.ToArray(), p.Size);
        }
    }
}

public class Proxy : ServerUDP
{
    private ClientUDP p = new ClientUDP();

    public Proxy()
    {
        OnRecv += (object s, ServerMsgEventArgs e) =>
        {
            Debug.Log("Proxy received " + e.Len + " bytes");
            p.Send(e.Buffer, e.Len);
        };

        p.Start("127.0.0.1", 33334);
    }

    public override void Stop()
    {
        base.Stop();
        p.Stop();
    }
}

public class Test : MonoBehaviour
{
    private Server server;

    private int nClients = 10;
    private List<Client> clients = new List<Client>();

    void Start ()
    {
        Testing1();
        //Testing2();
    }

    void Update ()
    {
        if(Input.GetKeyDown(KeyCode.K))
        {
            foreach(var client in clients)
            {
                string text = RandomString(16);
                client.Send(Encoding.ASCII.GetBytes(text), text.Length);
            }
        }
        
        if(Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("Server has received " + serverRecvMsgs + " messages");
        }
    }

    private void OnApplicationQuit()
    {
        foreach (var client in clients)
            client.Stop();
        server.Stop();
    }

    private int serverRecvMsgs = 0;
    private void Testing1()
    {
        // Create server, set handlers and start
        server = new ServerTCP();
        server.OnRecv += (object s, Server.ServerMsgEventArgs e) =>
        {
            try
            {
                if (e.Len > 0)
                {
                    Interlocked.Increment(ref serverRecvMsgs);
                    //Debug.Log("[Server] Received: " + Encoding.ASCII.GetString(e.Buffer, 0, e.Len));
                    Server serv = (Server)s;
                    serv.Send(e.Client, e.Buffer, e.Len);
                }
            }
            catch
            {
                Debug.Log("Error");
            }           
        };
        server.OnSend += (object s, Server.ServerMsgEventArgs e) =>
        {
            //Debug.Log("[Server] Message sent");
        };
        server.Start(12345);

        for (int i = 0; i < nClients; i++)
        {
            // Create client, set handlers and start
            Client client = new ClientTCP();
            client.OnRecv += (object s, Client.ClientMsgEventArgs e) =>
            {
                //Debug.Log("[Client] Received: " + e.Len + " bytes");// Encoding.ASCII.GetString(e.Buffer));

                string str = "";
                for (int j = 0; j < e.Len; j++)
                {
                    str += e.Buffer[j].ToString("X2") + ' ';
                    if ((j + 1) % 16 == 0)
                        str += '\n';
                }
                Debug.Log(str);
            };
            client.Start("127.0.0.1", 12345);
            //client.Start("45.58.13.77", 15779);

            clients.Add(client);
        }
    }

    private void Testing2()
    {
        ClientTestTCP client = new ClientTestTCP();
        client.Start("45.58.13.77", 15779);
    }

    private static System.Random random = new System.Random();
    public static string RandomString(int length)
    {
        const string chars = "abcdefABCDEF0123456789";
        return new string(Enumerable.Repeat(chars, length)
          .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
