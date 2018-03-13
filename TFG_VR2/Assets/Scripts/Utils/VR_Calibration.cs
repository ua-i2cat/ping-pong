// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class VR_Calibration : MonoBehaviour
{
    // Model transforms
    [SerializeField]
    protected Transform model;
    private Transform root;
    private Transform head;
    private Transform rightHand;
    private Transform leftHand;

    // CameraRig transforms
    [SerializeField]
    protected GameObject cameraRig;
    private Transform hmd;
    private Transform rightController;
    private Transform leftController;

    private SteamVR_TrackedObject rightTrack;
    private SteamVR_Controller.Device device;

    void Start ()
    {
        // The hmd is always the last child of the cameraRig object
        hmd = cameraRig.transform.GetChild(cameraRig.transform.childCount - 1);
        leftController = cameraRig.transform.GetChild(0);
        rightController = cameraRig.transform.GetChild(1);
        rightTrack = rightController.gameObject.GetComponent<SteamVR_TrackedObject>();

        //root = FindDeepChild(model, "Root");
        head = FindDeepChild(model, "Head");
        rightHand = FindDeepChild(model, "RightHand");
        leftHand = FindDeepChild(model, "LeftHand");
    }
	
	void Update ()
    {
        Debug.DrawLine(rightHand.position, rightController.position, Color.red);
        Debug.DrawLine(leftHand.position, leftController.position, Color.red);

        if (rightTrack.index > 0)
        {
            device = SteamVR_Controller.Input((int)rightTrack.index);

            if (device.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
            {
                // Scale model to match distance between controllers
                ScaleToMatchControllers();

                // Compute position in between the two controllers
                //Vector3 center = (rightController.position + leftController.position) / 2;
                Vector3 headToModel = model.position - head.position;

                // Set model's x position to that center
                model.position = new Vector3(
                    hmd.position.x, hmd.position.y + headToModel.y, hmd.position.z);                
            }

            if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            {
                // Start Controlling the Virtual Body
                StartTracking();
            }
        }

    }

    private void ScaleToMatchControllers()
    {
        float dist = Vector3.Distance(rightController.position, leftController.position);
        dist += 0.25f * dist;
        model.transform.localScale = new Vector3(dist / 2, dist / 2, dist / 2);
    }

    private bool tracking = false;

    private void StartTracking()
    {
        SwitchModelRender(hmd);
        SwitchModelRender(rightController);
        SwitchModelRender(leftController);

        if (!tracking)
        {
            tracking = true;

            //iksolver headIK = gameObject.AddComponent<iksolver>();
            //headIK.joints = new GameObject[2];
            //headIK.joints[0] = head.parent.gameObject;
            //headIK.joints[1] = head.gameObject;
            //headIK.target = hmd.gameObject;
            //headIK.Solve();

            IK_CCD2 rightIK = rightHand.gameObject.AddComponent<IK_CCD2>();
            rightIK.joints = new GameObject[3];
            rightIK.joints[0] = rightHand.parent.parent.gameObject;
            rightIK.joints[1] = rightHand.parent.gameObject;
            rightIK.joints[2] = rightHand.gameObject;
            rightIK.target = rightController.gameObject;

            IK_CCD2 leftIK = leftHand.gameObject.AddComponent<IK_CCD2>();
            leftIK.joints = new GameObject[3];
            leftIK.joints[0] = leftHand.parent.parent.gameObject;
            leftIK.joints[1] = leftHand.parent.gameObject;
            leftIK.joints[2] = leftHand.gameObject;
            leftIK.target = leftController.gameObject;

            rightIK.Solve();
            leftIK.Solve();
        }
    }

    private void SwitchModelRender(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.name == "Model")
                child.gameObject.SetActive(!child.gameObject.activeSelf);
        }
    }

    private Transform FindDeepChild(Transform parent, string name)
    {
        var result = parent.Find(name);
        if (result != null)
            return result;
        foreach (Transform child in parent)
        {
            result = FindDeepChild(child, name);
            if (result != null)
                return result;
        }
        return null;
    }
}
