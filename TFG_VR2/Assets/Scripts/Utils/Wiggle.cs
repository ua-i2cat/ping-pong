// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using UnityEngine;
using System.Collections;

public class Wiggle : MonoBehaviour
{
    public AnimationCurve curve;
    public Vector3 distance;
    public float speed;

    private Vector3 startPos, toPos;
    private float timeStart;

    void randomToPos()
    {
        toPos = startPos;
        toPos.x += Random.Range(-1.0f, +1.0f) * distance.x;
        toPos.y += Random.Range(-1.0f, +1.0f) * distance.y;
        toPos.z += Random.Range(-1.0f, +1.0f) * distance.z;
        timeStart = Time.time;
    }

    // Use this for initialization
    void Start()
    {
        startPos = transform.position;
        randomToPos();
    }

    // Update is called once per frame
    void Update()
    {
        float d = (Time.time - timeStart) / speed, m = curve.Evaluate(d);
        if (d > 1)
        {
            randomToPos();
        }
        else if (d < 0.5)
        {
            transform.position = Vector3.Lerp(startPos, toPos, m * 2.0f);
        }
        else
        {
            transform.position = Vector3.Lerp(toPos, startPos, (m - 0.5f) * 2.0f);
        }
    }
}