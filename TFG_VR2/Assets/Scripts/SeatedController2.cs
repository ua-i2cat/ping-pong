// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;
using System.Linq;

public class SeatedController2 : MonoBehaviour
{
    [SerializeField]
    private Transform rigTransform;

    [SerializeField]
    private Transform eyeTransform;

    [SerializeField]
    private Transform modelEyesTransform;

    [SerializeField]
    private Transform model;

    [SerializeField]
    private GameObject headTarget;

    [SerializeField]
    private GameObject lhTarget;

    [SerializeField]
    private GameObject rhTarget;

    private IK_CCD3 headIK;
    private IK_CCD3 lhIK;
    private IK_CCD3 rhIK;

    private bool startedIK = false;

    private Quaternion Qrig;
    private Quaternion Qcam;
    private Quaternion Qmodel;

    void Start()
    {
        headIK = headTarget.GetComponent<IK_CCD3>();
        lhIK = lhTarget.GetComponent<IK_CCD3>();
        rhIK = rhTarget.GetComponent<IK_CCD3>();

        Qrig = rigTransform.rotation;
        Qcam = eyeTransform.rotation;
        Qmodel = model.rotation;
    }

    void Update ()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            Calibrate();           
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            startedIK = true;
            headIK.ready = true;
        }

        if(startedIK)
        {
            IK();
        }
    }

    void Calibrate()
    {
        // 1.3 Scale the model to match the dimensions of the user
        float dist = Vector3.Distance(rigTransform.GetChild(0).position, rigTransform.GetChild(1).position);
        dist += 0.25f * dist;
        //model.transform.localScale = new Vector3(dist / 2, dist / 2, dist / 2);

        // Match model orientation
        Qrig = rigTransform.rotation;
        Qcam = eyeTransform.rotation;
        Qmodel = model.rotation;

        // Qcam = Qrig * Qrot
        // Qrot = inv(Qrig) * Qcam
        Quaternion Qrot = Quaternion.Inverse(Qrig) * Qcam;

        // We want Qcam = Qmodel
        // Qrig = Qcam * inv(Qrot) = Qmodel * inv(Qrot)
        Qrig = Qmodel * Quaternion.Inverse(Qrot);
        rigTransform.rotation = Qrig * Quaternion.Euler(0, 180, 0);

        // Move CameraRig (1.1 cam + 1.2 controllers) to match the model eyes position
        Vector3 camToRig = rigTransform.position - eyeTransform.position;
        rigTransform.position = modelEyesTransform.position + camToRig;       
    }

    void IK()
    {
        if(headIK.done)
        {
            lhIK.ready = true;
            rhIK.ready = true;
        }
    }
}
