// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Client_0 : MonoBehaviour, IClientPrefab
{
    public float speed = 3.0f;
    public float angularSpeed = 100.0f;

    private Transform player;

    public List<byte> GetMotionData()
    {
        Debug.Log("GetMotionData Client_0");
        List<byte> motionData = new List<byte>();
        return motionData;
    }

    public void SetMotionData(List<byte> motionData)
    {
        Debug.Log("SetMotionData Client_0");
    }

    private void Start()
    {
        player = this.transform.parent;
    }

    private void Update()
    {
        // Move
        if (Input.GetKey(KeyCode.D))
        {
            player.Translate(new Vector3(speed * Time.deltaTime, 0, 0));
        }
        if (Input.GetKey(KeyCode.A))
        {
            player.Translate(new Vector3(-speed * Time.deltaTime, 0, 0));
        }
        if (Input.GetKey(KeyCode.E))
        {
            player.Translate(new Vector3(0, -speed * Time.deltaTime, 0));
        }
        if (Input.GetKey(KeyCode.Q))
        {
            player.Translate(new Vector3(0, speed * Time.deltaTime, 0));
        }
        if (Input.GetKey(KeyCode.W))
        {
            player.Translate(new Vector3(0, 0, speed * Time.deltaTime));
        }
        if (Input.GetKey(KeyCode.S))
        {
            player.Translate(new Vector3(0, 0, -speed * Time.deltaTime));
        }

        // Rotate
        if(Input.GetKey(KeyCode.RightArrow))
        {
            player.Rotate(new Vector3(0, angularSpeed * Time.deltaTime, 0));
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            player.Rotate(new Vector3(0, -angularSpeed * Time.deltaTime, 0));
        }
    }
}
