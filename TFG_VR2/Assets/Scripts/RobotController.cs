// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;
using System.Linq;

public class RobotController : MonoBehaviour
{
    [SerializeField]
    protected GameObject head;

    [SerializeField]
    protected GameObject rightHand;

    [SerializeField]
    protected GameObject rightHandTarget;

    [SerializeField]
    protected GameObject leftHand;

    [SerializeField]
    protected GameObject leftHandTarget;

    [SerializeField]
    protected GameObject root;

    [SerializeField]
    protected GameObject cam;

    private bool calibrated = false;

    private Vector3 headToRootOffset;
    private Quaternion headToRootQ;

    private Vector3 hmdToRControllerOffset;
    private Vector3 hmdToLControllerOffset;

    private Vector3 headToRHandOffset;
    private Vector3 headToLHandOffset;
    private Quaternion headToRHandQ;
    private Quaternion headToLHandQ;

    [SerializeField]
    protected Transform rightController;
    private SteamVR_Controller.Device rightInput;

    [SerializeField]
    protected Transform leftController;
    private SteamVR_Controller.Device leftInput;

    [SerializeField]
    protected float forwardDist = -0.25f; // Distance so we can't see inside the head

    // Use this for initialization
    void Start ()
    {
        rightInput = SteamVR_Controller.Input((int)5);
        //leftInput = SteamVR_Controller.Input((int)1);
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (rightInput.GetHairTriggerDown() || Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("Calibrating");

            //headToRootOffset = root.transform.position - (head.transform.position);// + new Vector3(0, 0, forwardDist));
            headToRootQ = Quaternion.Inverse(cam.transform.rotation) * head.transform.rotation;
            calibrated = true;

            hmdToRControllerOffset = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.RightHand)/* - InputTracking.GetLocalPosition(VRNode.Head)*/;
            headToRHandOffset = rightHand.transform.position - head.transform.position;

            hmdToLControllerOffset = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.LeftHand) - UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.Head);
            headToLHandOffset = leftHand.transform.position - head.transform.position;

            headToRHandQ = Quaternion.Inverse(UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.RightHand)) * rightHand.transform.rotation;
            headToLHandQ = Quaternion.Inverse(UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.LeftHand)) * leftHand.transform.rotation;

            Debug.Log("hmdToRControlerOffset: " + hmdToRControllerOffset);
            Debug.Log("headToRHandOffset: " + headToRHandOffset);

            //root.transform.position = InputTracking.GetLocalPosition(VRNode.Head) + headToRootOffset;

            //headSolver.enabled = true;
            //head.transform.position = InputTracking.GetLocalPosition(VRNode.Head);
        }

        if (calibrated)
        {
            head.transform.rotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.Head) * headToRootQ;
            //head.transform.position = InputTracking.GetLocalPosition(VRNode.Head) + new Vector3(0, 0, forwardDist);

            //rightHandTarget.transform.position = rightHand.transform.position;

            Vector3 controllerRightPos = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.RightHand);
            Vector3 controllerLeftPos = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.LeftHand);
            //Vector3 hmdPos = InputTracking.GetLocalPosition(VRNode.Head);

            rightHandTarget.transform.position = new Vector3(controllerRightPos.x * (headToRHandOffset.x/ hmdToRControllerOffset.x), controllerRightPos.y, controllerRightPos.z);
            rightHand.transform.rotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.RightHand) * headToRHandQ;

            leftHandTarget.transform.position = new Vector3(controllerLeftPos.x * (headToLHandOffset.x / hmdToLControllerOffset.x), controllerLeftPos.y, controllerLeftPos.z);
            leftHand.transform.rotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.LeftHand) * headToLHandQ;
        }
    }
}
