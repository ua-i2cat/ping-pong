// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class ServerUDP : Server
{
    private Socket socket;
    private Thread serverThread;

    // Run flag, we use an int since booleans are not supported by Interlocked
    private int run = 1;

    public override void Start(int port)
    {
        // Create non-blocking UDP socket
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Blocking = false;

        // Bind socket to the port passed as argument
        socket.Bind(new IPEndPoint(IPAddress.Any, port));
        Debug.Log("[Server] Socket Listening");

        // Start ServerLoop in a new thread
        serverThread = new Thread(new ThreadStart(ServerLoop));
        serverThread.Start();
    }

    public override void Stop()
    {
        // Set running flag to false and wait for the serverThread to finish
        Interlocked.Decrement(ref run);
        if(serverThread != null)
            serverThread.Join();
    }

    public override void Send(EndPoint client, byte[] buffer, int len)
    {
        try
        {
            socket.SendTo(buffer, len, SocketFlags.None, client);
            SendHandler(new ServerMsgEventArgs(client, buffer, len));
        }
        catch(SocketException e)
        {
            Debug.Log(e);
        }
    }

    private void ServerLoop()
    {
        try
        {
            byte[] data = new byte[Constants.BUFF_SIZE];
            while (run > 0)
            {
                while (socket.Available > 0)
                {
                    EndPoint client = new IPEndPoint(IPAddress.Any, 0);
                    try
                    {
                        int bytesRecv = socket.ReceiveFrom(data, ref client);
                        RecvHandler(new ServerMsgEventArgs(client, data, bytesRecv));
                    }
                    catch(SocketException e)
                    {
                        // Disconnection of clients has to be handled by the application
                        if(e.SocketErrorCode != SocketError.ConnectionReset)
                            Debug.Log(e);

                        break;
                    }
                }

                // Do not starve other threads
                Thread.Sleep(1);
            }

            Debug.Log("[Server] Closing socket");
            socket.Close();
        }
        catch(Exception e)
        {
            Debug.Log(e);
            Debug.Log("Exception in ServerLoop!!");
        }
    }
}
