// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sound_table : MonoBehaviour
{

    public AudioSource audioSource_table;

    void Start()
    {
        audioSource_table = GetComponent<AudioSource>();

    }

    void OnCollisionEnter(Collision collision)
    {
        //And then:
        //adjust asour values, like:
        //and then play:
        if (collision.relativeVelocity.magnitude > 1.7)
        {
            audioSource_table.volume = 3.5f;
            audioSource_table.Play();
        }
        else if (collision.relativeVelocity.magnitude > 1.2)
        {
            audioSource_table.volume = 3f;

            audioSource_table.Play();

        }
        else
        {
            audioSource_table.volume = 1.5f;
            audioSource_table.Play();
        }
    }
}
