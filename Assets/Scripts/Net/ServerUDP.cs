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

    private int run = 1;

    public override void Start(int port)
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Blocking = false;
        socket.Bind(new IPEndPoint(IPAddress.Any, port));
        Debug.Log("[Server] Socket Listening");

        serverThread = new Thread(new ThreadStart(ServerLoop));
        serverThread.Start();
    }

    public override void Stop()
    {
        Interlocked.Decrement(ref run);
        if(serverThread != null)
            serverThread.Join();
    }

    public override void Send(EndPoint client, byte[] buffer, int len)
    {
        try
        {
            socket.SendTo(buffer, len, SocketFlags.None, client);
            OnSend(new ServerMsgEventArgs(client, buffer, len));
        }
        catch
        {
            Debug.Log("Error sending");
        }
    }

    private void ServerLoop()
    {
        try
        {
            byte[] data = new byte[8192];
            while (run > 0)
            {
                while (socket.Available > 0)
                {
                    EndPoint client = new IPEndPoint(IPAddress.Any, 0);
                    try
                    {
                        int bytesRecv = socket.ReceiveFrom(data, ref client);
                        //Debug.Log("Received " + bytesRecv + " bytes from " + client.ToString());
                        OnRecv(new ServerMsgEventArgs(client, data, bytesRecv));
                    }
                    catch
                    {
                        //Debug.Log("Exception on receive but continuing ServerLoop. Available: " + socket.Available);
                        break;
                    }
                }

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
