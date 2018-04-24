// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class ClientUDP : Client
{
    private Socket socket;
    private EndPoint server;
    private Thread clientThread;

    // Run flag, we use an int since booleans are not supported by Interlocked
    private int run = 1;

    public override void Start(string ip, int port)
    {
        // Create non-blocking UDP socket
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Blocking = false;

        // Cache server endPoint
        server = new IPEndPoint(IPAddress.Parse(ip), port);

        // Start ClientLoop in a new thread
        clientThread = new Thread(new ThreadStart(ClientLoop));
        clientThread.Start();
    }

    public override void Stop()
    {
        // Set running flag to false and wait for the clientThread to finish
        Interlocked.Decrement(ref run);
        if(clientThread != null)
            clientThread.Join();
    }

    public override void Send(byte[] buffer, int len)
    {
        try
        {
            if (run > 0)
            {
                socket.SendTo(buffer, len, SocketFlags.None, server);
                SendHandler(new ClientMsgEventArgs(buffer, len));
            }
        }
        catch(SocketException e)
        {
            //Debug.Log(e);
            ErrorHandler(new ErrorEventArgs(e));
        }
    }

    private void ClientLoop()
    {
        try
        {
            byte[] data = new byte[Constants.BUFF_SIZE];
            while (run > 0)
            {                
                while (socket.Available > 0)
                {
                    try
                    {
                        int bytesRecv = socket.ReceiveFrom(data, ref server);
                        RecvHandler(new ClientMsgEventArgs(data, bytesRecv));
                    }
                    catch(SocketException e)
                    {
                        ErrorHandler(new ErrorEventArgs(e));
                        //Debug.Log(e);
                        if(socket != null && socket.Connected)
                            socket.Close();
                        // e.SocketErrorCode == SocketError.ConnectionReset
                    }
                }

                // Do not starve other threads
                Thread.Sleep(1);
            }

            Debug.Log("[Client] Closing socket");
            socket.Close();
        }
        catch(Exception e)
        {
            run = 0;
            //Debug.Log("Exception in ClientLoop!!");
            ErrorHandler(new ErrorEventArgs(e));
        }
    }
}
