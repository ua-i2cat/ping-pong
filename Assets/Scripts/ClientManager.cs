// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using System.Collections;
using System.Collections.Generic;
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
    public string ip = Constants.IP;
    public int port = Constants.PORT;

    private const int BUFF_SIZE = 8192;
    private byte[] recvBuffer = new byte[BUFF_SIZE];

    private int packetRate = 60;

    public bool Reconnect = true;
    private bool connecting = true;

    //private GameObject rig;
    private bool spawned = false;
    private Vector3 spawnPos;
    private Trans spawn;

    public AvatarManager avatarManager;

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
    }

    private void Start()
    {
        //foreach (string tag in new string[] { "Head", "LHand", "RHand", "Hip", "LFoot", "RFoot" })
        //{
        //    GameObject[] obj = GameObject.FindGameObjectsWithTag(tag);
        //    foreach (var o in obj)
        //    {
        //        Debug.Log(o.name);
        //        Destroy(o);
        //    }
        //}
    }

    private void Update()
    {
        if (socket.Connected)
        {
            connecting = false;
            SendClientPacket(BuildClientPacket(Packet.PacketType.Sensors));
        }
        else if (!connecting && Reconnect)
        {
            connecting = true;
            Connect();
        }

        if (spawned)
        {
            // Move Body and Rig? to the position and rotation received
            avatarManager.Body.transform.position = spawn.Pos;
            avatarManager.Body.transform.rotation = spawn.Rot;
            spawned = false;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Socket: " + socket.Connected);
        }

        for (int i = 0; i < oponents.Count; i++)
        {
            Oponent oponent = oponents.GetOponent(i);
            if (oponent.TransCount > 0)
            {
                Trans t = oponent.GetTrans(0);
                //Debug.Log(t.Id);
                GameObject obj = GameObject.Find(oponent.Id + " - " + t.Id);
                if (obj == null)
                {
                    //obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    obj = Instantiate(Resources.Load("Rig")) as GameObject;
                    obj.name = oponent.Id + " - " + t.Id;
                }
                obj.transform.position = t.Pos;
                obj.transform.rotation = t.Rot;

                for (int j = 1; j < oponent.TransCount; j++)
                {
                    t = oponent.GetTrans(j);
                    //Debug.Log(t.Id);
                    Transform child = obj.transform.Find(t.Id);
                    child.position = t.Pos;
                    child.rotation = t.Rot;
                }

                --oponent.TTL;
                if (oponent.TTL <= 0)
                    Destroy(obj);
            }
        }

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
            //int bytes_sent = socket.EndSend(AR);
            //if (bytes_sent % 228 != 0)
            //    Debug.Log(bytes_sent + " bytes sent");

            Thread.Sleep(1000 / packetRate);
        }
        catch
        {
            Debug.Log("Exception");
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

    private List<byte> BuildClientPacket(Packet.PacketType type, object data = null)
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

            // Sensor Data (Ref + 6 sensors)                
            case Packet.PacketType.Sensors:
                int transformCount = avatarManager.ControllerRig.GetTransformCount();
                int packetSize = sizeof(short)  // 2 bytes: Length of the packet (in bytes)
                    + sizeof(byte)              // 1 byte:  Packet type
                    + sizeof(byte)              // 1 byte:  Transform Count (up to 255)
                    //+ sendTransforms.Count * Trans.Size;
                    + transformCount * Trans.Size;

                packet.AddRange(BitConverter.GetBytes((short)packetSize));
                packet.Add((byte)Packet.PacketType.Sensors);
                packet.Add((byte)transformCount);

                //foreach (var transform in sendTransforms)
                for(int i = 0; i < transformCount; i++)
                {
                    var t = avatarManager.ControllerRig.GetTransform(i);

                    byte[] name = new byte[4];
                    //Debug.Assert(transform.name.Length <= 4, "The transform name is too long!");
                    //Encoding.ASCII.GetBytes(transform.name, 0, transform.name.Length, name, 0);
                    Encoding.ASCII.GetBytes(t.Key, 0, t.Key.Length, name, 0);

                    packet.AddRange(name);
                    //packet.AddRange(BitConverter.GetBytes(transform.GetHashCode()));

                    packet.AddRange(BitConverter.GetBytes(t.Value.position.x));
                    packet.AddRange(BitConverter.GetBytes(t.Value.position.y));
                    packet.AddRange(BitConverter.GetBytes(t.Value.position.z));

                    packet.AddRange(BitConverter.GetBytes(t.Value.rotation.x));
                    packet.AddRange(BitConverter.GetBytes(t.Value.rotation.y));
                    packet.AddRange(BitConverter.GetBytes(t.Value.rotation.z));
                    packet.AddRange(BitConverter.GetBytes(t.Value.rotation.w));
                }
                break;

            // Invalid Type
            default:
                throw new InvalidOperationException("Invalid packet type");
        }

        return packet;
    }

    private bool HandleServerPacket(byte[] packet, int size)
    {
        if (size == 0)
        {
            Debug.Log("Disconnected from the server");
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
                        recvText = data;
                        receivedNewText = true;
                        Debug.Log("[S->C]: " + data + " (" + packetLength + " of " + size + " bytes)");
                    }
                    break;

                case Packet.PacketType.Sensors:
                    Debug.Log("[S->C]: Sensor Data");
                    break;

                case Packet.PacketType.Spawn:
                    {
                        float x = BitConverter.ToSingle(packet, dataIndex); dataIndex += 4;
                        float y = BitConverter.ToSingle(packet, dataIndex); dataIndex += 4;
                        float z = BitConverter.ToSingle(packet, dataIndex); dataIndex += 4;
                        float qx = BitConverter.ToSingle(packet, dataIndex); dataIndex += 4;
                        float qy = BitConverter.ToSingle(packet, dataIndex); dataIndex += 4;
                        float qz = BitConverter.ToSingle(packet, dataIndex); dataIndex += 4;
                        float qw = BitConverter.ToSingle(packet, dataIndex); dataIndex += 4;
                        spawn = new Trans(new Vector3(x, y, z), new Quaternion(qx, qy, qz, qw));
                        spawned = true;
                        Debug.Log("[S->C]: Spawn Position = " + spawn.Pos + " (" + packetLength + " of " + size + " bytes)");
                    }
                    break;

                case Packet.PacketType.OtherClients:
                    {
                        byte clientCount = packet[dataIndex++];
                        for (int i = 0; i < clientCount; i++)
                        {
                            int clientId = BitConverter.ToInt32(packet, dataIndex); dataIndex += 4;
                            Oponent oponent = oponents.AddOponent(clientId);
                            byte transformCount = packet[dataIndex++];

                            for (int j = 0; j < transformCount; j++)
                            {
                                string tName = Encoding.ASCII.GetString(packet, dataIndex, 4); dataIndex += 4;
                                //int transformId = BitConverter.ToInt32(packet, dataIndex); dataIndex += 4;

                                float x = BitConverter.ToSingle(packet, dataIndex); dataIndex += 4;
                                float y = BitConverter.ToSingle(packet, dataIndex); dataIndex += 4;
                                float z = BitConverter.ToSingle(packet, dataIndex); dataIndex += 4;

                                float qx = BitConverter.ToSingle(packet, dataIndex); dataIndex += 4;
                                float qy = BitConverter.ToSingle(packet, dataIndex); dataIndex += 4;
                                float qz = BitConverter.ToSingle(packet, dataIndex); dataIndex += 4;
                                float qw = BitConverter.ToSingle(packet, dataIndex); dataIndex += 4;

                                Vector3 pos = new Vector3(x, y, z);
                                Quaternion rot = new Quaternion(qx, qy, qz, qw);

                                oponent.AddTransform(new Trans(pos, rot, tName));
                                //oponent.AddTransform(new Trans(pos, rot, transformId));
                            }
                        }
                        //Debug.Assert(clientCount == 0);
                        //Debug.Log("[S->C]: Other Clients Info" + " (" + packetLength + " of " + size + " bytes)");
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

    private void Connect()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.BeginConnect(IPAddress.Parse(ip), port, new AsyncCallback(ConnectCallback), null);
        Debug.Log("Connecting to the server...");
    }
    private void Disconnect()
    {
        socket.Close();
        Debug.Log("Socket closed");
    }

    private void SendClientPacket(List<byte> packet)
    {
        if (socket.Connected && packet.Count > 0)
        {
            socket.BeginSend(packet.ToArray(), 0, packet.Count, SocketFlags.None,
                    new AsyncCallback(SendCallback), socket);
        }
    }

    public void OnSendBtn_Click()
    {
        GameObject sendText = GameObject.Find("SendInputField");
        Debug.Assert(sendText != null);
        string text = sendText.GetComponent<InputField>().text;
        Debug.Log("[C->S]: " + text);
        List<byte> packet;
        if (text != string.Empty)
            packet = BuildClientPacket(Packet.PacketType.Text, text);
        else
            packet = BuildClientPacket(Packet.PacketType.Sensors);

        SendClientPacket(packet);
    }
}
