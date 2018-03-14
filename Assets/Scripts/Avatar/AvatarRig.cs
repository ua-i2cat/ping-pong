using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class AvatarRig
{
    protected Dictionary<string, Transform> transforms = new Dictionary<string, Transform>();

    public int GetTransformCount()
    {
        return transforms.Count;
    }

    public KeyValuePair<string, Transform> GetTransform(string name)
    {
        if (transforms.ContainsKey(name))
            return new KeyValuePair<string, Transform>(name, transforms[name]);
        else
            return new KeyValuePair<string, Transform>(null, null);
    }

    public KeyValuePair<string, Transform> GetTransform(int index)
    {
        return transforms.ElementAt(index);
    }

    public Transform GetRigEye()
    {
        return GetTransform(Constants.Eye).Value;
    }
}

public class AvatarRigVR : AvatarRig
{
    public AvatarRigVR(AvatarBody body)
    {
        try
        {
            GameObject rig = GameObject.Find("[CameraRig]");

            transforms.Add(Constants.Eye, rig.transform.Find("Camera (eye)"));
            transforms.Add(Constants.LeftHand, rig.transform.Find("Controller (left)").Find("attach"));
            transforms.Add(Constants.RightHand, rig.transform.Find("Controller (right)").Find("attach"));
            transforms.Add(Constants.LeftFoot, rig.transform.Find("Foot (left)"));
            transforms.Add(Constants.RightFoot, rig.transform.Find("Foot (right)"));

            // Move the rig so that it matches the INITIAL position of the body
            rig.transform.position += body.GetBodyEye().position - GetRigEye().position;
        }
        catch
        {
            Debug.Assert(false, "Invalid Rig for VR");
        }
    }
}

public class AvatarRigCam : AvatarRig
{
    public AvatarRigCam(AvatarBody body)
    {
        GameObject cam = GameObject.FindGameObjectWithTag("MainCamera");
        Debug.Assert(cam != null && !UnityEngine.XR.XRSettings.enabled,
            "Incompatible camera configuration!");

        transforms.Add(Constants.Eye, cam.transform);

        cam.transform.position = body.GetBodyEye().position;
        cam.transform.rotation = body.GetBodyEye().rotation;
        cam.transform.parent = body.transform;
    }
}
