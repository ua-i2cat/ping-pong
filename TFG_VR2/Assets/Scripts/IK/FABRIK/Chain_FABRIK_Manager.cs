// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class Chain_FABRIK_Manager : MonoBehaviour
{
    // Camera Rig components
    public Transform Rig;       // [CameraRig]
    public Transform HMD;       // > Camera (eye)
    public Transform LCtrl;     // > Controller (left)
    public Transform RCtrl;     // > Controller (right)

    // Model components
    public Transform ModelRoot;
    public Transform Head;
    public Transform LHand;
    public Transform RHand;
    public Transform EyesCenter;

    private SteamVR_TrackedController RInput;
    private SteamVR_Controller.Device InputDevice;
    private bool calibrated = false;
    private bool runIK = false;
    private bool pressDown = false;

    private Quaternion offsetQ;

    // Distance from the controller sensor to the base of the controller
    private const float CONTROLLER_OFFSET = 0.15f;

    void Start ()
    {
        RInput = RCtrl.GetComponent<SteamVR_TrackedController>();
    }
	
	void Update ()
    {
        InputDevice = new SteamVR_Controller.Device(RInput.controllerIndex);
        if (/*Input.GetKeyDown(KeyCode.C) ||*/ (InputDevice != null && InputDevice.GetHairTrigger()))
        {
            Calibrate();
            calibrated = true;

            offsetQ = Quaternion.Inverse(RHand.rotation) * RCtrl.rotation;
            offsetQ = Quaternion.Inverse(offsetQ);
        }

        if (/*Input.GetKeyDown(KeyCode.I) ||*/ (InputDevice != null && !pressDown && InputDevice.GetPressDown(EVRButtonId.k_EButton_SteamVR_Touchpad)))
        {
            pressDown = true;

            runIK = !runIK;
            IK(runIK);

            // if(!runIK) restore original joint config
        }

        if(!InputDevice.GetPressDown(EVRButtonId.k_EButton_SteamVR_Touchpad))
        {
            pressDown = false;
        }
    }

    void LateUpdate()
    {
        if(calibrated)
            RHand.rotation = RCtrl.rotation * offsetQ;
    }

    void Calibrate()
    {
        // We assume the user is in T pose
        Rig.rotation = Quaternion.Inverse(EyesCenter.rotation);

        // Rdist is the distance from the left wrist to the right wrist of the user
        float Rdist = Vector3.Distance(LCtrl.position, RCtrl.position) - 2 * CONTROLLER_OFFSET;

        // Vdist if the distance from the left wrist to the right wrist of the model
        float Vdist = Vector3.Distance(LHand.position, RHand.position);

        ModelRoot.transform.localScale *= Rdist / Vdist;

        // Move CameraRig to match the model eyes position
        Vector3 hmdToModelEye = EyesCenter.position - HMD.position;
        Rig.position += hmdToModelEye;
    }

    void IK(bool turnOn = true)
    {
        var ikArray = GetComponents<ChainIK>();

        foreach(var ik in ikArray)
        {
            ik.TurnIK(turnOn);
        }
    }
}
