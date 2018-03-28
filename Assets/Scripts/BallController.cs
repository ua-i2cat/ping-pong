// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    private GameObject paddle;
    private Rigidbody rb;
    public Vector3 ballOffset = new Vector3(0, 0.5f, 0);
    public bool serve = false;
    //private PaddleSpeed paddleSpeed;

    private void Start()
    {
        //Physics.gravity = new Vector3(0, -4f, 0);
        rb = GetComponent<Rigidbody>();
        //paddleSpeed = paddle.GetComponentInChildren<PaddleSpeed>();
    }

    private void Update()
    {
        if(paddle != null && serve
            /*Input.GetKeyDown(KeyCode.Space)*/ 
            /*Vector3.Distance(paddle.transform.position, transform.position) > 1*/)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.rotation = Quaternion.identity;
            Vector3 paddlePos = paddle.transform.position;
            Vector3 paddleBase = paddle.GetComponent<BoxCollider>().center;
            Debug.DrawLine(paddlePos, paddlePos + paddle.transform.forward * 0.2f, Color.red, 10);
            Debug.DrawLine(paddlePos + paddle.transform.forward * 0.2f,
                paddlePos + paddle.transform.forward * 0.2f + paddle.transform.up * 0.5f,
                Color.blue, 10);
            transform.position = paddlePos + paddle.transform.forward * 0.2f + Vector3.up * 0.5f;           

            serve = false;
        }

        if(paddle == null)
        {
            paddle = GameObject.Find(Constants.RightHand);
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        //if (rb && collision.gameObject.name == "paddle")
        //{
        //    Vector3 dir = collision.relativeVelocity.normalized;

        //    if(paddleSpeed.velocity.magnitude > 0.2f)
        //    {
        //        rb.velocity = Vector3.zero;
        //        rb.angularVelocity = Vector3.zero;
        //        rb.AddForce(dir * 2);
        //    }
        //    else
        //        rb.AddForce(dir);

        //    Debug.Log(paddleSpeed.velocity.magnitude);
        //}
    }
}
