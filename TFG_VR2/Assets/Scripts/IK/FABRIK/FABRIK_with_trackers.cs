// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FABRIK_with_trackers : MonoBehaviour
{
    public Transform model;
    public Transform Head;

    public Transform LHand;
    public Transform RHand;

    public Transform Rig;
    public Transform Eye;
    public Transform LCtrl;   //Hands
    public Transform RCtrl;
    public Transform LTrack;  //Feet
    public Transform RTrack;

    public bool HeadTrackingEnabled = false;
    public bool HandsTrackingEnabled = false;
    public bool FeetTrackingEnabled = false;

    private SteamVR_TrackedController RInput;

    // Distance from the controller sensor to the base of the controller
    const float CONTROLLER_OFFSET = 0.15f;

    private Vector3 headToRoot;
    private bool calibrated = false;

    // Use this for initialization
    void Start ()
    {
        RInput = RCtrl.GetComponent<SteamVR_TrackedController>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.C) || (RInput != null && RInput.triggerPressed))
        {
            Calibrate();
            calibrated = true;
        }

        if(Input.GetKeyDown(KeyCode.I) || (RInput != null && RInput.padPressed))
        {
            IK();
        }
    }

    void LateUpdate()
    {
        //Debug.DrawLine(Eye.position, Head.position);
        //Debug.DrawLine(Head.position, RCtrl.position);
        //Debug.DrawLine(Head.position, LCtrl.position);
        //Debug.DrawLine(LCtrl.position, RCtrl.position);

        Vector3 HeadToHMD = Eye.position - Head.position;
        

        if(Mathf.Abs(HeadToHMD.x) > 0.1f && calibrated)
        {
            //Debug.Log(Mathf.Abs(HeadToHMD.x));
            //model.position = new Vector3(Head.position.x, model.position.y, model.position.z);
            model.position = Vector3.Lerp(model.position, new Vector3(Head.position.x, model.position.y, model.position.z), Time.deltaTime);
        }
    }

    void IK()
    {
        IK_Manager H_IK = Eye.GetComponent<IK_Manager>();
        IK_Manager RH_IK = RCtrl.GetComponent<IK_Manager>();
        IK_Manager LH_IK = LCtrl.GetComponent<IK_Manager>();
        IK_Manager RF_IK = RTrack.GetComponent<IK_Manager>();
        IK_Manager LF_IK = LTrack.GetComponent<IK_Manager>();

        if(H_IK != null && HeadTrackingEnabled)
            H_IK.enabled = true;

        if (HandsTrackingEnabled)
        {
            RH_IK.enabled = true;
            LH_IK.enabled = true;
        }

        if (FeetTrackingEnabled)
        {
            RF_IK.enabled = true;
            LF_IK.enabled = true;
        }
    }

    void Calibrate()
    {
        // We assume the user is in seated T pose
        Rig.rotation = Quaternion.Inverse(model.rotation);

        // Rdist is the distance from the left wrist to the right wrist of the user
        float Rdist = Vector3.Distance(LCtrl.position, RCtrl.position) - 2*CONTROLLER_OFFSET;

        // Vdist if the distance from the left wrist to the right wrist of the model
        float Vdist = Vector3.Distance(LHand.position, RHand.position);

        model.transform.localScale *= Rdist / Vdist;
    
        // Move CameraRig to match the model eyes position
        Vector3 camToRig = Rig.position - Eye.position;
        Rig.position = Head.position + camToRig;
        

        //headToRoot = model.transform.position - Head.position;
    }
}
