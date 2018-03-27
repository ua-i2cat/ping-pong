using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class NetBenchmarks
{
    public long sendTimeStamp;
    public long recvTimeStamp;

    public NetBenchmarks()
    {
        this.sendTimeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        this.recvTimeStamp = -1;
    }

    public NetBenchmarks(long send, long recv)
    {
        this.sendTimeStamp = send;
        this.recvTimeStamp = recv;
    }

    public static int Size { get { return 2 * sizeof(long); } }
}
