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

public class ClientManager : MonoBehaviour
{
    private Socket socket;
    public string ip = "127.0.0.1";
    public int port = 3333;

    public GameObject clientPrefab;

    public Transform controllerRight;

    public class ClientInfo
    {
        public UInt32 id;
        public Vector3 pos;
        public Quaternion rot;
        public GameObject instance;
        public bool connected = true;
    }

    private List<ClientInfo> clients = new List<ClientInfo>();

    private byte[] buffer = new byte[8192];

    void Start ()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);        
        socket.BeginConnect(IPAddress.Parse(ip), port, new AsyncCallback(ConnectCallback), null);
    }
	
	void Update ()
    {
        SendControllersInfo();

        foreach (var client in clients)
        {
            if (client.connected)
            {
                if (client.instance == null)
                    client.instance = Instantiate(clientPrefab, client.pos, client.rot);

                client.instance.transform.position = client.pos;
                client.instance.transform.rotation = client.rot;
            }
            else
            {
                if (client.instance != null)
                    Destroy(client.instance);
            }
        }

        // Remove clients that are disconnected from the list of clients
        for (int i = clients.Count - 1; i >= 0; i--)
        {
            if (!clients[i].connected)
                clients.Remove(clients[i]);
        }
    }

    void OnApplicationQuit()
    {
        socket.Close();
    }

    void SendControllersInfo()
    {
        List<byte> sendBuffer = new List<byte>();
        sendBuffer.AddRange(BitConverter.GetBytes(controllerRight.position.x));   // Client pos
        sendBuffer.AddRange(BitConverter.GetBytes(controllerRight.position.y));
        sendBuffer.AddRange(BitConverter.GetBytes(controllerRight.position.z));
        socket.BeginSend(sendBuffer.ToArray(), 0, sendBuffer.Count, SocketFlags.None, null/*new AsyncCallback(SendCallback)*/, null /*socket*/);
    }

    // Callbacks
    private void ConnectCallback(IAsyncResult AR)
    {
        Debug.Log("Connected");
        socket.EndConnect(AR);

        // Start receiving
        socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, 
            new AsyncCallback(ReceiveCallback), null);      
    }

    private void SendCallback(IAsyncResult AR)
    {
        //Socket socket = (Socket)AR.AsyncState;
        //int bytes_sent = socket.EndSend(AR);
        int packetsPerSecond = 60;
        Thread.Sleep(1000 / packetsPerSecond);

        SendControllersInfo();
    }

    private void ReceiveCallback(IAsyncResult AR)
    {
        int bytes_received = socket.EndReceive(AR);
        Debug.Log("Received " + bytes_received + " bytes");

        if(bytes_received == 0)
        {
            return;
        }

        const uint CLIENT_SIZE = 8 * sizeof(float); // 8 floats
        uint packetSize = BitConverter.ToUInt32(buffer, 0);
        uint clientCount = packetSize / CLIENT_SIZE;
        Debug.Log("Packet size: " + packetSize + " #Clients: " + clientCount);

        List<ClientInfo> clientsReceived = new List<ClientInfo>();

        int bufferIndex = 4;
        for (int i = 0; i < clientCount; i++)
        {
            uint id = BitConverter.ToUInt32(buffer, bufferIndex + 0);
            float x = BitConverter.ToSingle(buffer, bufferIndex + 4);
            float y = BitConverter.ToSingle(buffer, bufferIndex + 8);
            float z = BitConverter.ToSingle(buffer, bufferIndex + 12);
            float qx = BitConverter.ToSingle(buffer, bufferIndex + 16);
            float qy = BitConverter.ToSingle(buffer, bufferIndex + 20);
            float qz = BitConverter.ToSingle(buffer, bufferIndex + 24);
            float qw = BitConverter.ToSingle(buffer, bufferIndex + 28);
            bufferIndex += (int)CLIENT_SIZE;

            ClientInfo client = new ClientInfo();
            client.id = id;
            client.pos = new Vector3(x, y, z);
            client.rot = new Quaternion(qx, qy, qz, qw);
            clientsReceived.Add(client);
        }

        // Mark as disconnected the clients that were not received from the server
        foreach(var client in clients)
        {
            var found = clientsReceived.Where(c => c.id == client.id).FirstOrDefault();
            if (found == null)
            {
                client.connected = false;
            }
        }

        // Add and update connected clients
        foreach (var client in clientsReceived)
        {
            var found = clients.Where(c => c.id == client.id).FirstOrDefault();
            if (found == null)
            {
                clients.Add(client);
            }
            else
            {
                found.pos = client.pos;
                found.rot = client.rot;
            }
        }

        // Keep receiving
        socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None,
            new AsyncCallback(ReceiveCallback), null);
    }
}
