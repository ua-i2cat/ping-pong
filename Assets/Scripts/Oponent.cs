// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Oponents
{
    private System.Object listLock = new System.Object();
    private List<Oponent> oponents = new List<Oponent>();

    // Adds an oponent to the list of oponents if it doesn't already 
    // exist one with the specified id and returns the Oponent object
    public Oponent AddOponent(int id)
    {
        lock (listLock)
        {
            Oponent o = oponents.FirstOrDefault<Oponent>(x => x.Id == id);
            if (o == null)
            {
                o = new Oponent(id);
                oponents.Add(o);
            }
            o.TTL = 100;
            return o;
        }
    }

    // Removed an oponent from the list by id if it exists
    public void RemoveOponent(int id)
    {
        lock(listLock)
        {
            Oponent o = oponents.FirstOrDefault<Oponent>(x => x.Id == id);
            if(o != null)
            {
                oponents.Remove(o);
            }
        }
    }

    public int Count
    {
        get { lock (listLock) { return oponents.Count; } }
    }

    public Oponent GetOponent(int index)
    {
        lock (listLock)
        {
            return oponents[index];
        }
    }
}

public class Oponent
{
    private System.Object oponentLock = new System.Object();
    private List<Trans> transforms = new List<Trans>();

    public int TTL = 100;

    public Oponent(int id)
    {
        this.Id = id;
    }

    // Adds a transform, only if it doesn't already exist one with the same id.
    // If it already exists, the position and rotation are updated
    public void AddTransform(Trans t)
    {
        lock (oponentLock)
        {
            Trans transform = transforms.FirstOrDefault<Trans>(x => x.Id == t.Id);
            if (transform == null)
            {
                transforms.Add(t);
            }
            else
            {
                transform.Pos = t.Pos;
                transform.Rot = t.Rot;
            }
        }
    }

    public int Id { get; }
    public int TransCount { get { lock (oponentLock) { return transforms.Count; } } }
    public Trans GetTrans(int index)
    {
        lock (oponentLock)
        {
            return transforms[index];
        }
    }
}