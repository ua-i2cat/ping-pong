// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class NegateTracking : MonoBehaviour
{
    //private Transform initial;
    //public Transform headMarker;
    //   private GameObject head;
    //   private GameObject neck;


    [SerializeField]
    protected GameObject root;

    [SerializeField]
    protected GameObject head;

    bool calibrated = false;


    Vector3 offset;
    Quaternion offsetQ;

    // Use this for initialization
    void Start ()
    {
        Debug.Log(head.transform.right);
        Debug.Log(head.transform.up);
        Debug.Log(head.transform.forward);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("Calibrating");

            offsetQ = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.Head) * Quaternion.Inverse(
                head.transform.rotation * 
                Quaternion.AngleAxis(90, Vector3.right) * // To remove weird rotation from the model head
                Quaternion.AngleAxis(-90, Vector3.forward));

            // Distance so we can't see inside the head
            float forwardDist = -0.25f;
            offset = root.transform.position - (head.transform.position + new Vector3(0, 0, forwardDist));
            calibrated = true;
        }

        if (calibrated)
        {
            head.transform.rotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.Head) * offsetQ;
            root.transform.position = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.Head) + offset;
        }
    }

    string getVector(Vector3 v)
    {
        return v.x.ToString() + " " + v.y.ToString() + " " + v.z.ToString();
    }
}
