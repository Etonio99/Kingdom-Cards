using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;

public class StatsManagerScript : MonoBehaviour
{
    public static StatsManagerScript instance;

    public string gameVersionCode = "0000";

    public bool isConnected = false;
    public string myPlayFabID = "";

    [HideInInspector]
    public List<int> unlockedCards = new List<int>();
    //Order of Kingdoms as shown in the Assets Manager.
    [HideInInspector]
    public List<int> kingdomProgress = new List<int>();

    [HideInInspector]
    public GameObject statsBar;
    public Text[] statTexts; //Gold, Gems, Tickets.

    [Header("Player Stats")]
    public int gold = 0;
    public int gems = 0;
    public int tickets = 0;

    [Header("Player Deck")]
    public List<string> playerDeck = new List<string>();
    public List<string> playerCurDeck = new List<string>();

    public List<string> starterDeck = new List<string>();

    private List<string> savedDeckOne = new List<string>();
    private List<string> savedDeckTwo = new List<string>();
    private List<string> savedDeckThree = new List<string>();

    public int curCardBack = 0;

    //[HideInInspector]
    public int activeDeck;

    //Used when an account is created to get the user data after it is saved to the server.
    private bool retrieveData = false;

    private void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(this);
            instance = this;
            return;
        }
        Destroy(gameObject);

        if (File.Exists(Application.persistentDataPath + "/kcsave.dat"))
            File.Delete(Application.persistentDataPath + "/kcsave.dat");
    }

    private void Start()
    {
        statsBar = transform.Find("GameCanvas").Find("StatsBar").gameObject;

        LoadGame();
        UpdateUI();

        activeDeck = PlayerPrefs.GetInt("ActiveDeck");

        if (kingdomProgress.Count < AssetsManagerScript.instance.kingdoms.Length)
        {
            for (int i = kingdomProgress.Count; i < AssetsManagerScript.instance.kingdoms.Length; i++)
                kingdomProgress.Add(-1);
            if (kingdomProgress[0] == -1)
                kingdomProgress[0] = 0;
        }
    }

    public void ShowStatsBar(bool _setting)
    {
        statsBar.SetActive(_setting);
    }

    public void UpdateUI()
    {
        if (isConnected)
        {
            GetUserInventoryRequest _request = new GetUserInventoryRequest();
            PlayFabClientAPI.GetUserInventory(_request, GetUserInventory, error => { Debug.LogError("Failed to Receive Player Currencies."); });
        }
    }

    void GetUserInventory(GetUserInventoryResult _result)
    {
        _result.VirtualCurrency.TryGetValue("GO", out gold);
        _result.VirtualCurrency.TryGetValue("GE", out gems);
        _result.VirtualCurrency.TryGetValue("TI", out tickets);

        statTexts[0].text = gold.ToString();
        statTexts[1].text = gems.ToString();
        statTexts[2].text = tickets + "/7";
    }

    public void SaveGame()
    {
        UpdatePlayerData();
    }

    public void UpdatePlayerData()
    {
        string _msgUnlockedCards = "";
        for (int i = 0; i < unlockedCards.Count; i++)
        {
            _msgUnlockedCards += unlockedCards[i].ToString();
        }

        string _msgKingdomProgress = "";
        for (int i = 0; i < kingdomProgress.Count; i++)
        {
            if (i < kingdomProgress.Count - 1)
                _msgKingdomProgress += kingdomProgress[i] + ":";
            else
                _msgKingdomProgress += kingdomProgress[i];
        }

        string _msgDeckOne = "";
        for (int i = 0; i < savedDeckOne.Count; i++)
        {
            for (int j = 0; j < AssetsManagerScript.instance.cards.Length; j++)
            {
                if (AssetsManagerScript.instance.cards[j].creatureName == savedDeckOne[i])
                {
                    if (i < savedDeckOne.Count - 1)
                        _msgDeckOne += j + ":";
                    else
                        _msgDeckOne += j;
                    break;
                }
            }
        }

        string _msgDeckTwo = "";
        for (int i = 0; i < savedDeckTwo.Count; i++)
        {
            for (int j = 0; j < AssetsManagerScript.instance.cards.Length; j++)
            {
                if (AssetsManagerScript.instance.cards[j].creatureName == savedDeckTwo[i])
                {
                    if (i < savedDeckTwo.Count - 1)
                        _msgDeckTwo += j + ":";
                    else
                        _msgDeckTwo += j;
                    break;
                }
            }
        }

        string _msgDeckThree = "";
        for (int i = 0; i < savedDeckThree.Count; i++)
        {
            for (int j = 0; j < AssetsManagerScript.instance.cards.Length; j++)
            {
                if (AssetsManagerScript.instance.cards[j].creatureName == savedDeckThree[i])
                {
                    if (i < savedDeckThree.Count - 1)
                        _msgDeckThree += j + ":";
                    else
                        _msgDeckThree += j;
                    break;
                }
            }
        }

        UpdateUserDataRequest _request = new UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string>()
            {
                {"UnlockedCards", _msgUnlockedCards },
                {"KingdomProgress", _msgKingdomProgress },
                {"DeckOne", _msgDeckOne },
                {"DeckTwo", _msgDeckTwo },
                {"DeckThree", _msgDeckThree }
            }
        };
        PlayFabClientAPI.UpdateUserData(_request, OnUpdateDataSuccess, OnPlayFabError);
    }

    void OnUpdateDataSuccess(UpdateUserDataResult _result)
    {
        //Data has been updated successfully.

        if (retrieveData)
        {
            PlayFabController.instance.GetUserData();
            retrieveData = false;
        }
    }

    void OnPlayFabError(PlayFabError _error)
    {
        Debug.LogError(_error.GenerateErrorReport());
    }

    public void LoadGame()
    {
        if (File.Exists(Application.persistentDataPath + "/kcsave.dat"))
        {
            BinaryFormatter _bf = new BinaryFormatter();
            FileStream _file = File.Open(Application.persistentDataPath + "/kcsave.dat", FileMode.Open);
            Save _save = (Save)_bf.Deserialize(_file);
            _file.Close();

            if (gameVersionCode != _save.gameVersionCode)
            {
                //Do something if the game has updated.
                gameVersionCode = _save.gameVersionCode;
            }

            UpdateDeck();
        }
        else
        {
            //Debug.Log("No Save File Exists!");
            //Debug.Log("Setting up a New Save.");
            //SetUpNewSave();
        }
    }

    public void UpdateDeck()
    {
        playerDeck.Clear();
        if (PlayerPrefs.GetInt("ActiveDeck") == 1)
            foreach (string _s in savedDeckOne)
                playerDeck.Add(_s);
        else if (PlayerPrefs.GetInt("ActiveDeck") == 2)
            foreach (string _s in savedDeckTwo)
                playerDeck.Add(_s);
        else if (PlayerPrefs.GetInt("ActiveDeck") == 3)
            foreach (string _s in savedDeckThree)
                playerDeck.Add(_s);

        AssetsManagerScript.instance.UpdatePlayerDeck();
    }

    //Used when loading decks from the server.
    public void SetUpDeck(int _deckNum, string[] _array)
    {
        int[] _cardIDs = new int[_array.Length];
        for (int i = 0; i < _array.Length; i++)
            _cardIDs[i] = int.Parse(_array[i]);

        if (_deckNum == 1)
        {
            savedDeckOne.Clear();
            for (int i = 0; i < _cardIDs.Length; i++)
                savedDeckOne.Add(AssetsManagerScript.instance.cards[_cardIDs[i]].creatureName);
        }
        if (_deckNum == 2)
        {
            savedDeckTwo.Clear();
            for (int i = 0; i < _cardIDs.Length; i++)
                savedDeckTwo.Add(AssetsManagerScript.instance.cards[_cardIDs[i]].creatureName);
        }
        if (_deckNum == 3)
        {
            savedDeckThree.Clear();
            for (int i = 0; i < _cardIDs.Length; i++)
                savedDeckThree.Add(AssetsManagerScript.instance.cards[_cardIDs[i]].creatureName);
        }
    }

    //Resets all info to allow room for the new account's info to be added.
    public void ResetForLogin()
    {
        playerDeck.Clear();
        savedDeckOne.Clear();
        savedDeckTwo.Clear();
        savedDeckThree.Clear();
        AssetsManagerScript.instance.playerDeck.Clear();
        kingdomProgress.Clear();
        for (int i = kingdomProgress.Count; i < AssetsManagerScript.instance.kingdoms.Length; i++)
            kingdomProgress.Add(-1);
        if (kingdomProgress[0] == -1)
            kingdomProgress[0] = 0;
    }

    //Used when a new player creates an account to setup the necessary information for him/her to begin playing.
    public void SetUpNewSave()
    {
        //PlayerPrefs.DeleteAll();
        Debug.Log("Setting up new account.");
        playerDeck.Clear();
        savedDeckOne.Clear();
        savedDeckTwo.Clear();
        savedDeckThree.Clear();

        PlayerPrefs.SetInt("ActiveDeck", 1);
        foreach (string _s in starterDeck)
            playerDeck.Add(_s);
        foreach (string _s in starterDeck)
            savedDeckOne.Add(_s);
        foreach (string _s in starterDeck)
            savedDeckTwo.Add(_s);
        foreach (string _s in starterDeck)
            savedDeckThree.Add(_s);

        for (int i = 0; i < AssetsManagerScript.instance.cards.Length; i++)
            unlockedCards.Add(0);
        
        AddCardToCollection("Glob", 3);
        AddCardToCollection("Slime", 3);
        AddCardToCollection("Living Sword", 3);
        AddCardToCollection("Blink", 3);
        AddCardToCollection("Gaze", 3);
        AddCardToCollection("Thicket", 3);
        AddCardToCollection("Bloom Golem", 3);
        

        //TEMP
        /*for (int i = 0; i < AssetsManagerScript.instance.cards.Length; i++)
        {
            if (AssetsManagerScript.instance.cards[i].canBeInDeck)
                unlockedCards.Add(3);
            else
                unlockedCards.Add(0);
        }*/

        StartCoroutine(DelayUpdatePlayerData());
    }

    IEnumerator DelayUpdatePlayerData()
    {
        //FIND A WAY TO DO THIS WIHOUT JUST ADDING DELAYS!!!!!!
        yield return new WaitForSeconds(1.0f);
        UpdatePlayerData();
        retrieveData = true;
    }

    public void EraseSavedGame()
    {
        if (File.Exists(Application.persistentDataPath + "/kcsave.dat"))
        {
            File.Delete(Application.persistentDataPath + "/kcsave.dat");
            Debug.Log("Saved Game has been Erased.");
        }
        else
        {
            Debug.Log("No Saved Game to Erase!");
        }
    }

    public void AddCardToCollection(string _cardName, int _amnt)
    {
        for (int i = 0; i < AssetsManagerScript.instance.cards.Length; i++)
        {
            if (AssetsManagerScript.instance.cards[i].creatureName == _cardName)
            {
                unlockedCards[i] += _amnt;
                if (unlockedCards[i] > 3)
                    unlockedCards[i] = 3;
                if (unlockedCards[i] < 0)
                    unlockedCards[i] = 0;
                break;
            }
        }
    }

    public void UpdateSavedDecks()
    {
        if (activeDeck == 1)
        {
            savedDeckOne.Clear();
            for (int i = 0; i < AssetsManagerScript.instance.playerDeck.Count; i++)
                savedDeckOne.Add(AssetsManagerScript.instance.playerDeck[i].creatureName);
        }
        else if (activeDeck == 2)
        {
            savedDeckTwo.Clear();
            for (int i = 0; i < AssetsManagerScript.instance.playerDeck.Count; i++)
                savedDeckTwo.Add(AssetsManagerScript.instance.playerDeck[i].creatureName);
        }
        else if (activeDeck == 3)
        {
            savedDeckThree.Clear();
            for (int i = 0; i < AssetsManagerScript.instance.playerDeck.Count; i++)
                savedDeckThree.Add(AssetsManagerScript.instance.playerDeck[i].creatureName);
        }
    }
}

[System.Serializable]
public class Save
{
    public string gameVersionCode;

    public int gold = 0;
    public int gems = 0;
    public int tickets = 0;

    public List<int> unlockedCards = new List<int>();

    public List<string> savedDeckOne = new List<string>();
    public List<string> savedDeckTwo = new List<string>();
    public List<string> savedDeckThree = new List<string>();
}