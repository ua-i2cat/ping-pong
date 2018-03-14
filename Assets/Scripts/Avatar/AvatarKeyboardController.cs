// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using UnityEngine;

// Controlls an avatar using the keyboard
public class AvatarKeyboardController : AvatarController
{
    private float speed = 3.0f;
    private float angularSpeed = 100.0f;

    public AvatarKeyboardController(AvatarBody body) : base(body)
    {
        rig = new AvatarRigCam(body);
    }

    public override void Update()
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
            body.Translate(new Vector3(0, -speed * Time.deltaTime, 0));
        }
        if (Input.GetKey(KeyCode.Q))
        {
            body.Translate(new Vector3(0, speed * Time.deltaTime, 0));
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
