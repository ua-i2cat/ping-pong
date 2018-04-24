// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using System.IO;

/// <summary>
/// This base class contains the basic API that all clients must implement
/// </summary>
public abstract class Client
{
    // Start a connection to the Server at ip:port or start receiving if the client is connectionless
    public abstract void Start(string ip, int port);

    // Disconnect and/or stop receiving
    public abstract void Stop();

    // Try to deliver the data to the server
    public abstract void Send(byte[] buffer, int len);

    // Executed when a packet is sent
    public event ClientMsgEventHandler OnSend;
    protected void SendHandler(ClientMsgEventArgs e)
    {
        OnSend?.Invoke(this, e);
    }

    // Executed when a packet is received
    public event ClientMsgEventHandler OnRecv;
    protected void RecvHandler(ClientMsgEventArgs e)
    {
        OnRecv?.Invoke(this, e);
    }

    // Executed when an error happens
    public event ErrorEventHandler OnError;
    protected void ErrorHandler(ErrorEventArgs e)
    {
        OnError?.Invoke(this, e);
    }

    public delegate void ClientMsgEventHandler(Object sender, ClientMsgEventArgs e);

    public class ClientMsgEventArgs : EventArgs
    {
        public ClientMsgEventArgs(byte[] buffer, int len)
        {
            this.Buffer = buffer;
            this.Len = len;
        }

        public byte[] Buffer { get; set; }
        public int Len { get; set; }
    }
}
