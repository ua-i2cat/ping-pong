using System.Collections;
// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using SharpConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

using AvatarSystem;

public class ClientManagerUDP : MonoBehaviour
{
    private Socket socket;

    // Fallback values in case the config file is not found
    private string ip = Constants.IP;
    private int port = Constants.PORT;
    private EndPoint endPoint;

    private byte[] recvBuffer = new byte[Constants.BUFF_SIZE];

    private int packetRate = 60;

    public bool Reconnect = true;
    private bool connecting = true;

    private bool justSpawned = false;
    private Trans spawn;

    // The avatar of the client itself
    public AvatarManager avatar;

    // List of other connected clients
    private Oponents oponents = new Oponents();

    // List of objects
    private List<Trans> objects = new List<Trans>();

    private bool online = false;
    private bool receivedNewText = false;
    private string recvText;
    private Text recvTextField;
    private Text onlineTxt;

    private SteamVR_TrackedController inputController;

    private void Awake()
    {
        // Fix the target framerate
        Application.targetFrameRate = 90;

        recvTextField = GameObject.Find("RecvTxt").GetComponent<Text>();
        onlineTxt = GameObject.Find("OnlineTxt").GetComponent<Text>();

        // Get ip and port from config file
        try
        {
            Configuration clientConfig = Configuration.LoadFromFile("Config.cfg");
            ip = clientConfig["Server"]["IP"].StringValue;
            port = clientConfig["Server"]["Port"].IntValue;
        }
        catch
        {
            Debug.LogWarning("Failed to load Configuration file!");
            Debug.LogWarning("Using the default values: [" + ip + ":" + port + "]");
        }

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Blocking = false;
        socket.DontFragment = true;
        port = 33334;
        endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
    }

    private void Update()
    {
        HandleInput();

        // Receive from Server
        byte[] data = new byte[socket.Available];
        while (socket.Available > 0)
        {
            try
            {
                int bytes_received = socket.ReceiveFrom(data, ref endPoint);
                HandleServerPacket(data, bytes_received);
            }
            catch
            {
                online = false;
            }
        }

        if(justSpawned)
        {
            avatar.GetController().SetTransforms(new List<Trans>() { spawn });
            justSpawned = false;
        }

        ProcessOponents();
        ProcessObjects();

        // Send Transforms
        {
            List<Trans> transforms = avatar.GetController().GetTransforms();
            Packet packet = PacketBuilder.Build(Packet.PacketType.Sensors, transforms);
            socket.SendTo(packet.ToArray(), endPoint);
        }
    }

    private void HandleInput()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            var oponent = oponents.GetOponent(0);
            Debug.Log("Id: " + oponent.Id);
            Debug.Log("TransCount: " + oponent.TransCount);
            for(int i = 0; i < oponent.TransCount; i++)
            {
                Trans t = oponent.GetTrans(i);
                Debug.Log(t.Id + " : " + t.Pos);
            }
        }

        if (inputController == null)
        {
            inputController = GameObject.FindObjectOfType<SteamVR_TrackedController>();
            if (inputController != null)
            {
                inputController.TriggerClicked += OnTriggerClicked;
            }
        }
    }

    private void OnTriggerClicked(object sender, ClickedEventArgs e)
    {
        //Debug.Log("Trigger Pressed");
        // When the server interprets this packet, the ball is served to the client that made the request
        Packet packet = PacketBuilder.Build(Packet.PacketType.Text, Constants.ServeRequest);
        socket.SendTo(packet.ToArray(), endPoint);
    }

    private void ProcessOponents()
    {
        for (int i = 0; i < oponents.Count; i++)
        {
            Oponent oponent = oponents.GetOponent(i);
            if (oponent.TransCount > 0)
            {
                Trans t = oponent.GetTrans(0);
                GameObject obj = GameObject.Find("Client (" + oponent.Id + ")");
                if (obj == null)
                {
                    // Body controlled by keyboard
                    if (oponent.TransCount == 1)
                        obj = Instantiate(Resources.Load("Avatar/AvatarNoCam")) as GameObject;

                    // Body controlled by VR sensors
                    if (oponent.TransCount > 1)
                        obj = Instantiate(Resources.Load("Avatar/AvatarVRNoCam")) as GameObject;

                    obj.transform.parent = this.transform;
                    obj.name = "Client (" + oponent.Id + ")";
                }

                for (int j = 0; j < oponent.TransCount; j++)
                {
                    t = oponent.GetTrans(j);
                    Transform child = obj.transform.FindDeepChild(t.Id);
                    if (child == null)
                    {
                        Debug.Log("Transform " + t.Id + " not found");
                        continue;
                    }
                    child.position = t.Pos;
                    child.rotation = t.Rot;
                }

                --oponent.TTL;
                if (oponent.TTL <= 0)
                    Destroy(obj);
            }
        }

        for (int i = oponents.Count - 1; i >= 0; i--)
        {
            Oponent o = oponents.GetOponent(i);
            if (o.TTL < 0)
            {
                oponents.RemoveOponent(o.Id);
            }
        }
    }

    private void ProcessObjects()
    {
        foreach (var obj in objects)
        {
            var instance = GameObject.Find(obj.Id);
            if (instance == null)
            {
                instance = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                instance.name = obj.Id;
                instance.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            }
            instance.transform.position = obj.Pos;
            instance.transform.rotation = obj.Rot;
        }
    }

    private bool HandleServerPacket(byte[] data, int size)
    {
        if (size == 0)
        {
            Debug.Log("Disconnected from the server");
            online = false;
            return false;
        }
        online = true;

        int dataIndex = 0;
        Packet packet = PacketBuilder.Parse(data, ref dataIndex);

        // Process the packet
        switch (packet.Type)
        {
            case Packet.PacketType.Text:
                recvText = ((PacketText)packet).Data;
                receivedNewText = true;
                Debug.Log("[S->C]: " + recvText + " (" + packet.Size + " of " + size + " bytes)");
                break;

            case Packet.PacketType.Spawn:
                spawn = ((PacketSpawn)packet).Data;
                justSpawned = true;
                Debug.Log("[S->C]: Spawn Position = " + spawn.Pos + " (" + packet.Size + " of " + size + " bytes)");
                break;

            case Packet.PacketType.OtherClients:
                var others = ((PacketOtherClients)packet).Data;
                foreach (var c in others)
                {
                    // Add or update an oponent by its Id (and restore its TTL)
                    Oponent oponent = oponents.AddOponent(c.Id);
                    for (int i = 0; i < c.TransCount; i++)
                        oponent.AddTransform(c.GetTrans(i));
                }
                break;

            case Packet.PacketType.Objects:
                List<Trans> objectsRecv = ((PacketObjects)packet).Data;
                foreach (var o in objectsRecv)
                {
                    Trans t = objects.Where(x => x.Id == o.Id).FirstOrDefault();
                    if (t == null)
                    {
                        Debug.Log("Received " + o.Id + " for the first time");
                        objects.Add(o);
                    }
                    else
                    {
                        t.Pos = o.Pos;
                        t.Rot = o.Rot;
                    }
                }
                break;

            case Packet.PacketType.Benchmark:
                NetBenchmarks b = ((PacketBenchmark)packet).Data;
                b.recvTimeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                Packet p = PacketBuilder.Build(Packet.PacketType.Benchmark, b);
                socket.SendTo(p.ToArray(), endPoint);
                break;

            default:
                Debug.Assert(false);
                Debug.LogError("Invalid PacketType" + " (" + packet.Size + " of " + size + " bytes)");
                break;
        }

        return true;
    }

    private void OnGUI()
    {
        if (receivedNewText)
        {
            recvTextField.text = "[" + DateTime.Now.ToString("hh:mm:ss") + "]: " + recvText;
            receivedNewText = false;
        }

        if (online)
        {
            onlineTxt.text = Constants.OnlineText;
            onlineTxt.color = Color.green;
        }
        else
        {
            onlineTxt.text = Constants.OfflineText;
            onlineTxt.color = Color.red;
        }
    }

    public void OnSendBtn_Click()
    {
        GameObject sendText = GameObject.Find(Constants.SendInputField);
        Debug.Assert(sendText != null);
        string text = sendText.GetComponent<InputField>().text;
        Debug.Log("[C->S]: " + text);
        if (text != string.Empty)
        {
            Packet packet = PacketBuilder.Build(Packet.PacketType.Text, text);
            socket.SendTo(packet.ToArray(), new IPEndPoint(IPAddress.Parse(ip), port));
        }
    }
}
