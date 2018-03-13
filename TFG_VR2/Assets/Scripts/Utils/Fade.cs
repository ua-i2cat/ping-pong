// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Fade : MonoBehaviour
{
    public Image img;
    public float fadeInDuration = 2.0f;
    public AnimationCurve fadeIn;

    private float elapsedTime = 0.0f;

    private void Awake()
    {
        // Start with black screen
        img.color = new Color(0, 0, 0, 1);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Update ()
    {
        elapsedTime += Time.deltaTime;

        if(elapsedTime >= 0.2f && elapsedTime <= fadeInDuration)
        {
            img.color = new Color(0, 0, 0, fadeIn.Evaluate(elapsedTime / fadeInDuration));
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset elapsedTime when a new scene is loaded to perform Fade again
        elapsedTime = 0.0f;
    }
}
