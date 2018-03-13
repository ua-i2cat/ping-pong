// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public VRTK.VRTK_HeadsetFade headsetFade;
    public GameObject VRTK_Manager;

    public void Start()
    {
        //GUI.color = Color.black;
        //GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTexture);
        //SteamVR_Fade.Start(Color.black, 0);
        //SteamVR_Fade.Start(Color.clear, 1);
        //headsetFade.Fade(Color.black, 0);
        //headsetFade.Unfade(2);
    }

    public void StartButton_Click()
    {
        headsetFade.Fade(Color.black, 2);
        Invoke("LoadGame", 2.2f);       
    }

    public void OptionsButton_Click()
    {

    }

    public void ExitButton_Click()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    private void LoadGame()
    {
        //Camera.main.enabled = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene("ClientSceneMalcolm");       
        //headsetFade.Unfade(2);
    }
}
