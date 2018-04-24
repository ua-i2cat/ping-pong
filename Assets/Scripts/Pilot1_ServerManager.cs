// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Text;
using UnityEngine;

public class Pilot1_ServerManager : MonoBehaviour
{
    private Server server = new ServerTCP();
    private int port = 44444;

    //private SessionManager sessionManager = new SessionManager();
    //private StreamingManager streamingManager = new StreamingManager();
    //private ConnectionManager connectionManager = new ConnectionManager();
    //private Orchestrator orchestrator = new Orchestrator();

    private void Awake()
    {
        server.OnRecv += OnMsgRecv;
        server.Start(port);
    }

    private void OnMsgRecv(object s, Server.ServerMsgEventArgs e)
    {
        Debug.Log("[" + e.Client.ToString() + "]: " + Encoding.ASCII.GetString(e.Buffer, 0, e.Len));

        // Add client to connectionManager
        //connectionManager.AddConnection(e.Client);

        //server.Send(e.Client, e.Buffer, e.Len);
    }

    private void Update()
    {
        // Decrease KeepAlive and remove clients that have been inactive for too long
        //connectionManager.Update();
    }

    private void OnApplicationQuit()
    {
        server.Stop();
        server.OnRecv -= OnMsgRecv;
    }
}
