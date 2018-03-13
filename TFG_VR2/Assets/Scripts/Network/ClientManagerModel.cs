// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
//using UnityEditor;
using UnityEngine;

public class ClientManagerModel : MonoBehaviour
{
    private Socket socket;
    private string ip = "127.0.0.1";
    private int port = 3333;
    private byte[] recvBuffer = new byte[8192];
    private int packetsPerSecond = 30;

    public GameObject playerPrefab;
    public GameObject ballPrefab;
    public WorldState world = new WorldState(Authority.Client);

    public Transform hmd;
    public Transform controllerLeft;
    public Transform controllerRight;
    public Transform trackerHip;
    public Transform trackerLeftFoot;
    public Transform trackerRightFoot;

    private Trans hmdT              = new Trans();
    private Trans controllerLeftT   = new Trans();
    private Trans controllerRightT  = new Trans();
    private Trans trackerHipT       = new Trans();
    private Trans trackerLeftFootT  = new Trans();
    private Trans trackerRightFootT = new Trans();

    private bool connected = false;

    public GameObject localClient;

    private void Awake()
    {
        //Debug.Assert(localClient != null);
        //string path = AssetDatabase.GetAssetPath(PrefabUtility.GetPrefabParent(localClient));
        //Debug.Log(path);
        //Debug.Assert(path != string.Empty);
        //Guid guid = new Guid(AssetDatabase.AssetPathToGUID(path));
        //Debug.Log(guid.ToString("N")); // convert to string without hyphens
        //byte[] bytes = guid.ToByteArray(); // serialize
        // payload.AddRange(bytes);
        //path = AssetDatabase.GUIDToAssetPath(guid.ToString("N"));
        //GameObject obj = Instantiate(AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject);

        //localClient.GetComponentInChildren<IClientPrefab>().GetMotionData();
    }

    void Start()
    {
        Application.targetFrameRate = 90;

        world.playerPrefab = playerPrefab;
        //world.ballPrefab = ballPrefab;

        if (!connected)
        {
            Invoke("Init", 2);
        }
    }

    // Enable OpenVR at startup without having to go to Edit->PlayerSettings
    //void Awake()
    //{
    //    StartCoroutine(LoadDevice("OpenVR"));
    //}

    //IEnumerator LoadDevice(string newDevice)
    //{
    //    UnityEngine.XR.XRSettings.LoadDeviceByName(newDevice);
    //    yield return null;
    //    UnityEngine.XR.XRSettings.enabled = true;
    //}

    void OnGUI()
    {
        ip = GUI.TextField(new Rect(10, 10, 100, 20), ip, 25);

        if (GUI.Button(new Rect(10, 35, 100, 20), "Connect"))
            Init();

        if (GUI.Button(new Rect(10, 60, 100, 20), "Disconnect"))
        {
            world.Clear();
            connected = false;
            socket.Close();
        }

        var s = new GUIStyle();
        s.fontSize = 20;
        s.fontStyle = FontStyle.Bold;
        s.normal.textColor = connected ? Color.green : Color.red;
        GUI.Label(new Rect(Screen.width / 2, 10, 100, 20), connected ? "Online" : "Offline", s);
    }

    void Init()
    {
        // Don't connect more than once!
        if (connected)
            return;

        Debug.Log("Connecting to the Server...");
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.BeginConnect(IPAddress.Parse(ip), port, new AsyncCallback(ConnectCallback), null);
    }

    void Update()
    {
        // Transfer transforms to custom struct, to access them from another thread
        hmdT.SetPosRot(hmd);

        controllerLeftT.SetPosRot(controllerLeft);
        controllerRightT.SetPosRot(controllerRight);

        if (trackerHip != null)
            trackerHipT.SetPosRot(trackerHip);
        else
            trackerHipT.SetPosRot(hmd);

        trackerLeftFootT.SetPosRot(trackerLeftFoot);
        trackerRightFootT.SetPosRot(trackerRightFoot);

        // Update the world with the latest received packets info
        world.Update();

        if (socket != null)
            connected = socket.Connected;
    }

    void OnApplicationQuit()
    {
        if (connected)
        {
            socket.Close();
            connected = false;
        }
    }

    // Callbacks
    private void ConnectCallback(IAsyncResult AR)
    {        
        socket.EndConnect(AR);
        connected = true;
        Debug.Log("Connected to the Server");

        // Start receiving from the server
        socket.BeginReceive(recvBuffer, 0, recvBuffer.Length, SocketFlags.None,
            new AsyncCallback(ReceiveCallback), null);

        SendSensorData();
    }

    private void ReceiveCallback(IAsyncResult AR)
    {
        int bytes_received = socket.EndReceive(AR);

        if (bytes_received == 0)
        {
            Debug.Log("Disconnected from the Server");
            connected = false;
            return;
        }

        // Handle received packet
        world.ClientUpdateState(recvBuffer, bytes_received);
        Thread.Sleep(1000 / packetsPerSecond);

        // Keep receiving
        socket.BeginReceive(recvBuffer, 0, recvBuffer.Length, SocketFlags.None,
            new AsyncCallback(ReceiveCallback), null);
    }

    private void SendCallback(IAsyncResult AR)
    {
        int bytes_sent = socket.EndSend(AR);
        Debug.Assert(bytes_sent == 6 * Trans.Size);

        Thread.Sleep(1000 / packetsPerSecond);

        SendSensorData();
    }

    // Sends the position and rotation of hmd, 2 controllers and 3 trackers 
    // from Client to Server Asynchronously
    private void SendSensorData()
    {
        List<byte> sendData = new List<byte>();

        WorldState.AddTransform(sendData, hmdT);
        WorldState.AddTransform(sendData, controllerLeftT);
        WorldState.AddTransform(sendData, controllerRightT);

        WorldState.AddTransform(sendData, trackerHipT);
        WorldState.AddTransform(sendData, trackerLeftFootT);
        WorldState.AddTransform(sendData, trackerRightFootT);

        socket.BeginSend(sendData.ToArray(), 0, sendData.Count, SocketFlags.None,
            new AsyncCallback(SendCallback), null);
    }
}
