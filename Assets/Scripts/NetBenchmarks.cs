// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;

public class NetBenchmarks
{
    public long sendTimeStamp;
    public long recvTimeStamp;

    public NetBenchmarks()
    {
        this.sendTimeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
        this.recvTimeStamp = -1;
    }

    public NetBenchmarks(long send, long recv)
    {
        this.sendTimeStamp = send;
        this.recvTimeStamp = recv;
    }

    public static int Size { get { return 2 * sizeof(long); } }
}
