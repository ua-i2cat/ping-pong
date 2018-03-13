// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawn_ball_limit : MonoBehaviour
{

    public GameObject prefab;
    public float height_spawn = 10;
    private SteamVR_TrackedObject trackedObj;

    private float nextActionTime = 5.5f;
    public float relativeTime = 0;
    public float lastBall = 0;

    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }
    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    void Update()
    {
        relativeTime = Time.time - lastBall;
        if (transform.position.z < -0.7 && relativeTime > 1)
        {
            lastBall = Time.time;
            relativeTime = 0;
            nextActionTime += Time.deltaTime;
            Instantiate(prefab, transform.position += Vector3.up * Time.deltaTime * height_spawn, transform.rotation);
        }
    }

}