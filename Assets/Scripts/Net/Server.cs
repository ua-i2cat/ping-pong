// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using System.Net;

/// <summary>
/// This base class contains the basic API that all servers must implement
/// </summary>
public abstract class Server
{
    // Start listening on the specified port
    public abstract void Start(int port);

    // Stop the server
    public abstract void Stop();

    // Try to deliver the data to the specified client
    public abstract void Send(EndPoint client, byte[] buffer, int len);

    // Executed when a packet is sent
    public event ServerMsgEventHandler OnSend;
    protected void SendHandler(ServerMsgEventArgs e)
    {
        OnSend?.Invoke(this, e);
    }

    // Executed when a packet is received
    public event ServerMsgEventHandler OnRecv;
    protected void RecvHandler(ServerMsgEventArgs e)
    {
        OnRecv?.Invoke(this, e);
    }    

    public delegate void ServerMsgEventHandler(Object sender, ServerMsgEventArgs e);

    public class ServerMsgEventArgs : EventArgs
    {
        public ServerMsgEventArgs(EndPoint client, byte[] buffer, int len)
        {
            this.Client = client;
            this.Buffer = buffer;
            this.Len = len;
        }

        public EndPoint Client { get; set; }
        public byte[] Buffer { get; set; }
        public int Len { get; set; }
    }
}


