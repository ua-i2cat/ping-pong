// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class ServerManagerPaddle : MonoBehaviour
{
    private TcpListener listener;
    public int port = 3333;
    private byte[] buffer = new byte[8192];

    public GameObject clientPrefab;
    public static WorldState world = new WorldState(Authority.Server);

    void Start ()
    {
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        listener.BeginAcceptSocket(new AsyncCallback(AcceptCallback), null);
        Debug.Log("Server started, accepting connections on port: " + port);

        world.playerPrefab = clientPrefab;
    }

    void Update ()
    {
        world.Update();
	}

    private void ShareServerState(ClientInfo client)
    {
        //List<byte> sendBuffer = new List<byte>();

        //int HEADER_SIZE = sizeof(int) + sizeof(int) + sizeof(int);  // total size of the packet, #clients, #balls

        //int CLIENTS_SIZE = 8 * sizeof(float) * world.clients.Count; // 8 floats per client
        //int BALLS_SIZE = (3 * sizeof(float) + sizeof(int)) * world.balls.Count;     // 3 floats + 1 int per ball

        //int PACKET_SIZE = HEADER_SIZE + CLIENTS_SIZE + BALLS_SIZE;
        //Debug.Log("Packet Size: " + PACKET_SIZE + " #Clients: " + world.clients.Count + " #Balls: " + world.balls.Count);

        // Packet Size
        //sendBuffer.AddRange(BitConverter.GetBytes(PACKET_SIZE));

        // #Clients
        //sendBuffer.AddRange(BitConverter.GetBytes(world.clients.Count));

        // #Balls
        //sendBuffer.AddRange(BitConverter.GetBytes(world.balls.Count));

        // Clients Data
        //sendBuffer.AddRange(BitConverter.GetBytes(client.id));      // Client id
        //sendBuffer.AddRange(BitConverter.GetBytes(client.pos.x));   // Client pos
        //sendBuffer.AddRange(BitConverter.GetBytes(client.pos.y));
        //sendBuffer.AddRange(BitConverter.GetBytes(client.pos.z));
        //sendBuffer.AddRange(BitConverter.GetBytes(client.rot.x));   // Client rot
        //sendBuffer.AddRange(BitConverter.GetBytes(client.rot.y));
        //sendBuffer.AddRange(BitConverter.GetBytes(client.rot.z));
        //sendBuffer.AddRange(BitConverter.GetBytes(client.rot.w));

        //foreach(var oponent in world.clients)
        {
        //    if (oponent.id == client.id)
        //        continue;

        //    sendBuffer.AddRange(BitConverter.GetBytes(oponent.id));
            //sendBuffer.AddRange(BitConverter.GetBytes(oponent.pos.x));
            //sendBuffer.AddRange(BitConverter.GetBytes(oponent.pos.y));
            //sendBuffer.AddRange(BitConverter.GetBytes(oponent.pos.z));
            //sendBuffer.AddRange(BitConverter.GetBytes(oponent.rot.x));
            //sendBuffer.AddRange(BitConverter.GetBytes(oponent.rot.y));
            //sendBuffer.AddRange(BitConverter.GetBytes(oponent.rot.z));
            //sendBuffer.AddRange(BitConverter.GetBytes(oponent.rot.w));
        }

        // Balls Data
        // ...
        //foreach(var ball in world.balls)
        //{   
        //    sendBuffer.AddRange(BitConverter.GetBytes(ball.id));
        //    sendBuffer.AddRange(BitConverter.GetBytes(ball.pos.x));
        //    sendBuffer.AddRange(BitConverter.GetBytes(ball.pos.y));
        //    sendBuffer.AddRange(BitConverter.GetBytes(ball.pos.z));
        //}

        //client.socket.BeginSend(sendBuffer.ToArray(), 0, sendBuffer.Count, SocketFlags.None, new AsyncCallback(SendCallback), client);
    }

    // Callbacks
    private void AcceptCallback(IAsyncResult AR)
    {
        ClientInfo client = new ClientInfo(listener.EndAcceptSocket(AR));

        //world.clients.Add(client);
        Debug.Log("Client accepted - Client id: " + client.Id);

        // Send server state to the client
        ShareServerState(client);

        // Start receiving from the recently connected client
        client.socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None,
            new AsyncCallback(ReceiveCallback), client);

        // Keep accepting other clients
        listener.BeginAcceptSocket(new AsyncCallback(AcceptCallback), null);
    }

    private void ReceiveCallback(IAsyncResult AR)
    {
        ClientInfo client = (ClientInfo)AR.AsyncState;
        int bytes_received = client.socket.EndReceive(AR);

        if(bytes_received == 0)
        {
            // Disconnect client
            client.Connected = false;

            Debug.Log("Client id: " + client.Id + " has disconnected");
            return;
        }

        Debug.Log("Received " + bytes_received + " bytes from Client id: " + client.Id);
        
        // Handle packet received
        //float x = BitConverter.ToSingle(buffer, 0);
        //float y = BitConverter.ToSingle(buffer, 4);
        //float z = BitConverter.ToSingle(buffer, 8);
        //float qx = BitConverter.ToSingle(buffer, 12);
        //float qy = BitConverter.ToSingle(buffer, 16);
        //float qz = BitConverter.ToSingle(buffer, 20);
        //float qw = BitConverter.ToSingle(buffer, 24);

        //client.pos = new Vector3(x, y, z);
        //client.rot = new Quaternion(qx, qy, qz, qw);

        // Keep receiving
        client.socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None,
            new AsyncCallback(ReceiveCallback), client);
    }

    private void SendCallback(IAsyncResult AR)
    {
        ClientInfo client = (ClientInfo)AR.AsyncState;
        int bytes_sent = client.socket.EndSend(AR);
        Debug.Log(bytes_sent + " bytes sent to Client id: " + client.Id);

        int packetsPerSecond = 60;
        Thread.Sleep(1000 / packetsPerSecond);

        ShareServerState(client);
    }
}
