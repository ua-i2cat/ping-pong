// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class ClientUDP : Client
{
    private Socket socket;

    private EndPoint server;

    private Thread clientThread;

    private int run = 1;

    public override void Start(string ip, int port)
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Blocking = false;

        server = new IPEndPoint(IPAddress.Parse(ip), port);

        clientThread = new Thread(new ThreadStart(ClientLoop));
        clientThread.Start();
    }

    public override void Stop()
    {
        Interlocked.Decrement(ref run);
        clientThread.Join();
    }

    public override void Send(byte[] buffer, int len)
    {
        socket.SendTo(buffer, len, SocketFlags.None, server);
        OnSend(new ClientMsgEventArgs(buffer, len));
    }

    private void ClientLoop()
    {
        while (run > 0)
        {
            byte[] data = new byte[socket.Available];
            while (socket.Available > 0)
            {
                int bytesRecv = socket.ReceiveFrom(data, ref server);
                OnRecv(new ClientMsgEventArgs(data, bytesRecv));
            }

            Thread.Sleep(1);
        }

        Debug.Log("[Client] Closing socket");
        socket.Close();
    }
}
