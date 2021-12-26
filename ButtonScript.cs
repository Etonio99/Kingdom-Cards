using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonScript : MonoBehaviour
{
    public void LoadScene(string _sceneName)
    {
        SceneTransitionsManager.instance.LoadScene(_sceneName);
    }
}