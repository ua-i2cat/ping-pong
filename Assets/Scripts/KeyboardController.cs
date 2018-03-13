// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using UnityEngine;

public class KeyboardController : MonoBehaviour
{
    public float speed = 3.0f;
    public float angularSpeed = 100.0f;

    private Transform Rig;
    //private Transform Head;
    //private Transform LHand;
    //private Transform RHand;
    //private Transform Hip;
    //private Transform LFoot;
    //private Transform RFoot;

    private void Start()
    {
        Rig = this.transform.parent;
        //Head = Rig.Find("Head");
        //LHand = Rig.Find("LH");
        //RHand = Rig.Find("RH");
        //Hip = Rig.Find("Hip");
        //LFoot = Rig.Find("LF");
        //RFoot = Rig.Find("RF");
    }

    private void Update()
    {
        // Move
        if (Input.GetKey(KeyCode.D))
        {
            Translate(new Vector3(speed * Time.deltaTime, 0, 0));
        }
        if (Input.GetKey(KeyCode.A))
        {
            Translate(new Vector3(-speed * Time.deltaTime, 0, 0));
        }
        if (Input.GetKey(KeyCode.E))
        {
            Translate(new Vector3(0, -speed * Time.deltaTime, 0));
        }
        if (Input.GetKey(KeyCode.Q))
        {
            Translate(new Vector3(0, speed * Time.deltaTime, 0));
        }
        if (Input.GetKey(KeyCode.W))
        {
            Translate(new Vector3(0, 0, speed * Time.deltaTime));
        }
        if (Input.GetKey(KeyCode.S))
        {
            Translate(new Vector3(0, 0, -speed * Time.deltaTime));
        }

        // Rotate
        if (Input.GetKey(KeyCode.RightArrow))
        {
            Rotate(new Vector3(0, angularSpeed * Time.deltaTime, 0));
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            Rotate(new Vector3(0, -angularSpeed * Time.deltaTime, 0));
        }
    }

    private void Translate(Vector3 v)
    {
        Rig.Translate(v);
        //Head.Translate(v, Space.World);
        //LHand.Translate(v, Space.World);
        //RHand.Translate(v, Space.World);
        //Hip.Translate(v, Space.World);
        //LFoot.Translate(v, Space.World);
        //RFoot.Translate(v, Space.World);
    }

    private void Rotate(Vector3 euler)
    {
        //Vector3 offset = Rig.position - Hip.position;

        //Head.Translate(offset, Space.World);
        //LHand.Translate(offset, Space.World);
        //RHand.Translate(offset, Space.World);
        //Hip.Translate(offset, Space.World);
        //LFoot.Translate(offset, Space.World);
        //RFoot.Translate(offset, Space.World);

        Rig.Rotate(euler);

        //Head.Translate(-offset, Space.World);
        //LHand.Translate(-offset, Space.World);
        //RHand.Translate(-offset, Space.World);
        //Hip.Translate(-offset, Space.World);
        //LFoot.Translate(-offset, Space.World);
        //RFoot.Translate(-offset, Space.World);
    }
}
