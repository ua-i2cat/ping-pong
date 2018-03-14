// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class ServerManager : MonoBehaviour
{
    private TcpListener listener;
    private int port = 33333;

    //private const int BUFF_SIZE = 8192;
    //private byte[] recvBuffer = new byte[BUFF_SIZE];

    private int packetRate = 30;

    private System.Object clientsLock = new System.Object();
    private List<ClientData> clients = new List<ClientData>();

    public List<Transform> spawnTransforms = new List<Transform>();
    private List<Vector3> spawnPositions = new List<Vector3>();

    public bool send = true;

    private void Awake()
    {
        // Fix the target framerate for standalone platforms
        Application.targetFrameRate = 60;

        // Get spawn positions from transforms
        spawnPositions = spawnTransforms.Select(x => x.position).ToList();

        // Start listening
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start(/*MAX_PENDING_CONNECTIONS*/);

        // Start accepting connections
        listener.BeginAcceptSocket(new AsyncCallback(AcceptCallback), null);
        Debug.Log("Server listening on port " + port);
    }

    private void Update()
    {
        RemoveDisconnectedClients();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log(clients.Count);
            foreach (var client in clients)
            {
                Debug.Log(client.socket.RemoteEndPoint.ToString());
                Debug.Log(client.TransformCount);
                foreach (var key in client.TransformKeys)
                {
                    Debug.Log("Key: " + key + " Id: " + client.GetTransform(key).Id);
                }
            }
        }

        lock (clientsLock)
        {
            foreach (var client in clients)
            {
                if (client.instance == null && client.TransformCount > 0)
                {
                    client.instance = new GameObject(client.socket.RemoteEndPoint.ToString());
                    foreach (var key in client.TransformKeys)
                    {
                        GameObject obj;
                        if (key.Contains("RH"))
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
                if (clients.Count >= 1 && send)
                {
                    SendServerPacket(BuildServerPacket(Packet.PacketType.OtherClients, client), client.socket);
                }
            }
        }

        //send = false;
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
                SendServerPacket(BuildServerPacket(Packet.PacketType.Text, "Welcome\n"), socket);

                Vector3 pos = spawnPositions[(clients.Count - 1) % spawnPositions.Count];
                clients.Last().spawnPos = pos;
                SendServerPacket(BuildServerPacket(Packet.PacketType.Spawn, pos), socket);

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
        //int bytes_sent = socket.EndSend(AR);
        //Debug.Log(bytes_sent + " bytes sent");

        Thread.Sleep(1000 / packetRate);
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

    private List<byte> BuildServerPacket(Packet.PacketType type, object data)
    {
        List<byte> packet = new List<byte>();

        switch (type)
        {
            // Plain Text
            case Packet.PacketType.Text:
                string text = (string)data;
                packet.AddRange(BitConverter.GetBytes((short)(2 + 1 + text.Length)));
                packet.Add((byte)Packet.PacketType.Text);
                packet.AddRange(Encoding.ASCII.GetBytes(text));
                break;

            // Sensor Data (6)
            case Packet.PacketType.Sensors:
                packet.AddRange(BitConverter.GetBytes((short)(2 + 1)));
                //packet.AddRange(BitConverter.GetBytes((short)(2 + 6 * 7 * sizeof(float))));
                packet.Add((byte)Packet.PacketType.Sensors);
                break;

            case Packet.PacketType.Spawn:
                packet.AddRange(BitConverter.GetBytes((short)(2 + 1 + 3 * sizeof(float))));
                packet.Add((byte)Packet.PacketType.Spawn);
                Vector3 pos = (Vector3)data;
                packet.AddRange(BitConverter.GetBytes(pos.x));
                packet.AddRange(BitConverter.GetBytes(pos.y));
                packet.AddRange(BitConverter.GetBytes(pos.z));
                break;

            case Packet.PacketType.OtherClients:
                lock (clientsLock)
                {
                    ClientData client = (ClientData)data;
                    int packetSize = sizeof(short)  // 2 bytes: Length of the packet (in bytes)
                        + sizeof(byte)              // 1 byte:  Packet type
                        + sizeof(byte);             // 1 byte:  #Clients - 1 (up to 255)
                    var otherClients = clients.Where(x => x.socket.Handle != client.socket.Handle
                        && x.TransformCount > 0);
                    foreach (var c in otherClients)
                    {               // ClientID  +  #transforms
                        packetSize += sizeof(int) + sizeof(byte) + c.TransformCount * Trans.Size;
                    }
                    if (otherClients.Count() > 0)
                    {
                        //Debug.Assert(packetSize == 233, "PacketSize: " + packetSize +
                        //    " otherClients.Count: " + otherClients.Count() + " " + 
                        //    otherClients.First<ClientData>().TransformCount);
                    }
                    packet.AddRange(BitConverter.GetBytes((short)packetSize));
                    packet.Add((byte)Packet.PacketType.OtherClients);
                    packet.Add((byte)(clients.Count - 1));

                    foreach (var c in otherClients)
                    {
                        if (c.instance == null)
                            return new List<byte>();

                        packet.AddRange(BitConverter.GetBytes(c.socket.GetHashCode())); // ClientID
                        packet.Add((byte)c.instance.transform.childCount);              // #Transforms
                        foreach (Transform t in c.instance.transform)
                        {
                            byte[] name = new byte[4];
                            Debug.Assert(t.name.Length <= 4, "The transform name is too long! ");
                            Encoding.ASCII.GetBytes(t.name, 0, t.name.Length, name, 0);
                            packet.AddRange(name);
                            //packet.AddRange(BitConverter.GetBytes(t.GetHashCode()));

                            packet.AddRange(BitConverter.GetBytes(t.position.x));
                            packet.AddRange(BitConverter.GetBytes(t.position.y));
                            packet.AddRange(BitConverter.GetBytes(t.position.z));

                            packet.AddRange(BitConverter.GetBytes(t.rotation.x));
                            packet.AddRange(BitConverter.GetBytes(t.rotation.y));
                            packet.AddRange(BitConverter.GetBytes(t.rotation.z));
                            packet.AddRange(BitConverter.GetBytes(t.rotation.w));
                        }
                    }
                }
                //Debug.Log(packetSize + " " + packet.Count);
                break;

            // Invalid Type
            default:
                throw new InvalidOperationException("Invalid packet type");
        }

        return packet;
    }

    // Discard invalid packets, and process the valid ones
    private bool HandleClientPacket(ClientData client, byte[] packet, int size)
    {
        lock (clientsLock)
        {
            if (size == 0)
            {
                Debug.Log("Client has disconnected " + client.socket.RemoteEndPoint);
                return false;
            }

            int dataIndex = 0;
            while (dataIndex < size)
            {
                int packetLength = BitConverter.ToInt16(packet, dataIndex);
                dataIndex += sizeof(Int16);
                if (packetLength > size)
                {
                    Debug.Log("Invalid packet received. PacketLength: " + packetLength + " Received: " + size + " bytes");
                    return false;
                }

                Packet.PacketType type = (Packet.PacketType)packet[dataIndex];
                dataIndex += sizeof(byte);

                switch (type)
                {
                    case Packet.PacketType.Text:
                        {
                            string data = Encoding.ASCII.GetString(packet, dataIndex,
                                packetLength - 3);
                            dataIndex += data.Length;
                            Debug.Log("[C(" + client.socket.RemoteEndPoint + ")->S]: " + data + " (" + packetLength + " of " + size + " bytes)");
                        }
                        break;

                    case Packet.PacketType.Sensors:
                        {
                            int transformCount = packet[dataIndex++];
                            for (int i = 0; i < transformCount; i++)
                            {
                                string tName = Encoding.ASCII.GetString(packet, dataIndex, 4); dataIndex += 4;
                                //int hashCode = BitConverter.ToInt32(packet, dataIndex); dataIndex += 4;

                                float x = BitConverter.ToSingle(packet, dataIndex); dataIndex += 4;
                                float y = BitConverter.ToSingle(packet, dataIndex); dataIndex += 4;
                                float z = BitConverter.ToSingle(packet, dataIndex); dataIndex += 4;

                                float qx = BitConverter.ToSingle(packet, dataIndex); dataIndex += 4;
                                float qy = BitConverter.ToSingle(packet, dataIndex); dataIndex += 4;
                                float qz = BitConverter.ToSingle(packet, dataIndex); dataIndex += 4;
                                float qw = BitConverter.ToSingle(packet, dataIndex); dataIndex += 4;

                                Vector3 pos = new Vector3(x, y, z);
                                Quaternion rot = new Quaternion(qx, qy, qz, qw);
                                client.SetTransform(tName, new Trans(pos, rot, tName));
                                //client.SetTransform(hashCode.ToString(), new Trans(pos, rot, hashCode));
                            }
                            //if (size % packetLength != 0)
                            //    Debug.Log("[C(" + client.socket.RemoteEndPoint + ")->S]: " + type + " (" + packetLength + " of " + size + " bytes)");
                        }
                        break;

                    default:
                        Debug.Assert(false);
                        Debug.Log("Invalid PacketType" + " (" + packetLength + " of " + size + " bytes)");
                        break;
                }
            }

            return true;
        }
    }

    // Send a packet to the specified client
    private void SendServerPacket(List<byte> packet, Socket clientSocket)
    {
        if (!clientSocket.Connected || packet.Count == 0)
            return;

        short packetLen = BitConverter.ToInt16(packet.ToArray(), 0);
        Debug.Assert(packetLen == packet.Count, "packetLen: " + packetLen + " packet.Count: " + packet.Count + " Type: " + (Packet.PacketType)packet[2]);
        Packet.PacketType type = (Packet.PacketType)packet[2];

        object content = null;
        switch (type)
        {
            case Packet.PacketType.Text:
                content = Encoding.ASCII.GetString(packet.ToArray(), 3, packet.Count - 3);
                break;

            case Packet.PacketType.Spawn:
                float x = BitConverter.ToSingle(packet.ToArray(), 3 + 0);
                float y = BitConverter.ToSingle(packet.ToArray(), 3 + 4);
                float z = BitConverter.ToSingle(packet.ToArray(), 3 + 8);
                content = new Vector3(x, y, z);
                break;
        }

        if (type != Packet.PacketType.OtherClients)
        {
            Debug.Log("[S->C(" + clientSocket.RemoteEndPoint + ")]: " + type + ": " + content
                + " (" + packet.Count + " bytes)");
        }

        clientSocket.BeginSend(packet.ToArray(), 0, packet.Count, SocketFlags.None,
                new AsyncCallback(SendCallback), clientSocket);
    }

    private void RemoveDisconnectedClients()
    {
        lock (clientsLock)
        {
            for (int i = clients.Count - 1; i >= 0; i--)
            {
                if (!clients[i].socket.Connected)
                {
                    Destroy(clients[i].instance);
                    clients.Remove(clients[i]);
                }
            }
        }
    }

    public void OnSendBtn_Click()
    {
        // Get text in the input field and build a packet with it
        GameObject sendText = GameObject.Find("SendInputField");
        string text = sendText.GetComponent<InputField>().text;
        List<byte> packet = BuildServerPacket(Packet.PacketType.Text, text);

        // Send the packet to all the connected clients
        lock (clientsLock)
        {
            foreach (ClientData client in clients)
            {
                SendServerPacket(packet, client.socket);
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
