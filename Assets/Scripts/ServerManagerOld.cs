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
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class drives the server. It handles the logic for:
/// - Accept incomming client connections
/// - Handle packets received from clients
/// - Send the state of the World to clients
/// </summary>
public class ServerManagerOld : MonoBehaviour
{
    // TcpListener containing the underlying socket listening and accepting connections
    private TcpListener listener;
    private int packetRate = 60;

    // Each connected client is represented by a ClientData object
    // A locking mechanism is required in order to synchronize access from various threads!
    private System.Object clientsLock = new System.Object();
    private List<ClientData> clients = new List<ClientData>();

    // Transforms where clients are to be spawned in the clients
    public List<Transform> spawnTransforms = new List<Transform>();
    private List<Trans> spawnTrans = new List<Trans>();

    // List of objects that need replication, a part from the clients themselves (balls)
    public List<Transform> objectsToSend = new List<Transform>();

    // Class in charge of controlling the ball
    private BallController ballController;

    // Send packets to measure RTT from server to client
    public bool benchmarkEnabled = false;

    private Thread unityThread;

    private void Awake()
    {
        // Fix the target framerate for standalone platforms
        Application.targetFrameRate = 60;

        // Get spawn transforms so that we can access them from another thread
        spawnTrans = spawnTransforms.Select(x => new Trans(x.position, x.rotation, x.name)).ToList();

        // Start listening
        listener = new TcpListener(IPAddress.Any, Constants.PORT);
        listener.Start(/*MAX_PENDING_CONNECTIONS*/);

        // Start accepting connections asynchronously. When there is an incomming connection, AcceptCallback is executed
        listener.BeginAcceptSocket(new AsyncCallback(AcceptCallback), null);
        Debug.Log("Server listening on port " + Constants.PORT);

        // Fetch the ball controller from the scene
        ballController = GameObject.Find(Constants.Ball).GetComponent<BallController>();

        // Store the UNITY Thread
        unityThread = Thread.CurrentThread;
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
            Send(clients[0].socket, packet);
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
                        if (key.Contains(Constants.RightHand))
                        {
                            // @TODO: Create a prefab for this! Hardcoded collider for now.
                            obj = new GameObject(Constants.RightHand);
                            obj.transform.localScale = Constants.RightHandScale;
                            var collider = obj.AddComponent<BoxCollider>();
                            collider.center = Constants.ColliderCenter;
                            collider.size = Constants.ColliderSize;
                        }
                        else
                        {
                            obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            Destroy(obj.GetComponent<SphereCollider>());
                            obj.transform.localScale = Constants.SphereScale;
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
                if (clients.Count > 1)
                {
                    var others = clients.Where(x => x.socket.Handle != client.socket.Handle
                        && x.TransformCount > 0 && x.instance != null).ToList();
                    Packet packet = PacketBuilder.Build(Packet.PacketType.OtherClients, others);
                    Send(client.socket, packet);
                    //packet.Send(client.socket, new AsyncCallback(SendCallback));
                }

                // Send the objectsToSend if any
                if (objectsToSend.Count > 0)
                {
                    List<Trans> objects = objectsToSend.Select(x => new Trans(x.position, x.rotation, x.name)).ToList();
                    Packet packet = PacketBuilder.Build(Packet.PacketType.Objects, objects);
                    Send(client.socket, packet);
                    //packet.Send(client.socket, new AsyncCallback(SendCallback));
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
    // Triggered when there is an incoming connection 
    // This method is called asynchronously and does NOT run on the UNITY Main Thread!!
    private void AcceptCallback(IAsyncResult AR)
    {
        try
        {
            // Synchronize access to the list of clients
            lock (clientsLock)
            {
                // Accept the connection and set Socket attributes
                Socket socket = listener.EndAcceptSocket(AR);
                Debug.Log("Connection accepted " + socket.RemoteEndPoint);
                socket.NoDelay = true;      // Disable Nagle's algorithm (https://msdn.microsoft.com/en-us/library/system.net.sockets.socket.nodelay(v=vs.110).aspx)
                socket.DontFragment = true;
                socket.ReceiveBufferSize = ClientData.BUFF_SIZE;
                socket.SendBufferSize = ClientData.BUFF_SIZE;

                // Create a new ClientData object from the incomming connection and add it to the list of clients
                ClientData client = new ClientData(socket);
                clients.Add(client);

                // Send welcome message to the recently connected client
                Packet packet = PacketBuilder.Build(Packet.PacketType.Text, Constants.WelcomeMsg);
                Send(socket, packet);

                // Choose a spawn transform according to the number of currently connected clients
                Trans spawn = spawnTrans[(clients.Count - 1) % spawnTrans.Count];
                clients.Last().spawn = spawn;

                // Send the spawn transform to the client
                packet = PacketBuilder.Build(Packet.PacketType.Spawn, spawn);
                Send(socket, packet);

                // Begin receiving data from the client (Sensor Data, etc...), pass in the ClientData object
                socket.BeginReceive(client.recvBuffer, 0, client.recvBuffer.Length, SocketFlags.None,
                    new AsyncCallback(ReceiveCallback), clients[clients.Count - 1]);

                // Keep accepting other connections asynchronously
                listener.BeginAcceptSocket(new AsyncCallback(AcceptCallback), null);
            }
        }
        catch
        {
            Debug.Log("Socket closed");
        }
    }

    // Triggered when a packet is sent
    private void SendCallback(IAsyncResult AR)
    {
        Socket socket = (Socket)AR.AsyncState;
        try
        { 
            int bytes_sent = socket.EndSend(AR);
            Debug.Assert(bytes_sent > 0);
            //Debug.Log(bytes_sent + " bytes sent");

            // Sleep to enforce the desired packetRate and don't waste CPU cycles
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
            // Extract the ClientData object from the IAsyncResult and finish the asynchronous receive
            ClientData client = (ClientData)AR.AsyncState;
            int bytes_recv = client.socket.EndReceive(AR);

            // Handle the data received and keep receiving or disconnect the client if anything went wrong
            if (HandleClientPacket(client, client.recvBuffer, bytes_recv))
            {
                // Sleep to enforce the packetRate and keep receiving data asynchronously
                Thread.Sleep(1000 / packetRate);
                client.socket.BeginReceive(client.recvBuffer, 0, client.recvBuffer.Length, SocketFlags.None,
                    new AsyncCallback(ReceiveCallback), client);
            }
            else
            {
                // Close the client connection
                client.socket.Close();
            }
        }
    }
    #endregion

    // Process the received packets, returns true on success or false if anything went wrong
    private bool HandleClientPacket(ClientData client, byte[] data, int size)
    {
        lock (clientsLock)
        {
            if (size == 0)
            {
                // The client has disconnected
                return false;
            }

            // The data received can contain more than one packet! Keep reading packets until there is data left
            int dataIndex = 0;
            while (dataIndex < size)
            {
                // Parse a packet from the data buffer
                Packet packet = PacketBuilder.Parse(data, ref dataIndex);

                // Process the packet
                switch(packet.Type)
                {
                    case Packet.PacketType.Text:
                        // Extract text string from the packet
                        string text = ((PacketText)packet).Data;
                        if(text == Constants.ServeRequest)
                        {
                            // Trigger the ballController if the text was a serve request
                            Debug.Log("Serving Ball");
                            ballController.serve = true;
                        }
                        Debug.Log("[C(" + client.socket.RemoteEndPoint + ")->S]: " + text
                            + " (" + packet.Size + " of " + size + " bytes)");

                        // Relay the message to all the other clients
                        foreach (var c in clients)
                        {
                            if (c.socket.Handle == client.socket.Handle)
                                continue;

                            Send(c.socket, packet);
                        }
                        break;

                    case Packet.PacketType.Sensors:
                        // Extract the client transforms from the packet and update the client transforms appropriately
                        List<Trans> transforms = ((PacketSensors)packet).Data;
                        foreach(Trans t in transforms)
                            client.SetTransform(t.Id, t);
                        break;

                    case Packet.PacketType.Benchmark:
                        NetBenchmarks b = ((PacketBenchmark)packet).Data;
                        Debug.Log("RTT: " + (b.recvTimeStamp - b.sendTimeStamp) + " ms.");
                        break;

                    default:
                        // Unknown Packet Type
                        Debug.Assert(false);
                        Debug.LogError("Invalid PacketType" + " (" + packet.Size + " of " + size + " bytes)");
                        //return false;
                        break;
                }
            }

            return true;
        }
    }

    // Called when the Send Button is pressed
    public void OnSendBtn_Click()
    {
        // Get text from the input field and build a packet with it
        GameObject sendText = GameObject.Find(Constants.SendInputField);
        string text = sendText.GetComponent<InputField>().text;
        Packet packet = PacketBuilder.Build(Packet.PacketType.Text, text);

        // Send the packet to all the currently connected clients
        lock (clientsLock)
        {
            foreach (ClientData client in clients)
            {
                Send(client.socket, packet);
            }
        }
    }

    #region Utils
    // Utility function to send a packet after some delay
    // This function can only be called from the UNITY Main Thread
    public float delay = 0.0f; // delay in seconds
    private void Send(Socket socket, Packet packet)
    {
        // StartCoroutine only if we are in the Unity thread
        if (unityThread.Equals(Thread.CurrentThread))
        {
            StartCoroutine(ExecuteAfterTime(delay,
                () => { packet.Send(socket, new AsyncCallback(SendCallback)); }
            ));
        }
        // Execute in a threadpool otherwise
        else
        {
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep((int)(delay * 1000)); // Convert to ms
                packet.Send(socket, new AsyncCallback(SendCallback));
            });
        }
    }

    private IEnumerator ExecuteAfterTime(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);

        // Code to execute after the delay
        action();
    }

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
