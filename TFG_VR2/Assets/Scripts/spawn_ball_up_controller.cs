// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawn_ball_up_controller : MonoBehaviour {

    public GameObject prefab;
    public float height_spawn=10;
    private SteamVR_TrackedObject trackedObj;


    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }
    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    void Update () {
       
        if (Controller.GetHairTriggerDown() )
        {
         
                Instantiate(prefab, transform.position +=Vector3.up*Time.deltaTime* height_spawn, transform.rotation);
        }
    }

}