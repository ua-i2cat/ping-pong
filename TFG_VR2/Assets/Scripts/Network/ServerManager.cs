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

public class ServerManager : MonoBehaviour
{
    private TcpListener listener;
    public int port = 3333;

    private List<ClientInfo> clients = new List<ClientInfo>();
    private byte[] buffer = new byte[8192];

    public GameObject clientPrefab;

    void Start ()
    {
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        listener.BeginAcceptSocket(new AsyncCallback(AcceptCallback), null);
        Debug.Log("Server started, accepting connections on port: " + port);
    }
	
	void Update ()
    {
		foreach(var client in clients)
        {
            if (client.Connected)
            {
                if(client.instance == null)
                    client.instance = Instantiate(clientPrefab, new Vector3(0, 1, 0), Quaternion.identity);

                // TODO: Physics and IK
                //client.instance.transform.position = client.pos;
                //client.instance.transform.rotation = client.rot;
                //client.pos = client.instance.transform.position;
                //client.rot = client.instance.transform.rotation;
            }
            else
            {
                if (client.instance != null)
                    Destroy(client.instance);
            }
        }

        // Remove clients that are disconnected from the list of clients
        for(int i = clients.Count - 1; i >= 0; i--)
        {
            if (!clients[i].Connected)
                clients.Remove(clients[i]);
        }
	}

    private void ShareServerState(ClientInfo client)
    {
        List<byte> sendBuffer = new List<byte>();

        const uint CLIENT_SIZE = 8 * sizeof(float); // 8 floats
        uint packetSize = CLIENT_SIZE * (uint)clients.Count + sizeof(uint);
        Debug.Log("Packet Size: " + packetSize);

        // TODO: Build packet
        sendBuffer.AddRange(BitConverter.GetBytes(packetSize));
        sendBuffer.AddRange(BitConverter.GetBytes(client.Id));      // Client id
        //sendBuffer.AddRange(BitConverter.GetBytes(client.pos.x));   // Client pos
        //sendBuffer.AddRange(BitConverter.GetBytes(client.pos.y));
        //sendBuffer.AddRange(BitConverter.GetBytes(client.pos.z));
        //sendBuffer.AddRange(BitConverter.GetBytes(client.rot.x));   // Client rot
        //sendBuffer.AddRange(BitConverter.GetBytes(client.rot.y));
        //sendBuffer.AddRange(BitConverter.GetBytes(client.rot.z));
        //sendBuffer.AddRange(BitConverter.GetBytes(client.rot.w));

        foreach(var oponent in clients)
        {
            if (oponent.Id == client.Id)
                continue;

            sendBuffer.AddRange(BitConverter.GetBytes(oponent.Id));
            //sendBuffer.AddRange(BitConverter.GetBytes(oponent.pos.x));
            //sendBuffer.AddRange(BitConverter.GetBytes(oponent.pos.y));
            //sendBuffer.AddRange(BitConverter.GetBytes(oponent.pos.z));
            //sendBuffer.AddRange(BitConverter.GetBytes(oponent.rot.x));
            //sendBuffer.AddRange(BitConverter.GetBytes(oponent.rot.y));
            //sendBuffer.AddRange(BitConverter.GetBytes(oponent.rot.z));
            //sendBuffer.AddRange(BitConverter.GetBytes(oponent.rot.w));
        }

        client.socket.BeginSend(sendBuffer.ToArray(), 0, sendBuffer.Count, SocketFlags.None, new AsyncCallback(SendCallback), client);
    }

    // Callbacks
    private void AcceptCallback(IAsyncResult AR)
    {
        ClientInfo client = new ClientInfo(listener.EndAcceptSocket(AR));

        clients.Add(client);
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
            // TODO: Locking??
            client.Connected = false;

            Debug.Log("Client id: " + client.Id + " has disconnected");
            return;
        }

        Debug.Log("Received " + bytes_received + " bytes from Client id: " + client.Id);
        // TODO: Handle packet received
        //float x = BitConverter.ToSingle(buffer, 0);
        //float y = BitConverter.ToSingle(buffer, 4);
        //float z = BitConverter.ToSingle(buffer, 8);
        //client.pos = new Vector3(x, y, z);

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
