// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

/// <summary>
/// Specialization of the Server class using the TCP protocol
/// </summary>
public class ServerTCP : Server
{
    // Contains the underlying socket used for listening and accepting incoming connections
    private TcpListener listener;

    // Map of the active connections with their receive buffer
    private Dictionary<Socket, byte[]> recvBuffers = new Dictionary<Socket, byte[]>();

    public override void Start(int port)
    {
        // Bind to the specified port and start listening
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start(/*MAX_PENDING_CONNECTIONS*/);

        // Start accepting connections asynchronously
        listener.BeginAcceptSocket(new AsyncCallback(AcceptCallback), null);
    }

    public override void Stop()
    {
        // Close the active connections
        foreach (var pair in recvBuffers)
        {
            pair.Key.Close();
        }

        // Stop listening
        listener.Stop();
    }

    public override void Send(EndPoint client, byte[] buffer, int len)
    {
        try
        {
            int index = 0;
            Packet p = PacketBuilder.Parse(buffer, ref index);
            //if (p.Type != Packet.PacketType.Objects)
            //    UnityEngine.Debug.Log("[S->C]: " + p.ToString());

            // Find an active connection for the given client EndPoint
            var pair = recvBuffers.Where(x => x.Key.RemoteEndPoint != null && x.Key.RemoteEndPoint.Equals(client)).FirstOrDefault();

            if (pair.Key != null)
            {
                // Get the socket from the connection
                Socket socket = pair.Key;

                //UnityEngine.Debug.Log("Send");

                // Send the packet asynchronously to the client
                socket.BeginSend(buffer, 0, len, SocketFlags.None, new AsyncCallback(SendCallback), new Tuple<Socket, byte[]>(socket, buffer));

                Thread.Sleep(1);
            }
        }
        catch(Exception e)
        {
            UnityEngine.Debug.LogError(e);
            Console.WriteLine(e);
        }
    }

    // Executed when there is an incomming connection
    public event ConnectEventHandler ClientConnect;
    private void OnConnect(ConnectEventArgs e)
    {
        UnityEngine.Debug.Log("OnConnect " + e.Socket.RemoteEndPoint);
        ClientConnect?.Invoke(this, e);
    }

    // Executed when a client is disconnected
    public event ConnectEventHandler ClientDisconnect;
    private void OnDisconnect(ConnectEventArgs e)
    {
        UnityEngine.Debug.Log("OnDisconnect");
        ClientDisconnect?.Invoke(this, e);
    }

    // Executed when there is an incomming connection
    private void AcceptCallback(IAsyncResult AR)
    {
        try
        {
            // Accept the connection
            Socket socket = listener.EndAcceptSocket(AR);
            OnConnect(new ConnectEventArgs(socket));

            // Create a buffer and add it to the connection map
            recvBuffers.Add(socket, new byte[8192]);
            byte[] recvBuffer = recvBuffers.Where(x => x.Key.RemoteEndPoint != null && x.Key.RemoteEndPoint.Equals(socket.RemoteEndPoint)).FirstOrDefault().Value;

            // Start receiving packets asynchronously from the connected client
            socket.BeginReceive(recvBuffer, 0, recvBuffer.Length, SocketFlags.None, new AsyncCallback(RecvCallback), socket);
        }
        catch
        {
            UnityEngine.Debug.Log("AcceptCallback error");
            throw new Exception("AcceptCallback error");
        }

        // Keep listening for other connections
        listener.BeginAcceptSocket(new AsyncCallback(AcceptCallback), null);
    }

    // Executed when a packet is received or a client has disconnected (0 bytes received)
    private void RecvCallback(IAsyncResult AR)
    {
        Socket socket = (Socket)AR.AsyncState;

        try
        {
            int bytesRecv = socket.EndReceive(AR);
            byte[] recvBuffer = recvBuffers.Where(x => x.Key.RemoteEndPoint.Equals(socket.RemoteEndPoint)).FirstOrDefault().Value;
            RecvHandler(new ServerMsgEventArgs(socket.RemoteEndPoint, recvBuffer, bytesRecv));

            // Keep receiving packets
            socket.BeginReceive(recvBuffer, 0, recvBuffer.Length, SocketFlags.None, new AsyncCallback(RecvCallback), socket);
        }
        catch(Exception e)
        {
            //UnityEngine.Debug.Log(e);
            recvBuffers.Remove(socket);
            OnDisconnect(new ConnectEventArgs(socket));
            throw new Exception("RecvCallback error");
        }
    }

    private void SendCallback(IAsyncResult AR)
    {
        try
        {
            var tuple = (Tuple<Socket, byte[]>)AR.AsyncState;
            Socket socket = tuple.Item1;
            int bytesSent = socket.EndSend(AR);
            SendHandler(new ServerMsgEventArgs(socket.RemoteEndPoint, tuple.Item2, bytesSent));
        }
        catch
        {
            UnityEngine.Debug.Log("SendCallback error");
            throw new Exception("SendCallback error");
        }
    }

    public delegate void ConnectEventHandler(Object sender, ConnectEventArgs e);
    public class ConnectEventArgs : EventArgs
    {
        public ConnectEventArgs(Socket s)
        {
            this.Socket = s;
        }

        public Socket Socket { get; set; }
    }
}
