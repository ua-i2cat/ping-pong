using System.Collections.Generic;
using System.Linq;
using System.Net;

class Connection
{
    public EndPoint endPoint;
    public int ttl; // time to live: -1 means that the connection lasts forever, 0 means that the connection is invalid

    public ClientDataUDP clientData;

    public Connection(EndPoint endPoint, int ttl)
    {
        this.endPoint = endPoint;
        this.ttl = ttl;
        this.clientData = new ClientDataUDP((IPEndPoint)endPoint);
    }
}

class ConnectionManager
{
    private int ttl; // Default time to live for new connections

    public ConnectionManager(int ttl = -1)
    {
        this.ttl = ttl;
    }

    public List<Connection> Connections { get; } = new List<Connection>();

    public Connection AddOrUpdateConnection(EndPoint endPoint)
    {
        Connection c = Connections.Where(x => x.endPoint.Equals(endPoint)).FirstOrDefault();
        if (c != null)
        {
            c.ttl = ttl;
        }           
        else
        {
            c = new Connection(endPoint, ttl);
            Connections.Add(c);
        }
        return c;   
    }

    public void Tick()
    {
        for(int i = Connections.Count - 1; i >= 0; i--)
        {
            if (Connections[i].ttl > 0)
            {
                --Connections[i].ttl;
            }

            if (Connections[i].ttl == 0)
            {
                Connections[i].clientData.Destroy();
                Connections.Remove(Connections[i]);
            }
        }
    }

    public bool Contains(EndPoint endPoint)
    {
        return Connections.Where(x => x.endPoint.Equals(endPoint)).FirstOrDefault() != null;
    }

    public int Count { get { return Connections.Count; } }
}
