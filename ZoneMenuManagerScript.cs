using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ZoneMenuManagerScript : MonoBehaviour
{
    [Header("Battlers")]
    public KingdomBattlers[] kingdomBattlers;
    private Battler activeBattler;
    private int activeKingdom = 0;

    [Header("UI Setup")]
    public GameObject battlerSelectMenu;
    public Image displayImage;
    public Text titleText;
    public Text nameText;
    [Tooltip("Health, Mana, Deck Count")]
    public Text[] statTexts;
    [Tooltip("Gold, Gems")]
    public Text[] rewardTexts;
    public Animator chestAnimator;
    public GameObject battlerButtonPrefab;
    public Transform battlerButtonsHolder;
    private List<GameObject> savedButtons = new List<GameObject>();

    private bool canBattle = true;
    public Image battleButton;
    private bool locked = false;
    public GameObject buttonLock;

    private bool waitingForResponse = false;
    private bool responseReceived = false;

    public void ShowMenu(bool _setting)
    {
        battlerSelectMenu.SetActive(_setting);
    }

    public void SetupMenu()
    {
        if (!waitingForResponse)
        {
            foreach (GameObject _go in savedButtons)
                Destroy(_go);
            savedButtons.Clear();

            for (int i = 0; i < kingdomBattlers[activeKingdom].battlers.Length; i++)
            {
                BattlerButtonScript _buttonInstance = Instantiate(battlerButtonPrefab, Vector2.zero, Quaternion.identity).GetComponent<BattlerButtonScript>();
                _buttonInstance.battlerId = i;
                _buttonInstance.GetComponent<Image>().sprite = kingdomBattlers[activeKingdom].battlers[i].buttonSprite;
                _buttonInstance.transform.SetParent(battlerButtonsHolder);
                _buttonInstance.transform.localScale = new Vector3(3.75f, 3.75f, 3.75f);
                savedButtons.Add(_buttonInstance.gameObject);
            }

            SetBattler(0);
        }
    }

    public void UpdateMenu()
    {
        if (activeBattler != null && !waitingForResponse && !BattleManagerScript.instance.activeBattle)
        {
            displayImage.sprite = activeBattler.displaySprite;
            titleText.text = activeBattler.title;
            nameText.text = activeBattler.name;
            statTexts[0].text = activeBattler.health.ToString();
            statTexts[1].text = activeBattler.mana.ToString();
            statTexts[2].text = activeBattler.deck.Count.ToString();

            if (activeBattler.rewardGold > 0)
            {
                rewardTexts[0].text = activeBattler.rewardGold.ToString();
                rewardTexts[0].transform.parent.gameObject.SetActive(true);
            }
            else
                rewardTexts[0].transform.parent.gameObject.SetActive(false);

            if (activeBattler.rewardGems > 0)
            {
                rewardTexts[1].text = activeBattler.rewardGems.ToString();
                rewardTexts[1].transform.parent.gameObject.SetActive(true);
            }
            else
                rewardTexts[1].transform.parent.gameObject.SetActive(false);

            if (activeBattler.rewardChest > -1)
            {
                chestAnimator.runtimeAnimatorController = ChestManagerScript.instance.chestAnimators[ChestManagerScript.instance.chests[activeBattler.rewardChest].animatorID];
                chestAnimator.transform.parent.gameObject.SetActive(true);
            }
            else
                chestAnimator.transform.parent.gameObject.SetActive(false);

            if (StatsManagerScript.instance.tickets <= 0)
                battleButton.color = Color.gray;
            else if (!canBattle)
                battleButton.color = Color.gray;
            else if (locked)
                battleButton.color = Color.gray;
            else
                battleButton.color = Color.white;

            if (locked)
                buttonLock.SetActive(true);
            else
                buttonLock.SetActive(false);
        }
    }

    public void StartBattle()
    {
        if (!waitingForResponse && !BattleManagerScript.instance.activeBattle && canBattle && !locked)
        {
            if (StatsManagerScript.instance.playerDeck.Count == 20)
            {
                if (StatsManagerScript.instance.tickets > 0)
                {
                    PlayFabController.instance.AttemptItemPurchase("UseTicket", 1, "TI");
                    waitingForResponse = true;
                }
                else
                {
                    PopupManagerScript.instance.AddPopup("Not Enough Tickets!", "You need at least one ticket to battle. You receive one every hour.");
                }
            }
            else
            {
                PopupManagerScript.instance.AddPopup("Incomplete Deck!", "Your deck must have 20 cards in order to battle.");
            }
        }
        if (locked)
        {
            PopupManagerScript.instance.AddPopup("Progress Needed", "You must defeat the previous battler in order to unlock this one!");
        }
    }

    public void UpdateKingdomProgress()
    {
        if (StatsManagerScript.instance.kingdomProgress[activeKingdom] == activeBattler.battlerNumber && StatsManagerScript.instance.kingdomProgress[activeKingdom] < kingdomBattlers[activeKingdom].battlers.Length - 1)
        {
            StatsManagerScript.instance.kingdomProgress[activeKingdom] = activeBattler.battlerNumber + 1;
            PopupManagerScript.instance.AddPopup("Unlock!", "New Battler Unlocked!");
            StatsManagerScript.instance.SaveGame();
        }
    }

    public void ReceiveResponse(bool _successful)
    {
        if (_successful)
            responseReceived = true;
        else
            PopupManagerScript.instance.AddPopup("Error", "An error occured.");
    }

    private void Update()
    {
        if (waitingForResponse && responseReceived)
        {
            waitingForResponse = false;
            responseReceived = false;
            BattleManagerScript.instance.ts.BattleTransition(activeBattler.name, activeBattler.battlerSprite);
            StartCoroutine(DelayBattleStart());
        }
    }

    IEnumerator DelayBattleStart()
    {
        yield return new WaitForSeconds(3.1f);
        BattleManagerScript.instance.StartNewBattle(activeBattler);
        BattleManagerScript.instance.SetRewards(activeBattler.rewardGold, activeBattler.rewardGems, activeBattler.rewardChest, kingdomBattlers[activeKingdom].kingdomName);
    }

    public void SetBattler(int _id)
    {
        if (kingdomBattlers[activeKingdom].battlers.Length > _id && !waitingForResponse && !BattleManagerScript.instance.activeBattle)
        {
            activeBattler = kingdomBattlers[activeKingdom].battlers[_id];
            canBattle = true;
            locked = false;

            for (int i = 0; i < savedButtons.Count; i++)
            {
                if (i == _id)
                {
                    savedButtons[i].GetComponent<Image>().color = Color.gray;
                    if (StatsManagerScript.instance.kingdomProgress[activeKingdom] < i)
                    {
                        canBattle = false;
                        locked = true;
                    }
                }
                else
                    savedButtons[i].GetComponent<Image>().color = Color.white;
            }
        }
    }

    public void SetKingdom(int _id)
    {
        if (kingdomBattlers.Length > _id && !waitingForResponse && !BattleManagerScript.instance.activeBattle)
        {
            activeKingdom = _id;
        }
    }

    public Vector3 GetRewards()
    {
        Vector3 _tempRewards = Vector3.zero;
        _tempRewards.x = activeBattler.rewardGold;
        _tempRewards.y = activeBattler.rewardGems;
        _tempRewards.z = activeBattler.rewardChest;
        return _tempRewards;
    }

    public void OpenChest()
    {
        if (activeBattler.rewardChest > -1)
            ChestManagerScript.instance.OpenChest(activeBattler.rewardChest, activeBattler.kingdom);
    }
}

[System.Serializable]
public class KingdomBattlers
{
    public string kingdomName = "Unnamed Kingdom";
    public Battler[] battlers;
}