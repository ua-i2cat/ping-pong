// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections.Generic;
using UnityEngine;

namespace AvatarSystem
{
    public class AvatarSensorsController : AvatarController
    {
        private Dictionary<string, Transform> transformsMap = new Dictionary<string, Transform>();

        private bool isClient;
        private int minimumSensors;

        public AvatarSensorsController(AvatarBody body, bool isClient, int minimumSensors)
        {
            this.type = AvatarControllerType.SENSORS;
            this.body = body;
            this.isClient = isClient;
            this.minimumSensors = minimumSensors;

            // Move the rig to match the eye of the body
        }

        public override List<Trans> GetTransforms()
        {
            List<Trans> transforms = new List<Trans>();

            foreach (var pair in transformsMap)
                transforms.Add(new Trans(pair.Value.position, pair.Value.rotation, pair.Key));

            return transforms;
        }

        public override void SetTransforms(List<Trans> transforms)
        {
            //Debug.Log("Body: " + body.transform.position);
            //Debug.Log("Spawn: " + transforms[0].Pos);
            //Debug.Log("SetTransforms");
            body.transform.position = transforms[0].Pos;
            body.transform.rotation = transforms[0].Rot;
            //throw new System.NotImplementedException();
        }

        public override void Update()
        {
            if (!initialized)
            {
                initialized = isClient ? InitSelf() : InitOponent();
            }

            if (initialized)
            {
                // Start the IK once everything is initialized
                this.body.SetIK(IKAction);
            }
        }

        private bool initialized = false;
        private bool InitSelf()
        {
            var rig = Object.FindObjectOfType<SteamVR_ControllerManager>();
            var cam = Object.FindObjectOfType<SteamVR_Camera>();
            var sensors = Object.FindObjectsOfType<SteamVR_TrackedObject>();

            // If the sensors are not valid or not found, the initialization failed
            // The camera is a sensor but not of type SteamVR_TrackedObject
            if (sensors.Length >= minimumSensors - 1)
            {
                foreach (var s in sensors)
                {
                    if (!s.isValid)
                    {
                        Debug.Log(s.name + " is invalid");
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
            

            Debug.Log(rig.name + " " + rig.transform.position);
            Debug.Log(cam.name + " " + cam.transform.position);
            foreach (var s in sensors)
            {
                Debug.Log(s.name + " " + s.index + " " + s.transform.position);
            }
            
            transformsMap.Add(Constants.Rig, rig.transform);
            transformsMap.Add(Constants.Eye, cam.transform);
            foreach (var s in sensors)
            {
                if (s.transform.Find("attach") != null)
                    transformsMap.Add(s.name, s.transform.Find("attach"));
                else
                    transformsMap.Add(s.name, s.transform);
            }
            
            // Move the rig to match the camera position with the eyes of the model
            var bodyEye = this.body.transform.Find(Constants.Eye).position;
            Vector3 offset = rig.transform.position - cam.transform.position;
            rig.transform.position = bodyEye + offset;

            Debug.Log("All sensors found");
            return true;
        }

        private bool InitOponent()
        {
            var rig = this.body.transform.parent.Find(Constants.Rig);

            if(rig == null || rig.childCount < 1)
            {
                return false;
            }

            transformsMap.Add(Constants.Rig, rig);

            foreach(Transform child in rig)
            {
                transformsMap.Add(child.name, child);
            }

            return true;
        }

        private void IKAction(Animator animator)
        {
            if (transformsMap.Keys.Count < 1)
                return;

            animator.SetLookAtWeight(1);
            Vector3 lookAt = transformsMap[Constants.Eye].position + 
                transformsMap[Constants.Eye].forward;
            animator.SetLookAtPosition(lookAt);

            // Use hip as the root
            if(transformsMap.ContainsKey(Constants.Hip) && transformsMap[Constants.Hip].localPosition != Vector3.zero)
            {
                Transform hip = transformsMap[Constants.Hip];
                body.transform.rotation = Quaternion.Euler(new Vector3(0, hip.eulerAngles.y, 0));
                body.transform.position = new Vector3(hip.position.x, body.transform.position.y, 
                    hip.position.z) - new Vector3(0, 0, 0.3f);
            }
            // Use the headset to move and rotate the model
            else if(transformsMap.ContainsKey(Constants.Eye))
            {
                Transform cam = transformsMap[Constants.Eye];

                // Rotate the body in the same direction as the Camera
                body.transform.rotation = Quaternion.Euler(new Vector3(0, cam.eulerAngles.y, 0));

                Vector3 offset = body.transform.Find(Constants.Eye).position - body.transform.position;
                Vector3 camPos = cam.position;
                body.transform.position = new Vector3(camPos.x - offset.x, body.transform.position.y, 
                    camPos.z - offset.z);
            }

            if (transformsMap.ContainsKey(Constants.LeftHand))
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                animator.SetIKPosition(AvatarIKGoal.LeftHand,
                    transformsMap[Constants.LeftHand].position);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                animator.SetIKRotation(AvatarIKGoal.LeftHand,
                    transformsMap[Constants.LeftHand].rotation);
            }

            if(transformsMap.ContainsKey(Constants.RightHand))
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                animator.SetIKPosition(AvatarIKGoal.RightHand,
                    transformsMap[Constants.RightHand].position);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                animator.SetIKRotation(AvatarIKGoal.RightHand,
                    transformsMap[Constants.RightHand].rotation);
            }

            if (transformsMap.ContainsKey(Constants.LeftFoot) && transformsMap[Constants.LeftFoot].localPosition != Vector3.zero)
            {
                Debug.Log("contains LF");
                animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
                animator.SetIKPosition(AvatarIKGoal.LeftFoot,
                    transformsMap[Constants.LeftFoot].position);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
                animator.SetIKRotation(AvatarIKGoal.LeftFoot,
                    transformsMap[Constants.LeftFoot].rotation);
            }

            if (transformsMap.ContainsKey(Constants.RightFoot) && transformsMap[Constants.RightFoot].localPosition != Vector3.zero)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
                animator.SetIKPosition(AvatarIKGoal.RightFoot,
                    transformsMap[Constants.RightFoot].position);
                animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);
                animator.SetIKRotation(AvatarIKGoal.RightFoot,
                    transformsMap[Constants.RightFoot].rotation);
            }
        }
    }
}