using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class ClientTCP : Client
{
    private Socket socket;

    private byte[] recvBuffer = new byte[8192];

    public override void Start(string ip, int port)
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.BeginConnect(IPAddress.Parse(ip), port, new AsyncCallback(ConnectCallback), null);
    }

    public override void Stop()
    {
        socket.Close();
    }

    public override void Send(byte[] buffer, int len)
    {
        socket.BeginSend(buffer, 0, len, SocketFlags.None, new AsyncCallback(SendCallback), buffer);
    }

    protected virtual void OnConnect()
    {
        //Debug.Log("OnConnect");
    }

    protected virtual void OnDisconnect()
    {
        //Debug.Log("OnDisconnect");
    }

    private void ConnectCallback(IAsyncResult AR)
    {
        try
        {
            socket.EndConnect(AR);
            OnConnect();
            socket.BeginReceive(recvBuffer, 0, recvBuffer.Length, SocketFlags.None,
                new AsyncCallback(ReceiveCallback), null);
        }
        catch
        {
            Debug.Log("ConnectCallback error");
        }
    }

    private void ReceiveCallback(IAsyncResult AR)
    {
        try
        {
            if(!socket.Connected)
            {
                OnDisconnect();
                return;
            }
            int bytesRecv = socket.EndReceive(AR);
            if(bytesRecv <= 0)
            {
                OnDisconnect();
                return;
            }
            OnRecv(new ClientMsgEventArgs(recvBuffer, bytesRecv));
            socket.BeginReceive(recvBuffer, 0, recvBuffer.Length, SocketFlags.None,
                new AsyncCallback(ReceiveCallback), null);
        }
        catch
        {
            Debug.Log("ReceiveCallback error");
        }
    }

    private void SendCallback(IAsyncResult AR)
    {
        try
        {
            byte[] buffer = (byte[])AR.AsyncState;
            int bytesSent = socket.EndSend(AR);
            OnSend(new ClientMsgEventArgs(buffer, bytesSent));
        }
        catch
        {
            Debug.Log("SendCallback error");
        }
    }
}
