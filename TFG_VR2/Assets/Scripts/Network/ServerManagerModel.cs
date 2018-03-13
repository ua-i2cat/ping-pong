// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class ServerManagerModel : MonoBehaviour
{
    private TcpListener listener;
    public int port = 3333;
    private byte[] recvBuffer = new byte[8192];
    private int packetsPerSecond = 30;

    public GameObject playerPrefab;
    public WorldState world = new WorldState(Authority.Server);

    public GameObject ballObj;

    public Text playersOnlineText;

    void Start()
    {
        //UnityEngine.XR.XRSettings.LoadDeviceByName("");
        //UnityEngine.XR.XRSettings.enabled = false;

        Application.targetFrameRate = 60;

        world.playerPrefab = playerPrefab;

        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        listener.BeginAcceptSocket(new AsyncCallback(AcceptCallback), null);
        
        Debug.Log("Server started, accepting connections on port: " + port); 
    }

    void Update()
    {
        world.Update();
        playersOnlineText.color = world.clients.Count > 0 ? Color.green : Color.red;
        playersOnlineText.text = "Online Players: " + world.clients.Count;

        if(world.clients.Count == 1)
        {
            transform.Find("Head").position = world.clients[0].hmd.pos;
            transform.Find("Head").rotation = world.clients[0].hmd.rot;

            transform.Find("LHand").position = world.clients[0].leftController.pos;
            transform.Find("LHand").rotation = world.clients[0].leftController.rot;

            transform.Find("RHand").position = world.clients[0].rightController.pos;
            transform.Find("RHand").rotation = world.clients[0].rightController.rot;

            transform.Find("Hip").position = world.clients[0].trackerHip.pos;
            transform.Find("Hip").rotation = world.clients[0].trackerHip.rot;

            transform.Find("LFoot").position = world.clients[0].trackerLeftFoot.pos;
            transform.Find("LFoot").rotation = world.clients[0].trackerLeftFoot.rot;

            transform.Find("RFoot").position = world.clients[0].trackerRightFoot.pos;
            transform.Find("RFoot").rotation = world.clients[0].trackerRightFoot.rot;
        }

        if(Input.GetKeyDown(KeyCode.D))
        {
            foreach (var c in world.clients)
            {
                Debug.Log("ClientID: " + c.Id + " Connected: " + c.socket.Connected +
                    "\n[" + c.socket.LocalEndPoint + " - " + c.socket.RemoteEndPoint + "]");
            }
        }
    }

    void OnApplicationQuit()
    {
        Debug.Log("Closing Server");
        world.Clear();
        listener.Stop();
    }

    // Callbacks
    private void AcceptCallback(IAsyncResult AR)
    {
        // Add a ball object to the world before the first client is added
        //if (ballObj != null && world.clients.Count == 0)
        //{
        //    world.AddBall(new ServerObject(ballObj));
        //}

        // Add new client to the world
        ClientInfo client = new ClientInfo(listener.EndAcceptSocket(AR));
        client.socket.NoDelay = true;

        //Debug.Log("Client accepted - Client id: " + client.id);
        world.AddClient(client);

        // Send world state to the client
        List<byte> sendData = world.ServerGetState(client.Id);
        //world.ClientUpdateState(sendBuffer, sendBuffer.Length);
        client.socket.BeginSend(sendData.ToArray(), 0, sendData.Count, SocketFlags.None, 
            new AsyncCallback(SendCallback), client);

        // Start receiving from the recently connected client
        client.socket.BeginReceive(recvBuffer, 0, recvBuffer.Length, SocketFlags.None,
            new AsyncCallback(ReceiveCallback), client);

        // Keep accepting other clients
        listener.BeginAcceptSocket(new AsyncCallback(AcceptCallback), null);
    }

    private void ReceiveCallback(IAsyncResult AR)
    {
        ClientInfo client = (ClientInfo)AR.AsyncState;
        int bytes_received = client.socket.EndReceive(AR);

        if (bytes_received == 0)
        {
            // Disconnect client
            client.Connected = false;
            Debug.Log("Client id: " + client.Id + " has disconnected");
            return;
        }

        // The Server should receive 6 Transforms from the client (head + hip + 2hand + 2feet)
        //Debug.Log("Received " + bytes_received + " bytes from Client id: " + client.id);
        if (bytes_received != 6 * Trans.Size)
        {
            Thread.Sleep(1000 / packetsPerSecond);
            client.socket.BeginReceive(recvBuffer, 0, recvBuffer.Length, SocketFlags.None,
                new AsyncCallback(ReceiveCallback), client);
            return;
        }

        // Handle received packet
        world.ServerUpdateState(recvBuffer, bytes_received, client.Id);

        // Keep receiving
        client.socket.BeginReceive(recvBuffer, 0, recvBuffer.Length, SocketFlags.None,
            new AsyncCallback(ReceiveCallback), client);
    }

    private void SendCallback(IAsyncResult AR)
    {
        ClientInfo client = (ClientInfo)AR.AsyncState;
        int bytes_sent = client.socket.EndSend(AR);
        //Debug.Log(bytes_sent + " bytes sent to Client id: " + client.id);

        Thread.Sleep(1000 / packetsPerSecond);

        // Send world state to the client
        List<byte> sendData = world.ServerGetState(client.Id);

        client.socket.BeginSend(sendData.ToArray(), 0, sendData.Count, SocketFlags.None,
            new AsyncCallback(SendCallback), client);
    }
}
