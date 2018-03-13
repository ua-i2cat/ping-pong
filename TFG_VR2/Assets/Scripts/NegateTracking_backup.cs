// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class NegateTracking_backup : MonoBehaviour
{
    //private Transform initial;
    public Transform headMarker;
    private GameObject head;
    private GameObject neck;

	// Use this for initialization
	void Start ()
    {       
        Debug.Log(this.name + " world position: " + getVector(transform.position));
        Debug.Log(this.name + " local position: " + getVector(transform.localPosition));

        headMarker.position = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.Head);
        headMarker.rotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.Head);
        Debug.Log(getVector(headMarker.position));

        Debug.Log("head world position: " + getVector(headMarker.position));
        Debug.Log("head local position: " + getVector(headMarker.localPosition));

        // Update the head of the model with the data from the HMD
        head = GameObject.Find("Head");
        //head.transform.position = headMarker.position;
        //head.transform.rotation = headMarker.rotation;

        // Recursively update the transform of the other joints
        //GameObject neckMarker = GameObject.Find("NeckMarker");

        neck = head.transform.parent.gameObject;
        Debug.Log(neck.name + " world position: " + getVector(neck.transform.position));
        Debug.Log(neck.name + " local position: " + getVector(neck.transform.localPosition));

    }

    // Update is called once per frame
    void Update()
    {
        headMarker.position = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.Head);
        headMarker.rotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.Head);
        //head.transform.position = headMarker.position;
        //head.transform.rotation = headMarker.rotation;

        //Debug.Log(getVector(InputTracking.GetLocalPosition(VRNode.Head)));
        /*Debug.Log(InputTracking.GetLocalPosition(VRNode.RightHand));
        Debug.Log(initial);
        transform.position = initial.position + InputTracking.GetLocalPosition(VRNode.RightHand);
        transform.rotation = initial.rotation * InputTracking.GetLocalRotation(VRNode.RightHand);

        Debug.DrawLine(transform.position, transform.parent.transform.position);*/
    }

    string getVector(Vector3 v)
    {
        return v.x.ToString() + " " + v.y.ToString() + " " + v.z.ToString();
    }
}
