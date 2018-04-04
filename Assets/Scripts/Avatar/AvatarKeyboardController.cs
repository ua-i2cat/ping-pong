// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AvatarSystem
{
    public class AvatarKeyboardController : AvatarController
    {
        private float speed = 3.0f;
        private float angularSpeed = 100.0f;

        private Canvas canvas;

        public AvatarKeyboardController(AvatarBody body)
        {
            this.type = AvatarControllerType.KEYBOARD;
            this.body = body;
            canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        }

        public override List<Trans> GetTransforms()
        {
            List<Trans> transforms = new List<Trans>();

            Transform t = this.body.transform;
            transforms.Add(new Trans(t.position, t.rotation, Constants.Body));

            return transforms;
        }

        public override void SetTransforms(List<Trans> transforms)
        {
            this.body.transform.position = transforms[0].Pos;
            this.body.transform.rotation = transforms[0].Rot;

            //var bodyTransforms = this.GetTransforms();

            //foreach(var bodyT in bodyTransforms)
            //{
            //    foreach(var t in transforms)
            //    {
            //        if(t.Id.Contains(bodyT.Id) || bodyT.Id.Contains(t.Id))
            //        {
            //            Transform trans = this.body.transform.Find(t.Id);
            //            trans.position = t.Pos;
            //            trans.rotation = t.Rot;
            //        }
            //    }
            //}
        }

        public override void Update()
        {
            if (!canvas.enabled && !body.isColliding)
            {
                // Translate
                if (Input.GetKey(KeyCode.D))
                {
                    body.Translate(new Vector3(speed * Time.deltaTime, 0, 0));
                }
                if (Input.GetKey(KeyCode.A))
                {
                    body.Translate(new Vector3(-speed * Time.deltaTime, 0, 0));
                }
                if (Input.GetKey(KeyCode.E))
                {
                    //body.Translate(new Vector3(0, -speed * Time.deltaTime, 0));
                }
                if (Input.GetKey(KeyCode.Q))
                {
                    //body.Translate(new Vector3(0, speed * Time.deltaTime, 0));
                }
                if (Input.GetKey(KeyCode.W))
                {
                    body.Translate(new Vector3(0, 0, speed * Time.deltaTime));
                }
                if (Input.GetKey(KeyCode.S))
                {
                    body.Translate(new Vector3(0, 0, -speed * Time.deltaTime));
                }

                // Rotate
                if (Input.GetKey(KeyCode.RightArrow))
                {
                    body.Rotate(new Vector3(0, angularSpeed * Time.deltaTime, 0));
                }
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    body.Rotate(new Vector3(0, -angularSpeed * Time.deltaTime, 0));
                }
            }
        }
    }
}