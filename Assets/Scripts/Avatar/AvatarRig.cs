using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AvatarRig
{
    //private Transform Root;
    protected Transform Eye;
    protected Transform LH;
    protected Transform RH;
    protected Transform Hip;
    //private Transform LF;
    //private Transform RF;

    public Transform GetRigEye()
    {
        return Eye;
    }

    public KeyValuePair<bool, Transform> GetHip()
    {
        return new KeyValuePair<bool, Transform>(Hip != null, Hip);
    }
    public KeyValuePair<bool, Transform> GetLH()
    {
        return new KeyValuePair<bool, Transform>(LH != null, LH);
    }
    public KeyValuePair<bool, Transform> GetRH()
    {
        return new KeyValuePair<bool, Transform>(RH != null, RH);
    }
}

public class AvatarRigVR : AvatarRig
{
    public AvatarRigVR(AvatarBody body)
    {
        GameObject rig = GameObject.Find("[CameraRig]");

        Eye = rig.transform.Find("Camera (eye)");
        LH = rig.transform.Find("Controller (left)").Find("attach");
        RH = rig.transform.Find("Controller (right)").Find("attach");

        // Move the rig so that it matches the INITIAL position of the body
        rig.transform.position += body.GetBodyEye().position - GetRigEye().position;
    }
}

public class AvatarRigCam : AvatarRig
{
    public AvatarRigCam(AvatarBody body)
    {
        GameObject cam = GameObject.FindGameObjectWithTag("MainCamera");
        Debug.Assert(cam != null && !UnityEngine.XR.XRSettings.enabled,
            "Incompatible camera configuration!");

        Eye = cam.transform;

        cam.transform.position = body.GetBodyEye().position;
        cam.transform.rotation = body.GetBodyEye().rotation;
        cam.transform.parent = body.transform;
    }
}
