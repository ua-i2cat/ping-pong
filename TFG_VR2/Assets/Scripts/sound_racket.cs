// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sound_racket : MonoBehaviour {

    public AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

    }

    void OnCollisionEnter(Collision collision)
    {
        //And then:
        //adjust asour values, like:
        //and then play:
        if (collision.relativeVelocity.magnitude > 1)
        {
            audioSource.Play();
        }else if (collision.relativeVelocity.magnitude > 0.7)
        {
            audioSource.volume = 0.5f;
            audioSource.Play();

        }
        else
        {
            audioSource.volume = 0.2f;
            audioSource.Play();
        }
    }
}
