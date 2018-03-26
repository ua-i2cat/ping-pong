// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using SharpConfig;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class ClientManager : MonoBehaviour
{
    private Socket socket;

    // Hardcoded default values
    public string ip = Constants.IP;
    public int port = Constants.PORT;

    private const int BUFF_SIZE = 8192;
    private byte[] recvBuffer = new byte[BUFF_SIZE];

    private int packetRate = 60;

    public bool Reconnect = true;
    private bool connecting = true;

    //private GameObject rig;
    private bool justSpawned = false;
    private Vector3 spawnPos;
    private Trans spawn;

    public AvatarManager avatarManager;
    public avatar.AvatarManager avatar;

    private Oponents oponents = new Oponents();

    private bool receivedNewText = false;
    private string recvText;
    private Text recvTextField;
    private Text onlineTxt;

    private void Awake()
    {
        // Fix the target framerate
        Application.targetFrameRate = 90;

        // Connect to the server
        Connect();

        recvTextField = GameObject.Find("RecvTxt").GetComponent<Text>();
        onlineTxt = GameObject.Find("OnlineTxt").GetComponent<Text>();

        // Get ip and port from config file
        Configuration clientConfig = Configuration.LoadFromFile("Config.cfg");
        ip = clientConfig["Server"]["IP"].StringValue;
        port = clientConfig["Server"]["Port"].IntValue;
    }

    private void Update()
    {
        HandleInput();

        if (socket.Connected)
        {
            connecting = false;

            List<Trans> transforms = avatar.controller.GetTransforms();
            Packet packet = PacketBuilder.Build(Packet.PacketType.Sensors, transforms);
            packet.Send(socket, new AsyncCallback(SendCallback));
        }
        else if (!connecting && Reconnect)
        {
            connecting = true;
            Connect();
        }

        if (justSpawned)
        {
            // Move Body and Rig? to the position and rotation received
            //avatarManager.Body.transform.position = spawn.Pos;
            //avatarManager.Body.transform.rotation = spawn.Rot;
            avatar.controller.SetTransforms(new List<Trans>() { spawn });
            justSpawned = false;
        }

        ProcessOponents();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Socket: " + socket.Connected);
        }
        else if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void ProcessOponents()
    {
        for (int i = 0; i < oponents.Count; i++)
        {
            Oponent oponent = oponents.GetOponent(i);
            if (oponent.TransCount > 0)
            {
                Trans t = oponent.GetTrans(0);
                //GameObject obj = GameObject.Find(oponent.Id + " - " + t.Id);
                GameObject obj = GameObject.Find("Client (" + oponent.Id + ")");
                if (obj == null)
                {
                    //var o = Instantiate(Resources.Load("AvatarManagerNet")) as GameObject;
                    //o.transform.parent = this.transform;

                    // Body controlled by keyboard
                    if(oponent.TransCount == 1)
                        obj = Instantiate(Resources.Load("NewAvatar/AvatarNoCam")) as GameObject;

                    // Body controlled by IK
                    if (oponent.TransCount > 1)
                        obj = Instantiate(Resources.Load("NewAvatar/AvatarVRNoCam")) as GameObject;

                    obj.transform.parent = this.transform;
                    obj.name = "Client (" + oponent.Id + ")";
                }

                for (int j = 0; j < oponent.TransCount; j++)
                {
                    t = oponent.GetTrans(j);
                    Transform child = obj.transform.Find(t.Id);
                    if (child == null)
                    {
                        Debug.Log("Transform " + t.Id + " not found");
                        continue;
                    }
                    //Debug.Log("Transform: " + t.Id);
                    child.position = t.Pos;
                    child.rotation = t.Rot;
                }

                --oponent.TTL;
                if (oponent.TTL <= 0)
                    Destroy(obj);
            }
        }

        for(int i = oponents.Count - 1; i >= 0; i--)
        {
            Oponent o = oponents.GetOponent(i);
            if(o.TTL < 0)
            {
                oponents.RemoveOponent(o.Id);
            }
        }
    }

    private void OnGUI()
    {
        if (receivedNewText)
        {
            recvTextField.text = "[" + DateTime.Now.ToString("hh:mm:ss") + "]: " + recvText;
            receivedNewText = false;
        }

        if (socket.Connected)
        {
            onlineTxt.text = "Online";
            onlineTxt.color = Color.green;
        }
        else
        {
            onlineTxt.text = "Offline";
            onlineTxt.color = Color.red;
        }
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    #region Callbacks
    private void ConnectCallback(IAsyncResult AR)
    {
        try
        {
            // Establish the connection and set Socket attributes
            socket.EndConnect(AR);
            socket.NoDelay = true;
            socket.DontFragment = true;
            socket.ReceiveBufferSize = BUFF_SIZE;
            socket.SendBufferSize = BUFF_SIZE;
            Debug.Log("Connection established");
            connecting = false;

            // Begin receiving data
            socket.BeginReceive(recvBuffer, 0, recvBuffer.Length, SocketFlags.None,
                ReceiveCallback, null);
        }
        catch
        {
            Thread.Sleep(3000);
            Debug.Log("Server unavailable");
            if (Reconnect)
            {
                Connect();
            }
        }
    }

    // Triggered when sending a packet
    private void SendCallback(IAsyncResult AR)
    {
        try
        {
            Socket socket = (Socket)AR.AsyncState;
            /*int bytes_sent = */socket.EndSend(AR);
            //Debug.Log(bytes_sent + " bytes sent");

            Thread.Sleep(1000 / packetRate);
        }
        catch
        {
            Debug.LogWarning("Could not send the packet");
        }
    }

    // Triggered when a packet is received
    private void ReceiveCallback(IAsyncResult AR)
    {
        int bytes_recv = socket.EndReceive(AR);
        if (HandleServerPacket(recvBuffer, bytes_recv))
        {
            // Keep receiving data
            Thread.Sleep(1000 / packetRate);
            socket.BeginReceive(recvBuffer, 0, recvBuffer.Length, SocketFlags.None,
                new AsyncCallback(ReceiveCallback), socket);
        }
        else
        {
            // Reconnect
            Debug.Log("Closing Socket");
            socket.Close();
            Connect();
        }
    }
    #endregion

    private bool HandleServerPacket(byte[] data, int size)
    {
        if (size == 0)
        {
            Debug.Log("Disconnected from the server");
            return false;
        }

        int dataIndex = 0;
        while (dataIndex < size)
        {
            // Parse a packet from the data buffer
            Packet packet = PacketBuilder.Parse(data, ref dataIndex);

            // Process the packet
            switch(packet.Type)
            {
                case Packet.PacketType.Text:
                    string text = ((PacketText)packet).Data;
                    recvText = text;
                    receivedNewText = true;
                    Debug.Log("[S->C]: " + text + " (" + packet.Size + " of " + size + " bytes)");
                    break;

                case Packet.PacketType.Spawn:
                    Trans trans = ((PacketSpawn)packet).Data;
                    spawn = trans;
                    justSpawned = true;
                    Debug.Log("[S->C]: Spawn Position = " + spawn.Pos + " (" + packet.Size + " of " + size + " bytes)");
                    break;

                case Packet.PacketType.OtherClients:
                    var ocs = ((PacketOtherClients)packet).Data;
                    foreach(var c in ocs)
                    {
                        Oponent oponent = oponents.AddOponent(c.Id);
                        for (int i = 0; i < c.TransCount; i++)
                            oponent.AddTransform(c.GetTrans(i));
                    }
                    break;

                default:
                    Debug.Assert(false);
                    Debug.Log("Invalid PacketType" + " (" + packet.Size + " of " + size + " bytes)");
                    break;
            }
        }

        return true;
    }

    private void Connect()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.BeginConnect(IPAddress.Parse(ip), port, new AsyncCallback(ConnectCallback), null);
        Debug.Log("Connecting to the server at [" + ip + ":" + port + "] ...");
    }
    private void Disconnect()
    {
        socket.Close();
        Debug.Log("Socket closed");
    }

    public void OnSendBtn_Click()
    {
        GameObject sendText = GameObject.Find("SendInputField");
        Debug.Assert(sendText != null);
        string text = sendText.GetComponent<InputField>().text;
        Debug.Log("[C->S]: " + text);
        if (text != string.Empty)
        {
            Packet packet = PacketBuilder.Build(Packet.PacketType.Text, text);
            packet.Send(socket, new AsyncCallback(SendCallback));
        }
    }
}
