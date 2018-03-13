// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    public Transform spawnPosition;
    public GameObject prefab;

    public Transform ballT;

	// Use this for initialization
	void Start ()
    {
        //InvokeRepeating("SpawnBall", 0.0f, 2.0f);
        InvokeRepeating("SetBallPos", 0.0f, 0.1f);
    }

    private void SpawnBall()
    {
        if (spawnPosition != null)
        {
            //GameObject ball = Instantiate(prefab, spawnPosition.position + new Vector3(0, 0.5f, 0), spawnPosition.rotation);
            //ServerManagerPaddle.world.balls.Add(new ServerObject(ball));
        }
        else
        {
            var paddle = GameObject.Find("paddle_attached_server(Clone)");
            if(paddle != null)
            {
                Debug.Log("paddle found");
                spawnPosition = paddle.transform.GetChild(0);
            }
        }
    }

    private void SetBallPos()
    {
        if(ballT == null)
        {
            var ball = GameObject.Find("Ball");
            if (ball != null)
                ballT = ball.transform;
            else
                return;
        }

        if(Vector3.Distance(ballT.position, spawnPosition.position) > 1.0f)
        {
            Rigidbody rb = ballT.GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            ballT.rotation = Quaternion.identity;
            ballT.position = spawnPosition.position + new Vector3(0, 0.35f, 0);
        }  
    }
}
