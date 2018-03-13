using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarRig
{
    //private Transform Root;
    //private Transform Head;
    //private Transform LH;
    //private Transform RH;
    //private Transform Hip;
    //private Transform LF;
    //private Transform RF;

    public AvatarRig(AvatarBody body)
    {
        // NO VR
        //GameObject cam = GameObject.FindGameObjectWithTag("MainCamera");
        //Debug.Assert(cam != null && cam.transform.parent == null && !UnityEngine.XR.XRSettings.enabled,
        //    "Incompatible camera configuration!");
        //cam.transform.position = body.GetEyeTransform().position;
        //cam.transform.rotation = body.GetEyeTransform().rotation;
        //cam.transform.parent = body.transform;

        // VR
        GameObject camRig = GameObject.Find("[CameraRig]");
        Debug.Log("Rig: " + camRig.transform.position);
        Transform eye = camRig.transform.Find("Camera (eye)");
        Debug.Log("Eye: " + eye.position);

        body.transform.rotation = Quaternion.Euler(new Vector3(0, eye.eulerAngles.y, 0));
        camRig.transform.position += body.GetEyeTransform().position - eye.position;
    }
}
