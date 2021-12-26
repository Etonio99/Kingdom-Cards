using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class SettingsManagerScript : MonoBehaviour
{
    public GameObject cheatButton;

    //TEMP -----------------------------------------
    public void SetCardBack(int _id)
    {
        StatsManagerScript.instance.curCardBack = _id;
    }
    //END TEMP -------------------------------------

    private void Start()
    {
        if (PlayerPrefs.GetString("UserName") == "kingdomcards")
        {
            cheatButton.SetActive(true);
        }
    }

    public void EraseSaveData()
    {
        StatsManagerScript.instance.EraseSavedGame();
        Destroy(GameObject.Find("GameManager"));
        SceneTransitionsManager.instance.LoadSceneWithoutReference("MainMenuScene");
    }

    public void LoadScene(string _sceneName)
    {
        StatsManagerScript.instance.LoadGame();
        SceneTransitionsManager.instance.LoadScene(_sceneName);
    }

    public void SignOut()
    {
        PlayFabClientAPI.ForgetAllCredentials();
        PlayerPrefs.DeleteKey("UserName");
        PlayerPrefs.DeleteKey("UserPassword");
        StatsManagerScript.instance.isConnected = false;

        StatsManagerScript.instance.ResetForLogin();

        PlayFabController.instance.ShowConnectPopup(true);
        PlayFabController.instance.ChangeConnectPage(0);
        SceneTransitionsManager.instance.LoadScene("LoginScene");
    }

    public void OpenChest(int _id)
    {
        ChestManagerScript.instance.OpenChest(_id, "Village");
    }
}