using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleManagerScriptBackup : MonoBehaviour
{
    //public static BattleManagerScript instance;
    private StatsManagerScript sms;
    private AssetsManagerScript ams;
    private CameraScript cs;
    private AfterBattleRewardScript abrs;

    public TransitionScript ts;

    public Transform[] activeCardHolders;
    public Transform[] handCardHolders;

    [HideInInspector]
    public CardScript[] activeCards = new CardScript[2];

    [HideInInspector]
    public bool activeBattle = false;
    public GameObject battleCanvas;

    public CardScript cardPrefab;

    private int roundCount = 1;
    public Text roundCountText;

    public GameObject[] UIXs;

    [HideInInspector]
    public CardScript displayCard;
    private bool canTapDisplayCard = false;
    public GameObject screenBlock;

    [HideInInspector]
    public bool canPlayCard = false;

    [Tooltip("Health, Mana, Deck.")]
    public Text[] playerTexts;
    [Tooltip("Health, Mana, Deck.")]
    public Text[] opponentTexts;

    [Header("Player Stats")]
    public int playerHealth = 25;
    public int playerCurHealth = 25;
    public int playerMana = 3;
    private bool canReshuffle = true;
    [HideInInspector]
    public bool playerWins = false;

    [Header("Opponent Stats")]
    public int opponentHealth = 25;
    public int opponentCurHealth = 25;
    public int opponentMana = 3;
    private bool canOpponentReshuffle = true;

    [Header("Opponent Deck")]
    public List<string> opponentDeck = new List<string>();
    public List<string> opponentCurDeck = new List<string>();
    public List<CardScript> opponentCurHand = new List<CardScript>();
    private int opponentDeckCount = 20;

    private int cardPickAttempts = 0;

    [Header("Battle Field")]
    public GameObject battleScene;

    public Transform[] handSlots;

    public Transform[] activeCardSlots;
    public Transform opponentHandSlot;

    public GameObject cardBlockBar;

    private bool roundEnded = true;

    [HideInInspector]
    public Vector3 nextPlayerCardStatChanges;
    public Vector3 nextOpponentCardStatChanges;

    [HideInInspector]
    public DialogueTrigger afterBattleTrigger;

    private int rewardGold = 0;
    private int rewardGems = 0;
    private int rewardChest = -1;
    private string kingdom;

    private void Awake()
    {
        //instance = this;

        sms = StatsManagerScript.instance;
        ams = AssetsManagerScript.instance;
        cs = Camera.main.GetComponent<CameraScript>();
        abrs = GetComponent<AfterBattleRewardScript>();
    }

    private void Update()
    {
        if (canPlayCard)
            cardBlockBar.SetActive(false);
        else
            cardBlockBar.SetActive(true);

        if (canPlayCard)
        {
            if (canReshuffle)
                UIXs[0].SetActive(false);
            else
                UIXs[0].SetActive(true);
            UIXs[1].SetActive(false);
        }
        else
        {
            UIXs[0].SetActive(true);
            UIXs[1].SetActive(true);
        }

        if (displayCard != null)
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                if (canTapDisplayCard)
                {
                    Vector3 _touchPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
                    Vector2 _readPos = new Vector2(_touchPos.x, _touchPos.y);
                    Collider2D _displayCardCollider = displayCard.GetComponent<Collider2D>();
                    if (_displayCardCollider.OverlapPoint(_readPos))
                    {
                        _displayCardCollider.SendMessage("PlayTrigger", "Attack");
                    }
                    else
                    {
                        SetCardDisplay(null);
                        canTapDisplayCard = false;
                        canPlayCard = false;
                        StartCoroutine(DelayCanPlayCard(0.25f));
                    }
                }
                else
                {
                    canTapDisplayCard = true;
                }
            }
        }
    }

    public void SetAfterBattleDialogueTrigger(DialogueTrigger _dialogueTrigger)
    {
        afterBattleTrigger = _dialogueTrigger;
    }

    //TEMP
    public void Win()
    {
        opponentCurHealth = 0;
    }
    //END TEMP

    public void StartNewBattle(List<string> _opponentDeck, int _opponentHealth)
    {
        battleScene.SetActive(true);
        SetOpponentDeck(_opponentDeck);
        CreateHand(1);
        sms.playerCurDeck.Clear();
        for (int i = 0; i < sms.playerDeck.Count; i++)
            sms.playerCurDeck.Add(sms.playerDeck[i]);
        CreateHand(0);
        StartCoroutine(DelayCanPlayCard(0.5f));
        opponentHealth = _opponentHealth;
        opponentCurHealth = opponentHealth;
        playerCurHealth = playerHealth;
        roundCount = 1;
        cardPickAttempts = 0;
        playerMana = 3;
        opponentMana = 3;
        UpdateUI();
        cs.followPlayer = false;
        cs.transform.position = new Vector3(0f, 0f, -10f);
        BoardManagerScript.instance.boardCanvas.SetActive(false);
        sms.statsBar.SetActive(false);
        activeBattle = true;
    }

    //Used to setup the Cards in a player's hand. Used by both players.
    void CreateHand(int _owner)
    {
        if (_owner == 0)
        {
            for (int i = 0; i < handSlots.Length; i++)
            {
                if (sms.playerCurDeck.Count > 0)
                {
                    CardScript _instance = Instantiate(cardPrefab, new Vector2(handSlots[i].position.x, handSlots[i].position.y - 5f), Quaternion.identity);
                    int _cardChoice = Random.Range(0, sms.playerCurDeck.Count);
                    _instance.SetUpCard(sms.playerCurDeck[_cardChoice], 0);
                    sms.playerCurDeck.Remove(sms.playerCurDeck[_cardChoice]);
                    _instance.SetMovePos(handSlots[i].position);
                    _instance.cardSlot = i;
                }
            }
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                if (opponentCurDeck.Count > 0)
                {
                    CardScript _instance = Instantiate(cardPrefab, opponentHandSlot.position, Quaternion.identity);
                    int _cardChoice = Random.Range(0, opponentCurDeck.Count);
                    _instance.SetUpCard(opponentCurDeck[_cardChoice], 1);
                    opponentCurDeck.Remove(opponentCurDeck[_cardChoice]);
                    _instance.SetMovePos(opponentHandSlot.position);
                    opponentCurHand.Add(_instance);
                }
            }
        }
    }

    public void DrawCard (int _owner, int _slot)
    {
        if (_owner == 0)
        {
            if (sms.playerCurDeck.Count > 0)
            {
                CardScript _instance = Instantiate(cardPrefab, new Vector2(handSlots[_slot].position.x, handSlots[_slot].position.y - 10f), Quaternion.identity);
                int _cardChoice = Random.Range(0, sms.playerCurDeck.Count);
                _instance.SetUpCard(sms.playerCurDeck[_cardChoice], 0);
                sms.playerCurDeck.Remove(sms.playerCurDeck[_cardChoice]);
                _instance.SetMovePos(handSlots[_slot].position);
                _instance.cardSlot = _slot;
                if (nextPlayerCardStatChanges != Vector3.zero)
                    _instance.ChangeCardStats(nextPlayerCardStatChanges);
                nextPlayerCardStatChanges = Vector3.zero;
            }
        }
        else
        {
            if (opponentCurDeck.Count > 0)
            {
                CardScript _instance = Instantiate(cardPrefab, opponentHandSlot.position, Quaternion.identity);
                int _cardChoice = Random.Range(0, opponentCurDeck.Count);
                _instance.SetUpCard(opponentCurDeck[_cardChoice], 1);
                opponentCurDeck.Remove(opponentCurDeck[_cardChoice]);
                _instance.SetMovePos(opponentHandSlot.position);
                opponentCurHand.Add(_instance);
                if (nextOpponentCardStatChanges != Vector3.zero)
                    _instance.ChangeCardStats(nextOpponentCardStatChanges);
                nextOpponentCardStatChanges = Vector3.zero;
            }
        }
    }

    public void DrawSpecificCard(int _owner, int _slot, string _cardName)
    {
        if (_owner == 0)
        {
            if (sms.playerCurDeck.Count > 0)
            {
                CardScript _instance = Instantiate(cardPrefab, new Vector2(handSlots[_slot].position.x, handSlots[_slot].position.y - 10f), Quaternion.identity);
                _instance.SetUpCard(_cardName, 0);
                _instance.SetMovePos(handSlots[_slot].position);
                _instance.cardSlot = _slot;
                if (nextPlayerCardStatChanges != Vector3.zero)
                    _instance.ChangeCardStats(nextPlayerCardStatChanges);
                nextPlayerCardStatChanges = Vector3.zero;
            }
        }
        else
        {
            if (opponentCurDeck.Count > 0)
            {
                CardScript _instance = Instantiate(cardPrefab, opponentHandSlot.position, Quaternion.identity);
                _instance.SetUpCard(_cardName, 1);
                _instance.SetMovePos(opponentHandSlot.position);
                opponentCurHand.Add(_instance);
                if (nextOpponentCardStatChanges != Vector3.zero)
                    _instance.ChangeCardStats(nextOpponentCardStatChanges);
                nextOpponentCardStatChanges = Vector3.zero;
            }
        }
    }

    //Used by both players to set the active cards that are played every round.
    public void SetActiveCard(int _slot, CardScript _card)
    {
        activeCards[_slot] = _card;

        if (sms.playerCurDeck.Count == 0)
            ResetDeck(0);
        if (opponentCurDeck.Count == 0)
            ResetDeck(1);
    }

    //Used before the battle to set the opponent's deck to that of each battler.
    public void SetOpponentDeck(List<string> _deck)
    {
        opponentDeck.Clear();
        opponentCurDeck.Clear();
        for (int i = 0; i < _deck.Count; i++)
            opponentDeck.Add(_deck[i]);
        for (int i = 0; i < opponentDeck.Count; i++)
            opponentCurDeck.Add(opponentDeck[i]);
        opponentDeckCount = opponentCurDeck.Count;
    }

    //Starts the next round. Used the moment when the player picks a card to play.
    public void StartRound()
    {
        canPlayCard = false;

        activeCards[0].SetMovePos(activeCardSlots[0].position);
        activeCards[1].SetMovePos(activeCardSlots[1].position);

        StartCoroutine(DelayCardAttacks(1.5f));

        UpdateUI();

        roundEnded = false;
    }

    IEnumerator DelayCardAttacks(float _time)
    {
        yield return new WaitForSeconds(_time * 0.6f);
        activeCards[0].PlayTrigger("Attack");
        yield return new WaitForSeconds(_time * 0.4f);
        activeCards[1].PlayTrigger("Attack");
        activeCards[0].Activate();
        activeCards[1].Activate();

        yield return new WaitForSeconds(2.0f);
        activeCards[0].UpdateCard(1);
        activeCards[1].UpdateCard(1);

        //Checks if cards should continue battling or if there is a winner, or if it ended in a draw.
        if (activeCards[0].healthValue > 0 && activeCards[1].healthValue > 0)
            StartCoroutine(DelayCardAttacks(1.0f));
        else if (activeCards[0].healthValue == 0 && activeCards[1].healthValue == 0)
        {
            //Tie Round Condition
            activeCards[0].Wipe();
            activeCards[1].Wipe();
            StartCoroutine(DelayRoundEnd(1.5f));
        }
        else if (activeCards[0].healthValue > 0 && activeCards[1].healthValue == 0)
        {
            //Player Round Win Condition
            activeCards[1].Wipe();
            activeCards[0].DirectAttack();
            StartCoroutine(DelayRoundEnd(1.5f));
        }
        else if (activeCards[0].healthValue == 0 && activeCards[1].healthValue > 0)
        {
            //Enemy Round Win Condition
            activeCards[0].Wipe();
            activeCards[1].DirectAttack();
            StartCoroutine(DelayRoundEnd(1.5f));
        }
    }

    IEnumerator DelayRoundEnd(float _time)
    {
        yield return new WaitForSeconds(_time);
        EndRound();
    }

    //Used when the round begins. Makes the opponent pick a card to play from his/her hand.
    public void MakeOpponentChoice()
    {
        canPlayCard = false;
        int _cardChoice = 0;
        _cardChoice = Random.Range(0, opponentCurHand.Count);

        for (int k = 0; k < 10; k++)
        {
            _cardChoice = Random.Range(0, opponentCurHand.Count);
            int _cardID = opponentCurHand[_cardChoice].cardID;

            if (opponentMana <= 10)
            {
                for (int i = 0; i < ams.cards[_cardID].limits.Length; i++)
                {
                    //Don't Use With Full Health
                    if (ams.cards[_cardID].limits[i].limit == Limits.WFH && opponentCurHealth == opponentHealth)
                    {
                        continue;
                    }
                    //Don't Use When Opponent Has Less Than Set Health
                    if (ams.cards[_cardID].limits[i].limit == Limits.WOHLTSH && playerCurHealth < ams.cards[_cardID].limits[i].limitValue)
                    {
                        continue;
                    }
                    //Don't Use When Opponent Has Less Than Set Mana
                    if (ams.cards[_cardID].limits[i].limit == Limits.WOHLTSM && playerMana < ams.cards[_cardID].limits[i].limitValue)
                    {
                        continue;
                    }
                    //Don't Use Unless Necessary
                    if (ams.cards[_cardID].limits[i].limit == Limits.UN)
                    {
                        if (cardPickAttempts < 10)
                        {
                            continue;
                        }
                    }
                }
                break;
            }
            else if (opponentMana > 10)
                break;
        }

        if (opponentCurHand[_cardChoice].manaValue <= opponentMana)
        {
            SetActiveCard(1, opponentCurHand[_cardChoice]);
            ChangePlayerMana(1, -opponentCurHand[_cardChoice].manaValue);
            for (int i = 0; i < ams.cards.Length; i++)
            {
                if (ams.cards[i].creatureName == opponentCurHand[_cardChoice].currentCard)
                {
                    if (ams.cards[i].abilities.Length > 0)
                        opponentCurHand[_cardChoice].ShowAbilityBox(true);
                    break;
                }
            }
            opponentCurHand.Remove(opponentCurHand[_cardChoice]);
        }
        else
        {
            CardScript _waitInstance = Instantiate(cardPrefab, opponentHandSlot.position, Quaternion.identity);
            _waitInstance.SetUpCard("Wait", 1);
            SetActiveCard(1, _waitInstance);
        }

        cardPickAttempts = 0;
        StartRound();
        activeCards[0].FirstActivation();
        activeCards[1].FirstActivation();
    }

    public void ChangePlayerHealth(int _owner, int _amnt)
    {
        if (_owner == 0)
        {
            playerCurHealth += _amnt;
        }
        else
        {
            opponentCurHealth += _amnt;
        }
    }

    public void ChangePlayerMana(int _owner, int _amnt)
    {
        if (_owner == 0)
        {
            playerMana += _amnt;
        }
        else
        {
            opponentMana += _amnt;
        }
    }

    public void DisableCardAbility(int _cardOwner)
    {
        activeCards[_cardOwner].canUseAbility = false;
    }

    public void EndRound()
    {
        if (!roundEnded)
        {
            if (roundCount <= 5)
            {
                ChangePlayerMana(0, 2);
                ChangePlayerMana(1, 2);
            }
            else
            {
                ChangePlayerMana(0, 3);
                ChangePlayerMana(1, 3);
            }
            roundCount++;
            UpdateUI();
            StartCoroutine(DelayCanPlayCard(1.5f));
            roundEnded = true;
            canReshuffle = true;
            canOpponentReshuffle = true;

            if (playerCurHealth == 0 && opponentCurHealth > 0)
            {
                playerWins = false;
                StartCoroutine(DelayBattleEnd());
            }
            else if (opponentCurHealth == 0 && playerCurHealth > 0)
            {
                playerWins = true;
                StartCoroutine(DelayBattleEnd());
            }
            else if (playerCurHealth == 0 && opponentCurHealth == 0)
            {
                playerWins = false;
                StartCoroutine(DelayBattleEnd());
            }
        }
    }

    //Used to set the rewards the player will earn upon defeating this opponent.
    public void SetRewards(int _rewardGold, int _rewardGems, int _rewardChest, string _kingdom)
    {
        rewardGold = _rewardGold;
        rewardGems = _rewardGems;
        rewardChest = _rewardChest;
        kingdom = _kingdom;
    }

    //Used when either player reaches 0 health to end the battle, or when the player leaves the battle early. Activated with the Coroutine "DelayBattleEnd."
    private void EndBattle()
    {
        GameObject[] playerCards = GameObject.FindGameObjectsWithTag("PlayerCard");
        foreach (GameObject _card in playerCards)
            Destroy(_card);
        foreach (CardScript _card in opponentCurHand)
            Destroy(_card.gameObject);
        opponentCurHand.Clear();
        battleScene.SetActive(false);
        BoardManagerScript.instance.boardCanvas.SetActive(true);

        if (playerWins)
        {
            if (sms.isConnected)
            {
                if (rewardGold > 0)
                    PlayFabController.instance.AddGold(rewardGold);
                if (rewardGems > 0)
                    PlayFabController.instance.AddGems(rewardGems);

                abrs.ShowRewards(new Vector3(rewardGold, rewardGems, 0), rewardChest, kingdom);
            }
        }

        sms.statsBar.SetActive(true);
        cs.followPlayer = true;

        activeBattle = false;
    }

    //Used to end an active battle. Do not use "End Battle" to do so but instead use this Coroutine.
    IEnumerator DelayBattleEnd()
    {
        canPlayCard = false;
        yield return new WaitForSeconds(0.75f);
        GameObject.Find("TransitionCanvas").GetComponent<TransitionScript>().EndBattleTransition();
        yield return new WaitForSeconds(1f);
        EndBattle();
    }

    public void UpdateUI()
    {
        if (playerCurHealth > playerHealth)
            playerCurHealth = playerHealth;
        if (playerCurHealth < 0)
            playerCurHealth = 0;
        if (opponentCurHealth > opponentHealth)
            opponentCurHealth = opponentHealth;
        if (opponentCurHealth < 0)
            opponentCurHealth = 0;
        if (playerMana > 12)
            playerMana = 12;
        if (playerMana < 0)
            playerMana = 0;
        if (opponentMana > 12)
            opponentMana = 12;
        if (opponentMana < 0)
            opponentMana = 0;

        playerTexts[0].text = playerCurHealth + "/" + playerHealth;
        playerTexts[1].text = playerMana + "/12";
        playerTexts[2].text = sms.playerCurDeck.Count + "/20";

        opponentTexts[0].text = opponentCurHealth + "/" + opponentHealth;
        opponentTexts[1].text = opponentMana + "/12";
        opponentTexts[2].text = opponentCurDeck.Count + "/" + opponentDeckCount;

        roundCountText.text = "Round: " + roundCount;
    }

    //Allows the player to use the Wait Spell.
    public void Wait()
    {
        if (canPlayCard)
        {
            CardScript _waitInstance = Instantiate(cardPrefab, new Vector2(0f, handSlots[1].position.y - 5f), Quaternion.identity);
            _waitInstance.SetUpCard("Wait", 0);
            SetActiveCard(0, _waitInstance);
            MakeOpponentChoice();
        }
    }

    //Used to Reshuffle either of the player's decks.
    public void Reshuffle(int _deckOwner)
    {
        if (canPlayCard)
        {
            if (_deckOwner == 0)
            {
                if (playerMana > 0 && canReshuffle)
                {
                    canPlayCard = false;
                    canReshuffle = false;
                    ChangePlayerMana(0, -1);

                    GameObject[] _playerHand = GameObject.FindGameObjectsWithTag("PlayerCard");
                    foreach (GameObject _card in _playerHand)
                    {
                        CardScript _cs = _card.GetComponent<CardScript>();
                        sms.playerCurDeck.Add(_cs.currentCard);
                        _cs.InstantWipe();
                    }

                    StartCoroutine(DelayCanPlayCard(1.0f));
                }
            }
            else
            {
                if (opponentMana > 0 && canOpponentReshuffle)
                {
                    canPlayCard = false;
                    canOpponentReshuffle = false;
                    ChangePlayerMana(1, -1);

                    foreach (CardScript _card in opponentCurHand)
                    {
                        opponentCurDeck.Add(_card.currentCard);
                        _card.InstantWipe();
                    }

                    StartCoroutine(DelayCanPlayCard(1.0f));
                }
            }
        }
    }

    //This function is used when a player runs out of cards and has to reshuffle their full decks.
    void ResetDeck (int _deckOwner)
    {
        if (_deckOwner == 0)
        {
            for (int i = 0; i < sms.playerDeck.Count; i++)
            {
                sms.playerCurDeck.Add(sms.playerDeck[i]);
            }
        }
        else
        {
            for (int i = 0; i < opponentDeck.Count; i++)
            {
                opponentCurDeck.Add(opponentDeck[i]);
            }
        }
    }

    IEnumerator DelayCanPlayCard(float _time)
    {
        yield return new WaitForSeconds(_time);
        canPlayCard = true;
        UpdateUI();
    }

    //Used when a card is held down long enough to show a Display of it.
    public void SetCardDisplay(CardScript _card)
    {
        if (_card != null)
        {
            canPlayCard = false;
            screenBlock.SetActive(true);
        }
        else
        {
            displayCard.InstantWipe();
            canPlayCard = true;
            screenBlock.SetActive(false);
        }
        displayCard = _card;
    }

    //Resets all stats back to the defaults after a battle so that another battle can be started at any time.
    void ResetGame()
    {
        opponentDeck.Clear();
        opponentCurDeck.Clear();
        sms.playerCurDeck.Clear();

        cardPickAttempts = 0;
    }
}