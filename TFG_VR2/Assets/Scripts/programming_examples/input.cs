// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class input : MonoBehaviour
{
    private SteamVR_TrackedObject trackedObject;
    private SteamVR_Controller.Device device;

    // Use this for initialization
    void Start ()
    {
        //trackedObject = GetComponent<SteamVR_TrackedObject>();

    }
	
	// Update is called once per frame
	void Update ()
    {
        /*device = SteamVR_Controller.Input((int)trackedObject.index);
        //Debug.Log((int)trackedObject.index);
        if(SteamVR_Controller.Input((int)trackedObject.index).GetHairTriggerDown())
        {
            Debug.Log("Trigger");
        }

        if (device.GetAxis().x != 0 || device.GetAxis().y != 0)
        {
            Debug.Log(device.GetAxis().x + " " + device.GetAxis().y);
        }*/
    }
}
