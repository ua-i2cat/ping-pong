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

    public abstract void Update();

    public List<Trans> GetTransforms()
    {
        List<Trans> t = new List<Trans>();
        for(int i = 0; i < this.GetTransformCount(); i++)
        {
            var pair = this.GetTransform(i);
            t.Add(new Trans(pair.Value.position, pair.Value.rotation, pair.Key));
        }
        return t;
    }
}

public class AvatarRigVR : AvatarRig
{
    private Transform rig;
    private Transform bodyEye;

    public AvatarRigVR(AvatarBody body)
    {
        try
        {
            rig = GameObject.Find("[CameraRig]").transform;
            bodyEye = body.GetBodyEye();

            transforms.Add(Constants.Eye, rig.Find("Camera (eye)"));
            transforms.Add(Constants.LeftHand, rig.Find("Controller (left)").Find("attach"));
            transforms.Add(Constants.RightHand, rig.Find("Controller (right)").Find("attach"));
            transforms.Add(Constants.LeftFoot, rig.Find("Foot (left)"));
            transforms.Add(Constants.RightFoot, rig.Find("Foot (right)"));

            // Move the rig so that it matches the INITIAL position of the body
            rig.position += bodyEye.position - bodyEye.position;
        }
        catch
        {
            Debug.Assert(false, "Invalid Rig for VR");
        }
    }

    public override void Update()
    {
        // Move the rig so that it matches the position of the body
        rig.transform.position += bodyEye.position - bodyEye.position;
    }
}

public class AvatarRigCam : AvatarRig
{
    private Transform rig;
    private Transform bodyEye;

    public AvatarRigCam(AvatarBody body)
    {
        rig = GameObject.FindGameObjectWithTag("MainCamera").transform;
        bodyEye = body.GetBodyEye();
        Debug.Assert(rig != null && !UnityEngine.XR.XRSettings.enabled,
            "Incompatible camera configuration!");

        transforms.Add(Constants.Eye, rig);

        rig.position = bodyEye.position;
        rig.rotation = bodyEye.rotation;
        rig.parent = body.transform;
    }

    public override void Update()
    {
        rig.position = bodyEye.position;
        rig.rotation = bodyEye.rotation;
    }
}
