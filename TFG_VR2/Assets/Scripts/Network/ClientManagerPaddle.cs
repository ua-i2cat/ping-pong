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

public class ClientManagerPaddle : MonoBehaviour
{
    private Socket socket;
    public string ip = "127.0.0.1";
    public int port = 3333;
    private byte[] buffer = new byte[8192];

    public GameObject clientPrefab;
    public GameObject ballPrefab;

    public Transform controllerRight;   

    private List<ClientInfo> clients = new List<ClientInfo>();
    private List<ServerObject> balls = new List<ServerObject>();

    public static WorldState world = new WorldState(Authority.Client);
    private bool connected = false;

    void Start ()
    {
        world.playerPrefab = clientPrefab;
        world.ballPrefab = ballPrefab;
    }

    void OnGUI()
    {
        ip = GUI.TextField(new Rect(10, 10, 100, 20), ip, 25);

        if(GUI.Button(new Rect(10, 35, 100, 20), "Connect"))
            Init();

        if (GUI.Button(new Rect(10, 60, 100, 20), "Disconnect"))
        {
            world.Clear();
            connected = false;
            socket.Close();
        }
    }

    void Init()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.BeginConnect(IPAddress.Parse(ip), port, new AsyncCallback(ConnectCallback), null);
        connected = true;
    }

    void Update ()
    {
        if (connected)
        {
            SendControllersInfo();

            world.Update();
        }
    }

    void OnApplicationQuit()
    {
        if(connected)
            socket.Close();
    }

    void SendControllersInfo()
    {
        List<byte> sendBuffer = new List<byte>();
        sendBuffer.AddRange(BitConverter.GetBytes(controllerRight.position.x));   // Right Controller pos
        sendBuffer.AddRange(BitConverter.GetBytes(controllerRight.position.y));
        sendBuffer.AddRange(BitConverter.GetBytes(controllerRight.position.z));
        sendBuffer.AddRange(BitConverter.GetBytes(controllerRight.rotation.x));   // Right Controller rot
        sendBuffer.AddRange(BitConverter.GetBytes(controllerRight.rotation.y));
        sendBuffer.AddRange(BitConverter.GetBytes(controllerRight.rotation.z));
        sendBuffer.AddRange(BitConverter.GetBytes(controllerRight.rotation.w));
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
        const uint BALL_SIZE = 3 * sizeof(float) + sizeof(int);
        uint packetSize = BitConverter.ToUInt32(buffer, 0);
        Debug.Assert(packetSize == bytes_received);
        uint clientCount = BitConverter.ToUInt32(buffer, 4);
        uint ballCount = BitConverter.ToUInt32(buffer, 8);
        Debug.Log("Packet size: " + packetSize + " #Clients: " + clientCount + " #Balls: " + ballCount);

        List<ClientInfo> clientsReceived = new List<ClientInfo>();

        int bufferIndex = 12;
        for (int i = 0; i < clientCount; i++)
        {
            //uint id = BitConverter.ToUInt32(buffer, bufferIndex + 0);
            //float x = BitConverter.ToSingle(buffer, bufferIndex + 4);
            //float y = BitConverter.ToSingle(buffer, bufferIndex + 8);
            //float z = BitConverter.ToSingle(buffer, bufferIndex + 12);
            //float qx = BitConverter.ToSingle(buffer, bufferIndex + 16);
            //float qy = BitConverter.ToSingle(buffer, bufferIndex + 20);
            //float qz = BitConverter.ToSingle(buffer, bufferIndex + 24);
            //float qw = BitConverter.ToSingle(buffer, bufferIndex + 28);
            bufferIndex += (int)CLIENT_SIZE;

            //ClientInfo client = new ClientInfo(id, x, y, z, qx, qy, qz, qw);
            //clientsReceived.Add(client);
        }

        for(int i = 0; i < ballCount; i++)
        {
            int id = BitConverter.ToInt32(buffer, bufferIndex + 0);
            float x = BitConverter.ToSingle(buffer, bufferIndex + 4);
            float y = BitConverter.ToSingle(buffer, bufferIndex + 8);
            float z = BitConverter.ToSingle(buffer, bufferIndex + 12);
            bufferIndex += (int)BALL_SIZE;

            ServerObject ball = new ServerObject(id, x, y, z);

            var found = balls.Where(b => b.id == ball.id).FirstOrDefault();
            if(found == null)
                balls.Add(ball);
            else
            {
                found.pos = ball.pos;
                found.rot = ball.rot;
            }
        }

        // Mark as disconnected the clients that were not received from the server
        foreach(var client in clients)
        {
            var found = clientsReceived.Where(c => c.Id == client.Id).FirstOrDefault();
            if (found == null)
            {
                client.Connected = false;
            }
        }

        // Add and update connected clients
        foreach (var client in clientsReceived)
        {
            var found = clients.Where(c => c.Id == client.Id).FirstOrDefault();
            if (found == null)
            {
                clients.Add(client);
            }
            else
            {
                //found.pos = client.pos;
                //found.rot = client.rot;
            }
        }

        //world.clients = clients;
        //world.balls = balls;

        // Keep receiving
        socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None,
            new AsyncCallback(ReceiveCallback), null);
    }
}
