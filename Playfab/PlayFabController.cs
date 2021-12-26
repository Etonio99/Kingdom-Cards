using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;

public class PlayFabController : MonoBehaviour
{
    public static PlayFabController instance;

    private string userName;
    private string userEmail;
    private string userPassword;

    public GameObject connectPopupBox;
    [Tooltip("Main Page, Sign Up, Login")]
    public GameObject[] connectPages;
    [Tooltip("Username, Password, Confirm Password, Login Username, Login Password")]
    public InputField[] inputFields;

    private string lastPurchasedItem = "";

    #region Unpermitted Words
    private string[] unpermittedWords = new string[] { "Shit", "Fuck", "Damn", "Cunt", "Penis", "Dick", "Breast", "Boob", "Vagina", "Nigger", "S.h.i.t", "F.u.c.k", "D.a.m.n", "C.u.n.t", "P.e.n.i.s", "D.i.c.k", "B.r.e.a.s.t", "B.o.o.b", "V.a.g.i.n.a", "N.i.g.g.e.r" };
    #endregion

    //TEMP
    public GameObject betaTesterPopup;

    private void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(this);
            instance = this;
            return;
        }
        Destroy(gameObject);
    }

    private void Start()
    {
        if (string.IsNullOrEmpty(PlayFabSettings.TitleId))
        {
            PlayFabSettings.TitleId = "6D887";
        }

        //Attempt Login at Start if PlayerPrefs has saved the player's login information.
        if (PlayerPrefs.HasKey("UserName"))
        {
            userName = PlayerPrefs.GetString("UserName");
            userPassword = PlayerPrefs.GetString("UserPassword");

            AttemptIntialPlayFabLogin();
        }
        else
        {
            ShowConnectPopup(true);
            ChangeConnectPage(0);
        }
    }

    #region Login and Signup

    //Login
    //Used as soon as the game starts to log in the player.
    void AttemptIntialPlayFabLogin()
    {
        var _requestPlayFab = new LoginWithPlayFabRequest { Username = userName, Password = userPassword };
        PlayFabClientAPI.LoginWithPlayFab(_requestPlayFab, OnLoginSuccess, OnInitialLoginError);
    }

    //Used when the Log In button is pressed.
    public void AttemptPlayFabLogin()
    {
        userName = inputFields[4].text;
        userPassword = inputFields[5].text;

        var _requestPlayFab = new LoginWithPlayFabRequest { Username = userName, Password = userPassword };
        PlayFabClientAPI.LoginWithPlayFab(_requestPlayFab, OnLoginSuccess, OnPlayFabError);
    }

    void OnLoginSuccess(LoginResult _result)
    {
        ShowConnectPopup(false);
        PlayerPrefs.SetString("UserName", userName);
        PlayerPrefs.SetString("UserPassword", userPassword);
        StatsManagerScript.instance.isConnected = true;
        StatsManagerScript.instance.myPlayFabID = _result.PlayFabId;
        GetUserData();
        //TEMP
        betaTesterPopup.SetActive(true);
        SceneTransitionsManager.instance.LoadSceneWithHoldTime("MainMenuScene", 2.0f);
    }

    void OnInitialLoginError(PlayFabError _error)
    {
        Debug.LogError(_error.GenerateErrorReport());
        ShowConnectPopup(true);
        ChangeConnectPage(0);
    }

    //Sign Up
    //Used when the Sign Up button is pressed.
    public void AttemptRegisterAccount()
    {
        if (inputFields[0].text == "" || inputFields[1].text == "" || inputFields[2].text == "" || inputFields[3].text == "")
        {
            Debug.Log("Not All Fields have been Filled.");
            return;
        }

        if (inputFields[0].text.Length < 3 || inputFields[0].text.Length > 20)
        {
            Debug.Log("Username must be between 3 and 20 characters.");
            return;
        }

        if (!IsValidEmail(inputFields[1].text))
        {
            Debug.Log("Not a valid Email Address.");
            return;
        }

        if (inputFields[2].text != inputFields[3].text)
        {
            Debug.Log("Passwords Mismatch.");
            return;
        }

        int _unpermittedWordsCount = 0;
        for (int i = 0; i < unpermittedWords.Length; i++)
        {
            if (inputFields[0].text.IndexOf(unpermittedWords[i], System.StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                _unpermittedWordsCount++;
            }
        }
        if (_unpermittedWordsCount > 0)
        {
            Debug.Log("Username Contains Unpermitted Words.");
            return;
        }

        userName = inputFields[0].text;
        userEmail = inputFields[1].text;
        userPassword = inputFields[2].text;

        var _registerRequest = new RegisterPlayFabUserRequest { Username = userName, Email = userEmail, Password = userPassword };
        PlayFabClientAPI.RegisterPlayFabUser(_registerRequest, OnRegisterAccountSuccess, OnPlayFabError);
    }

    void OnRegisterAccountSuccess(RegisterPlayFabUserResult _result)
    {
        ShowConnectPopup(false);
        PlayerPrefs.SetString("UserName", userName);
        PlayerPrefs.SetString("UserPassword", userPassword);
        StatsManagerScript.instance.isConnected = true;
        StatsManagerScript.instance.myPlayFabID = _result.PlayFabId;
        UpdateDisplayName();
        StatsManagerScript.instance.SetUpNewSave();
        //TEMP
        betaTesterPopup.SetActive(true);
        SceneTransitionsManager.instance.LoadScene("MainMenuScene");
    }

    //Used to check if an email address is written in a valid format.
    bool IsValidEmail(string _email)
    {
        try
        {
            var _addr = new System.Net.Mail.MailAddress(_email);
            return _addr.Address == _email;
        }
        catch
        {
            return false;
        }
    }

    //TEMP
    public void CloseBetaTesterPopup()
    {
        betaTesterPopup.SetActive(false);
    }
    //END TEMP

    void UpdateDisplayName()
    {
        PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = userName
        }, result =>
        {
            //Name has been changed successfully.
        }, error => Debug.LogError(error.GenerateErrorReport()));
    }

    public void ShowConnectPopup(bool _setting)
    {
        connectPopupBox.SetActive(_setting);
    }

    public void ChangeConnectPage(int _page)
    {
        //0 = Main, 1 = Sign Up, 2 = Login
        for (int i = 0; i < connectPages.Length; i++)
        {
            if (i == _page)
                connectPages[i].SetActive(true);
            else
                connectPages[i].SetActive(false);
        }
    }

    #endregion Login and Signup

    #region Player Data

    public void GetUserData()
    {
        var _request = new GetUserDataRequest() { PlayFabId = StatsManagerScript.instance.myPlayFabID, Keys = null };
        PlayFabClientAPI.GetUserData(_request, OnGetUserDataSuccess, OnPlayFabError);
    }

    void OnGetUserDataSuccess(GetUserDataResult _result)
    {
        if (_result.Data.ContainsKey("UnlockedCards"))
        {
            string _temp = _result.Data["UnlockedCards"].Value;
            StatsManagerScript.instance.unlockedCards.Clear();
            for (int i = 0; i < _temp.Length; i++)
                StatsManagerScript.instance.unlockedCards.Add(int.Parse(_temp[i].ToString()));
            AssetsManagerScript.instance.SetUpDeckCreatorCards();
        }
        if (_result.Data.ContainsKey("KingdomProgress"))
        {
            string _temp = _result.Data["KingdomProgress"].Value;
            StatsManagerScript.instance.kingdomProgress.Clear();
            if (_temp != null)
            {
                string[] _tempArray = _temp.Split(':');
                for (int i = 0; i < _tempArray.Length; i++)
                    StatsManagerScript.instance.kingdomProgress.Add(int.Parse(_tempArray[i]));
            }
        }
        if (_result.Data.ContainsKey("DeckOne"))
        {
            string _temp = _result.Data["DeckOne"].Value;
            if (_temp != null)
            {
                string[] _tempArray = _temp.Split(':');
                StatsManagerScript.instance.SetUpDeck(1, _tempArray);
            }
        }
        if (_result.Data.ContainsKey("DeckTwo"))
        {
            string _temp = _result.Data["DeckTwo"].Value;
            if (_temp != null)
            {
                string[] _tempArray = _temp.Split(':');
                StatsManagerScript.instance.SetUpDeck(2, _tempArray);
            }
        }
        if (_result.Data.ContainsKey("DeckThree"))
        {
            string _temp = _result.Data["DeckThree"].Value;
            if (_temp != null)
            {
                string[] _tempArray = _temp.Split(':');
                StatsManagerScript.instance.SetUpDeck(3, _tempArray);
            }
        }
        StatsManagerScript.instance.UpdateDeck();
    }

    #endregion Player Data

    #region Virtual Currencies

    public void AddGold(int _amnt)
    {
        if (StatsManagerScript.instance.isConnected)
        {
            ExecuteCloudScriptRequest _request = new ExecuteCloudScriptRequest()
            {
                FunctionName = "AddGold",
                FunctionParameter = new { amnt = _amnt },
                GeneratePlayStreamEvent = true
            };
            PlayFabClientAPI.ExecuteCloudScript(_request, result => { StatsManagerScript.instance.SendMessage("UpdateUI"); }, error => { Debug.Log("Error updating gold."); });
        }
        else
            Debug.Log("You must be logged in to perform this action. (Add Gold)");
    }

    public void AddGems(int _amnt)
    {
        if (StatsManagerScript.instance.isConnected)
        {
            ExecuteCloudScriptRequest _request = new ExecuteCloudScriptRequest()
            {
                FunctionName = "AddGems",
                FunctionParameter = new { amnt = _amnt },
                GeneratePlayStreamEvent = true
            };
            PlayFabClientAPI.ExecuteCloudScript(_request, result => { StatsManagerScript.instance.SendMessage("UpdateUI"); }, error => { Debug.Log("Error updating gems."); });
        }
        else
            Debug.Log("You must be logged in to perform this action. (Add Gems)");
    }

    public void AddTickets(int _amnt)
    {
        if (StatsManagerScript.instance.isConnected)
        {
            ExecuteCloudScriptRequest _request = new ExecuteCloudScriptRequest()
            {
                FunctionName = "AddTickets",
                FunctionParameter = new { amnt = _amnt },
                GeneratePlayStreamEvent = true
            };
            PlayFabClientAPI.ExecuteCloudScript(_request, result => { StatsManagerScript.instance.SendMessage("UpdateUI"); }, error => { Debug.Log("Error updating tickets."); });
        }
        else
            Debug.Log("You must be logged in to perform this action. (Add Tickets)");
    }

    public void AttemptItemPurchase(string _itemID, int _cost, string _currency)
    {
        var _request = new PurchaseItemRequest() { ItemId = _itemID, Price = _cost, VirtualCurrency = _currency };
        PlayFabClientAPI.PurchaseItem(_request, OnPurchaseItemSuccess, OnPlayFabError);
        lastPurchasedItem = _itemID;
    }

    void OnPurchaseItemSuccess(PurchaseItemResult _result)
    {
        if (lastPurchasedItem == "UseTicket")
        {
            GameObject.Find("MainMenuManager").GetComponent<ZoneMenuManagerScript>().ReceiveResponse(true);
            lastPurchasedItem = "";
        }
    }

    #endregion Virtual Currencies

    void OnPlayFabError(PlayFabError _error)
    {
        Debug.LogError(_error.GenerateErrorReport());
        ChangeConnectPage(0);

        if (lastPurchasedItem == "UseTicket")
        {
            GameObject.Find("MainMenuManager").GetComponent<ZoneMenuManagerScript>().ReceiveResponse(false);
            lastPurchasedItem = "";
        }
    }
}