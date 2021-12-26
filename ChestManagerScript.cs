using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChestManagerScript : MonoBehaviour
{
    public static ChestManagerScript instance;

    public RuntimeAnimatorController[] chestAnimators;
    public Chest[] chests;

    public GameObject chestPopup;
    public Animator chestPopupAnimator;
    private Transform chestPopupTransform;

    public GameObject rewardPopup;
    public Image rewardImage;
    public Text rewardText;

    private bool canOpenChest = false;
    private bool canTap = false;

    private Vector2 chestPos;
    private bool moveChest = false;

    private Chest activeChest;
    private int contentAmount = 0;
    //Gold, Gems, Tickets.
    private Vector3 rewards;
    public GameObject chestRewardPrefab;
    public GameObject chestCardPrefab;
    private List<string> rewardCards = new List<string>();
    private int curKingdomID = -1;
    private int curItem = 0;
    public Text amountText;

    private bool sendCanTapDialogue = false;

    [Tooltip("Gold, Gems, Tickets")]
    public Sprite[] rewardSprites;

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
        chestPopupTransform = chestPopupAnimator.GetComponent<Transform>();
        chestPos = chestPopupTransform.localPosition;
    }

    public void OpenChest(int _chestID, string _kingdom)
    {
        DialogueManager.instance.canTap = false;

        for (int i = 0; i < AssetsManagerScript.instance.kingdoms.Length; i++)
        {
            if (AssetsManagerScript.instance.kingdoms[i].kingdomName == _kingdom)
            {
                curKingdomID = i;
                break;
            }
        }

        chestPopupTransform.localPosition = new Vector2(2500f, chestPos.y);
        moveChest = true;

        activeChest = chests[_chestID];

        //Gold
        rewards.x = Mathf.Round(Random.Range(activeChest.goldMinMax.x, activeChest.goldMinMax.y));
        if (rewards.x > 0)
        {
            contentAmount++;
            PlayFabController.instance.AddGold((int)rewards.x);
        }
        //Gems
        rewards.y = Mathf.Round(Random.Range(activeChest.gemsMinMax.x, activeChest.gemsMinMax.y));
        if (rewards.y > 0)
        {
            contentAmount++;
            PlayFabController.instance.AddGems((int)rewards.y);
        }
        //Tickets
        if (activeChest.ticketChance > 0)
        {
            float _rand = Random.Range(0.0f, 1.0f);
            if (_rand <= activeChest.ticketChance)
            {
                contentAmount++;
                PlayFabController.instance.AddTickets(1);
                rewards.z = 1;
            }
        }
        //Cards
        if (activeChest.possibleCards.Length > 0 && curKingdomID > -1)
        {
            for (int i = 0; i < activeChest.possibleCards.Length; i++)
            {
                float _rand = Random.Range(0.0f, 1.0f);
                if (_rand <= activeChest.possibleCards[i].cardChance)
                {
                    int _cardChoice = Random.Range(0, AssetsManagerScript.instance.kingdoms[curKingdomID].unlockableCards.Length);
                    rewardCards.Add(AssetsManagerScript.instance.kingdoms[curKingdomID].unlockableCards[_cardChoice]);
                    contentAmount++;
                }
            }
        }

        chestPopup.SetActive(true);
        chestPopupAnimator.runtimeAnimatorController = chestAnimators[activeChest.animatorID];

        StartCoroutine(DelayCanOpenChest(1.0f));
    }

    IEnumerator DelayCanOpenChest(float _time)
    {
        yield return new WaitForSeconds(_time);
        canOpenChest = true;
        amountText.text = contentAmount.ToString();
        amountText.gameObject.SetActive(true);
    }

    IEnumerator DelayCanTap(float _time)
    {
        yield return new WaitForSeconds(_time);
        canTap = true;
    }

    IEnumerator ShowFirstItem()
    {
        yield return new WaitForSeconds(0.5f);
        ShowNextItem();
    }

    private void Update()
    {
        if (moveChest)
        {
            float _interpolation = 7f * Time.fixedDeltaTime;
            Vector2 _newPos = Vector2.Lerp(chestPopupTransform.localPosition, chestPos, _interpolation);
            chestPopupTransform.localPosition = _newPos;
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (canOpenChest)
            {
                chestPopupAnimator.SetTrigger("Open");
                canOpenChest = false;
                StartCoroutine(DelayCanTap(1.2f));
                StartCoroutine(ShowFirstItem());
            }

            if (canTap)
            {
                ShowNextItem();
                chestPopupAnimator.SetTrigger("Bounce");
            }
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            if (sendCanTapDialogue)
            {
                StartCoroutine(DelaySendCanTapDialogue());
                sendCanTapDialogue = false;
            }
        }
    }

    IEnumerator DelaySendCanTapDialogue()
    {
        yield return new WaitForSeconds(0.2f);
        DialogueManager.instance.canTap = true;
    }

    void ShowNextItem()
    {
        canTap = false;
        if (curItem < contentAmount)
        {
            StartCoroutine(DelayCanTap(0.65f));

            //Takes care of the gold, gems, and tickets.
            switch (curItem)
            {
                case 0:
                    if (rewards.x == 0)
                    {
                        curItem++;
                        contentAmount++;
                        break;
                    }
                    CreateRewardPopup(0, (int)rewards.x);
                    break;
                case 1:
                    if(rewards.y == 0)
                    {
                        curItem++;
                        contentAmount++;
                        break;
                    }
                    CreateRewardPopup(1, (int)rewards.y);
                    break;
                case 2:
                    if (rewards.z == 0)
                    {
                        curItem++;
                        contentAmount++;
                        break;
                    }
                    CreateRewardPopup(2, (int)rewards.z);
                    break;
            }
            //Takes care of the cards.
            if (curItem > 2)
            {
                if (GameObject.FindGameObjectWithTag("ChestReward"))
                {
                    ChestRewardScript _activeReward = GameObject.FindGameObjectWithTag("ChestReward").GetComponent<ChestRewardScript>();
                    _activeReward.Wipe();
                }

                if (GameObject.FindGameObjectWithTag("DisplayCard"))
                {
                    ChestCardScript _activeCard = GameObject.FindGameObjectWithTag("DisplayCard").GetComponent<ChestCardScript>();
                    _activeCard.Wipe();
                }

                ChestCardScript _instance = Instantiate(chestCardPrefab, new Vector2(0f, -3f), Quaternion.identity).GetComponent<ChestCardScript>();
                _instance.SetUpCard(rewardCards[curItem - 3]);
                _instance.SetMovePos(new Vector2(0f, 1f));

                int _cardID = 0;
                for (int i = 0; i < AssetsManagerScript.instance.cards.Length; i++)
                {
                    if (AssetsManagerScript.instance.cards[i].creatureName == rewardCards[curItem - 3])
                    {
                        _cardID = i;
                        break;
                    }
                }
                int _ownedAmnt = StatsManagerScript.instance.unlockedCards[_cardID];
                if (_ownedAmnt == 0)
                {
                    //New Card!
                    _instance.ownedText.text = "New Card!";
                    StatsManagerScript.instance.unlockedCards[_cardID]++;
                }
                else if (_ownedAmnt >= 3)
                {
                    PlayFabController.instance.AddGold(25);
                    _instance.ShowGoldReturn();
                    _instance.ownedText.text = "Max Owned";
                }
                else
                {
                    StatsManagerScript.instance.unlockedCards[_cardID]++;
                    _instance.ownedText.text = "You Own " + (_ownedAmnt + 1) + "/3";
                }
            }
            curItem++;
            amountText.text = (contentAmount - curItem).ToString();
        }
        else
        {
            sendCanTapDialogue = true;
            ResetChestPopup();
        }
    }

    void CreateRewardPopup(int _id, int _amnt)
    {
        if (GameObject.FindGameObjectWithTag("ChestReward"))
        {
            ChestRewardScript _activeReward = GameObject.FindGameObjectWithTag("ChestReward").GetComponent<ChestRewardScript>();
            _activeReward.Wipe();
        }

        ChestRewardScript _instance = Instantiate(chestRewardPrefab, new Vector2(0f, -3f), Quaternion.identity).GetComponent<ChestRewardScript>();
        _instance.SetupReward(_id, _amnt);
        _instance.SetMovePos(new Vector2(0f, 1f));
    }

    void ResetChestPopup()
    {
        activeChest = null;
        contentAmount = 0;
        rewards = Vector3.zero;
        curKingdomID = -1;
        if (rewardCards.Count > 0)
            AssetsManagerScript.instance.SetUpDeckCreatorCards();
        rewardCards.Clear();
        if (GameObject.FindGameObjectWithTag("ChestReward"))
        {
            ChestRewardScript _activeReward = GameObject.FindGameObjectWithTag("ChestReward").GetComponent<ChestRewardScript>();
            _activeReward.Wipe();
        }
        if (GameObject.FindGameObjectWithTag("DisplayCard"))
        {
            ChestCardScript _activeCard = GameObject.FindGameObjectWithTag("DisplayCard").GetComponent<ChestCardScript>();
            _activeCard.Wipe();
        }
        curItem = 0;

        amountText.gameObject.SetActive(false);

        canOpenChest = false;
        canTap = false;

        moveChest = false;

        rewardPopup.SetActive(false);
        chestPopup.SetActive(false);

        StatsManagerScript.instance.SaveGame();
    }
}

[System.Serializable]
public class Chest
{
    [Tooltip("Do not include 'Chest' in the name.")]
    public string chestName = "Unnamed";
    public int animatorID = 0;
    public Vector2 goldMinMax = new Vector2(5, 15);
    public Vector2 gemsMinMax = new Vector2(0, 1);
    [Tooltip("Value from 0 to 1.")]
    public float ticketChance = 0.0f;
    public ChestCard[] possibleCards;
}

[System.Serializable]
public class ChestCard
{
    [Tooltip("Value from 0 to 1.")]
    public float cardChance = 0.5f;
}