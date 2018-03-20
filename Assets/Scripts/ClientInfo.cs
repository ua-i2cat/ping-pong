// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientInfo
{
    private int id = 0;
    private List<Trans> transforms = new List<Trans>();

    public List<byte> Serialize()
    {
        List<byte> data = new List<byte>();
        data.AddRange(BitConverter.GetBytes(id));   // Client id
        data.Add((byte)transforms.Count);           // Transform Count
        foreach (var transform in transforms)
            data.AddRange(transform.Serialize());
        return data;
    }

    public static ClientInfo Deserialize(List<byte> data)
    {
        throw new NotImplementedException();
    }
}
