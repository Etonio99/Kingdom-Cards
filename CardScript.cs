using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardScript : MonoBehaviour
{
    [Header("Base Attributes")]
    public string currentCard;
    public int cardOwner = 0; //0 == Player, 1 == Opponent
    [HideInInspector]
    public int cardID = 0;
    private AssetsManagerScript ams;
    private BattleManagerScript bms;
    private BoxCollider2D m_Collider2D;
    [HideInInspector]
    public int cardSlot = 0;
    private bool disabled = false;

    [Header("Base Stats")]
    public int healthValue;
    public int manaValue;
    public int attackValue;

    [Header("UI Setup")]
    private SpriteRenderer cardSpriteRenderer;
    public SpriteRenderer cardBgSpriteRenderer;
    public Text nameText;
    public SpriteRenderer[] statSpriteRenderers;
    public SpriteRenderer creatureSpriteRenderer;
    private Animator creatureAnimator;
    public SpriteRenderer elementSpriteRenderer;
    public SpriteRenderer scrollSpriteRenderer;

    public Transform abilityBox;
    public Text[] abilityTexts;
    private Vector2 abilityBoxMovePos;
    private bool showAbilityBox = false;
    private bool activatedAbility = false;
    //[HideInInspector]
    public bool canUseAbility = true;

    private float touchHoldTime = 0f;

    private Vector2 movePos;

    private Vector2 nonCreatureSpritePos = new Vector2(0f, 1.6875f);

    //If this string has a creature name in it, that card will be placed into the card owner's hand after this card has been wiped.
    private string specificCardToPlace = "";

    private void Awake()
    {
        ams = AssetsManagerScript.instance;
        bms = BattleManagerScript.instance;
        m_Collider2D = GetComponent<BoxCollider2D>();
        cardSpriteRenderer = GetComponent<SpriteRenderer>();
        creatureAnimator = creatureSpriteRenderer.GetComponent<Animator>();
    }

    private void Start()
    {
        UpdateCard(0);
    }

    private void Update()
    {
        float _interpolation = 5f * Time.deltaTime;
        Vector2 _newPos = Vector2.Lerp(transform.position, movePos, _interpolation);
        transform.position = _newPos;

        if (cardOwner == 0)
            CheckForTouch();

        Vector2 _newAbilityBoxPos = Vector2.Lerp(abilityBox.localPosition, abilityBoxMovePos, _interpolation);
        abilityBox.localPosition = _newAbilityBoxPos;
    }

    public void ShowAbilityBox (bool _setting)
    {
        if (_setting)
            abilityBoxMovePos = new Vector2(3.75f, 0f);
        else
            abilityBoxMovePos = Vector2.zero;
    }

    public void UpdateCard(int _option)
    {
        if (_option == 0)
        {
            //0 = Update all stats, used when card is created.

            for (int i = 0; i < ams.cards.Length; i++)
            {
                if (ams.cards[i].creatureName == currentCard)
                {
                    cardID = i;
                    break;
                }
            }

            nameText.text = ams.cards[cardID].creatureName;
            statSpriteRenderers[0].sprite = ams.numberSprites[ams.cards[cardID].manaValue];
            manaValue = ams.cards[cardID].manaValue;
            statSpriteRenderers[1].sprite = ams.numberSprites[ams.cards[cardID].attackValue];
            attackValue = ams.cards[cardID].attackValue;
            statSpriteRenderers[2].sprite = ams.numberSprites[ams.cards[cardID].healthValue];
            healthValue = ams.cards[cardID].healthValue;

            creatureSpriteRenderer.sprite = ams.cards[cardID].creatureSprite;
            creatureAnimator.runtimeAnimatorController = ams.cards[cardID].creatureAnimator;

            abilityTexts[0].text = ams.cards[cardID].abilityName;
            abilityTexts[1].text = ams.cards[cardID].abilityDescription;
            if (cardOwner == 2)
                ShowAbilityBox(true);

            if (ams.cards[cardID].element > 0)
            {
                scrollSpriteRenderer.sprite = ams.scrollSprites[1];
                elementSpriteRenderer.sprite = ams.elementSprites[(int)ams.cards[cardID].element - 1];
            }

            //Set the Card Style.
            if (ams.cards[cardID].cardBack == CardBacks.Spell || ams.cards[cardID].cardBack == CardBacks.Structure)
            {
                if (cardOwner != 1)
                {
                    //Player
                    if (ams.cards[cardID].cardBack == CardBacks.Spell)
                        cardSpriteRenderer.sprite = ams.spellCardSprites[StatsManagerScript.instance.curCardBack];
                    else
                        cardSpriteRenderer.sprite = ams.structureCardSprites[StatsManagerScript.instance.curCardBack];
                }
                else
                {
                    //Opponent
                    if (ams.cards[cardID].cardBack == CardBacks.Spell)
                        cardSpriteRenderer.sprite = ams.spellCardSprites[BattleManagerScript.instance.opponentCardBack];
                    else
                        cardSpriteRenderer.sprite = ams.structureCardSprites[BattleManagerScript.instance.opponentCardBack];
                }
                cardBgSpriteRenderer.enabled = false;
            }
            else
            {
                if (cardOwner != 1)
                    cardSpriteRenderer.sprite = ams.creatureCardSprites[StatsManagerScript.instance.curCardBack];
                else
                    cardSpriteRenderer.sprite = ams.creatureCardSprites[BattleManagerScript.instance.opponentCardBack];
                cardBgSpriteRenderer.sprite = ams.cardBgSprites[(int)ams.cards[cardID].cardBack];
            }
        }
        else if (_option == 1)
        {
            //1 = Update only certain stats, used during the attack phase.

            if (attackValue < 0)
                attackValue = 0;
            if (manaValue < 0)
                manaValue = 0;
            if (healthValue < 0)
                healthValue = 0;

            statSpriteRenderers[0].sprite = ams.numberSprites[manaValue];
            statSpriteRenderers[1].sprite = ams.numberSprites[attackValue];
            statSpriteRenderers[2].sprite = ams.numberSprites[healthValue];
        }
    }

    public void SetUpCard(string _creatureName, int _owner)
    {
        currentCard = _creatureName;
        cardOwner = _owner;

        for (int i = 0; i < ams.cards.Length; i++)
        {
            if (ams.cards[i].creatureName == _creatureName)
            {
                cardID = i;
                break;
            }
        }

        if (_owner == 0)
        {
            transform.name = "Player Card: " + currentCard;
        }
        else if (_owner == 1)
        {
            transform.tag = "OpponentCard";
            transform.name = "Opponent Card: " + currentCard;
        }
        else if (_owner == 2)
        {
            transform.tag = "DisplayCard";
            cardSpriteRenderer.sortingLayerName = "BattleEffects";
            cardBgSpriteRenderer.sortingLayerName = "BattleEffects";
            creatureSpriteRenderer.sortingLayerName = "BattleEffects";
            statSpriteRenderers[0].sortingLayerName = "BattleEffects";
            statSpriteRenderers[1].sortingLayerName = "BattleEffects";
            statSpriteRenderers[2].sortingLayerName = "BattleEffects";
            scrollSpriteRenderer.sortingLayerName = "BattleEffects";
            elementSpriteRenderer.sortingLayerName = "BattleEffects";
            abilityBox.GetComponent<SpriteRenderer>().sortingLayerName = "BattleEffects";
            abilityBox.Find("AbilityBoxCanvas").GetComponent<Canvas>().sortingLayerName = "BattleEffects";
            transform.Find("CardCanvas").GetComponent<Canvas>().sortingLayerName = "BattleEffects";
            transform.Find("CardCanvas").Find("TapToExitText").gameObject.SetActive(true);
            disabled = true;
        }

        if (ams.cards[cardID].cardBack == CardBacks.Spell || ams.cards[cardID].cardBack == CardBacks.Structure)
        {
            creatureSpriteRenderer.transform.localPosition = nonCreatureSpritePos;
            scrollSpriteRenderer.enabled = false;
            statSpriteRenderers[1].enabled = false;
            statSpriteRenderers[2].enabled = false;
        }

        UpdateCard(0);
    }

    public void SetMovePos(Vector2 _pos)
    {
        movePos = _pos;
    }

    //Check to see if this card is tapped by the player.
    private bool CheckForTouch()
    {
        if (cardOwner == 0)
        {
            if (Input.touchCount > 0)
            {
                Vector3 _touchPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
                Vector2 _newTouchPos = new Vector2(_touchPos.x, _touchPos.y);

                //Check if the player is holding down on the card to view it's info.
                if (Input.GetTouch(0).phase == TouchPhase.Stationary)
                {
                    touchHoldTime += Time.deltaTime;

                    if (touchHoldTime > 0.7f && m_Collider2D == Physics2D.OverlapPoint(_newTouchPos) && !bms.displayCard && bms.canPlayCard)
                    {
                        CardScript _displayCard = Instantiate(bms.cardPrefab, new Vector2(-10f, 0f), Quaternion.identity);
                        _displayCard.SetMovePos(Vector2.zero);
                        _displayCard.SetUpCard(currentCard, 2);
                        touchHoldTime = 0.0f;
                        bms.SetCardDisplay(_displayCard);
                    }
                }

                //Check if the player is tapping the card to send it to battle.
                if (Input.GetTouch(0).phase == TouchPhase.Ended)
                {
                    touchHoldTime = 0.0f;

                    if (m_Collider2D == Physics2D.OverlapPoint(_newTouchPos) && bms.canPlayCard && touchHoldTime != 0.7f)
                    {
                        if (bms.playerMana >= manaValue && !disabled)
                        {
                            BattleManagerScript.instance.ChangePlayerMana(0, -manaValue);
                            bms.SetActiveCard(0, this);
                            bms.MakeOpponentChoice();
                            disabled = true;

                            if (ams.cards[cardID].abilities.Length > 0)
                                ShowAbilityBox(true);
                        }
                    }
                }
            }
        }
        return false;
    }

    //This function is only activated once when the cards are picked to be played. It is used for abilities that involve the cards entry.
    public void FirstActivation()
    {
        StartCoroutine(DelayFirstActivation());

        if (canUseAbility)
        {
            for (int i = 0; i < ams.cards[cardID].abilities.Length; i++)
            {
                //Disable Ability of Opponent's Card.
                if (ams.cards[cardID].abilities[i].ability == AbilityType.DAOOC)
                    bms.DisableCardAbility(1 - cardOwner);
            }
        }
    }

    IEnumerator DelayFirstActivation()
    {
        yield return new WaitForSeconds(0.01f);
        if (canUseAbility)
        {
            for (int i = 0; i < ams.cards[cardID].abilities.Length; i++)
            {
                //Change Player Health on Entry
                if (ams.cards[cardID].abilities[i].ability == AbilityType.CPHOE)
                    bms.ChangePlayerHealth(cardOwner, ams.cards[cardID].abilities[i].abilityValue);
                //Change Opponent Health on Entry
                if (ams.cards[cardID].abilities[i].ability == AbilityType.COHOE)
                    bms.ChangePlayerHealth(1 - cardOwner, ams.cards[cardID].abilities[i].abilityValue);
                //Change Player Mana on Entry
                if (ams.cards[cardID].abilities[i].ability == AbilityType.CPMOE)
                    bms.ChangePlayerMana(cardOwner, ams.cards[cardID].abilities[i].abilityValue);
                //Change Opponent Mana on Entry
                if (ams.cards[cardID].abilities[i].ability == AbilityType.COMOE)
                    bms.ChangePlayerMana(1 - cardOwner, ams.cards[cardID].abilities[i].abilityValue);
            }
        }
    }

    //Used during the attack phase of a round. Card Activates is abilities and deals damage to the other.
    public void Activate()
    {
        if (canUseAbility)
        {
            for (int i = 0; i < ams.cards[cardID].abilities.Length; i++)
            {
                //Change Player Health on Attack
                if (ams.cards[cardID].abilities[i].ability == AbilityType.CPHOA)
                    bms.ChangePlayerHealth(cardOwner, ams.cards[cardID].abilities[i].abilityValue);
                //Change Opponent Health on Attack
                if (ams.cards[cardID].abilities[i].ability == AbilityType.COHOA)
                    bms.ChangePlayerHealth(1 - cardOwner, ams.cards[cardID].abilities[i].abilityValue);
                //Change Player Mana on Attack
                if (ams.cards[cardID].abilities[i].ability == AbilityType.CPMOA)
                    bms.ChangePlayerMana(cardOwner, ams.cards[cardID].abilities[i].abilityValue);
                //Change Opponent Mana on Attack
                if (ams.cards[cardID].abilities[i].ability == AbilityType.COMOA)
                    bms.ChangePlayerMana(1 - cardOwner, ams.cards[cardID].abilities[i].abilityValue);
                //Steal Card From Opponent Deck
                if (ams.cards[cardID].abilities[i].ability == AbilityType.SCFOD)
                {
                    if (cardOwner == 0)
                    {
                        if (bms.opponentCurDeck.Count > 0)
                        {
                            int _cardChoice = Random.Range(0, bms.opponentCurDeck.Count);
                            specificCardToPlace = bms.opponentCurDeck[_cardChoice];
                            bms.opponentCurDeck.Remove(bms.opponentCurDeck[_cardChoice]);
                            bms.nextPlayerCardStatChanges += ams.cards[cardID].abilities[i].cardChangeValues;
                        }
                    }
                    else if (cardOwner == 1)
                    {
                        if (StatsManagerScript.instance.playerCurDeck.Count > 0)
                        {
                            int _cardChoice = Random.Range(0, StatsManagerScript.instance.playerCurDeck.Count);
                            specificCardToPlace = StatsManagerScript.instance.playerCurDeck[_cardChoice];
                            StatsManagerScript.instance.playerCurDeck.Remove(StatsManagerScript.instance.playerCurDeck[_cardChoice]);
                            bms.nextOpponentCardStatChanges += ams.cards[cardID].abilities[i].cardChangeValues;
                        }
                    }
                }
                //Change Next Player Card's Stats
                if (ams.cards[cardID].abilities[i].ability == AbilityType.CNPCS)
                {
                    if (cardOwner == 0)
                        bms.nextPlayerCardStatChanges += ams.cards[cardID].abilities[i].cardChangeValues;
                    else if (cardOwner == 1)
                        bms.nextOpponentCardStatChanges += ams.cards[cardID].abilities[i].cardChangeValues;
                }
                //Change Next Opponent Card's Stats
                if (ams.cards[cardID].abilities[i].ability == AbilityType.CNOCS)
                {
                    if (cardOwner == 1)
                        bms.nextPlayerCardStatChanges += ams.cards[cardID].abilities[i].cardChangeValues;
                    else if (cardOwner == 0)
                        bms.nextOpponentCardStatChanges += ams.cards[cardID].abilities[i].cardChangeValues;
                }
                //Place Card Into Player Deck On Attack
                if (ams.cards[cardID].abilities[i].ability == AbilityType.PCIPDOA)
                {
                    for (int j = 0; j < ams.cards[cardID].abilities[i].abilityValue; j++)
                    {
                        if (cardOwner == 0)
                            StatsManagerScript.instance.playerCurDeck.Add(ams.cards[cardID].abilities[i].stringValue);
                        else if (cardOwner == 1)
                            bms.opponentCurDeck.Add(ams.cards[cardID].abilities[i].stringValue);
                    }
                }
                //Place Card Into Opponent Deck On Attack
                if (ams.cards[cardID].abilities[i].ability == AbilityType.PCIODOA)
                {
                    for (int j = 0; j < ams.cards[cardID].abilities[i].abilityValue; j++)
                    {
                        if (cardOwner == 0)
                            bms.opponentCurDeck.Add(ams.cards[cardID].abilities[i].stringValue);
                        else if (cardOwner == 1)
                            StatsManagerScript.instance.playerCurDeck.Add(ams.cards[cardID].abilities[i].stringValue);
                    }
                }
                //Change Card Health on Attack
                if (ams.cards[cardID].abilities[i].ability == AbilityType.CCHOA)
                    healthValue += ams.cards[cardID].abilities[i].abilityValue;
            }
        }
        bms.activeCards[1 - cardOwner].ChangeCardHealth(-attackValue);
    }

    public void DirectAttack()
    {
        PlayTrigger("Attack");
        if (canUseAbility)
        {
            for (int i = 0; i < ams.cards[cardID].abilities.Length; i++)
            {
                //Change Player Health on Direct Attack
                if (ams.cards[cardID].abilities[i].ability == AbilityType.CPHODA)
                    bms.ChangePlayerHealth(cardOwner, ams.cards[cardID].abilities[i].abilityValue);
                //Change Opponent Health on Direct Attack
                if (ams.cards[cardID].abilities[i].ability == AbilityType.COHODA)
                    bms.ChangePlayerHealth(1 - cardOwner, ams.cards[cardID].abilities[i].abilityValue);
                //Change Player Mana on Direct Attack
                if (ams.cards[cardID].abilities[i].ability == AbilityType.CPMODA)
                    bms.ChangePlayerMana(cardOwner, ams.cards[cardID].abilities[i].abilityValue);
                //Change Opponent Mana on Direct Attack
                if (ams.cards[cardID].abilities[i].ability == AbilityType.COMODA)
                    bms.ChangePlayerMana(1 - cardOwner, ams.cards[cardID].abilities[i].abilityValue);
            }
        }
        bms.ChangePlayerHealth(1 - cardOwner, -attackValue);
        Wipe();
    }

    //Used to change this card's health stat.
    public void ChangeCardHealth(int _amnt)
    {
        if (ams.cards[cardID].cardBack != CardBacks.Spell || ams.cards[cardID].cardBack != CardBacks.Structure)
            healthValue += _amnt;
    }

    public void ChangeCardStats(Vector3 _stats)
    {
        StartCoroutine("DelayChangeCardStats", _stats);
    }

    IEnumerator DelayChangeCardStats (Vector3 _stats)
    {
        yield return null;
        //x = Mana, y = Attack, z = Health.
        manaValue += (int)Mathf.Ceil(_stats.x);
        if (manaValue < 0)
            manaValue = 0;
        if (_stats.x != 0)
            statSpriteRenderers[0].color = Color.cyan;
        if (ams.cards[cardID].cardBack != CardBacks.Spell || ams.cards[cardID].cardBack != CardBacks.Structure)
        {
            attackValue += (int)Mathf.Ceil(_stats.y);
            if (attackValue < 0)
                attackValue = 0;
            if (_stats.y != 0)
                statSpriteRenderers[1].color = Color.cyan;
            healthValue += (int)Mathf.Ceil(_stats.z);
            if (healthValue < 0)
                healthValue = 0;
            if (_stats.z != 0)
                statSpriteRenderers[2].color = Color.cyan;
        }

        UpdateCard(1);
    }

    //Used to set triggers on the sprite's animator and play animations.
    public void PlayTrigger (string _trigger)
    {
        creatureAnimator.SetTrigger(_trigger);
    }

    //Delay Wipe is used when this card deals a Direct Attack and stays on the screen for a bit longer than normal.
    //This also resets the round to allow the players to pick new cards.
    IEnumerator DelayWipe(float _time)
    {
        yield return new WaitForSeconds(_time);
        if (cardOwner != 2)
            bms.UpdateUI();

        ShowAbilityBox(false);
        if (currentCard != "Wait" && cardOwner != 2)
        {
            if (specificCardToPlace == "")
                bms.DrawCard(cardOwner, cardSlot);
            else
                bms.DrawSpecificCard(cardOwner, cardSlot, specificCardToPlace);
        }
        if (cardOwner != 2)
            bms.SetActiveCard(cardOwner, null);
        if (cardOwner == 0)
            SetMovePos(new Vector2(transform.position.x, -17f));
        else if (cardOwner == 1)
            SetMovePos(new Vector2(transform.position.x, 18f));
        else if (cardOwner == 2)
            SetMovePos(new Vector2(10f, 0f));
        Destroy(gameObject, 1.0f);
    }

    //Used to remove the card after a round has ended.
    public void Wipe()
    {
        tag = "Untagged";
        if (healthValue == 0)
            PlayTrigger("Die");
        StartCoroutine(DelayWipe(1.5f));
    }

    public void InstantWipe()
    {
        tag = "Untagged";
        StartCoroutine(DelayWipe(0.05f));
    }
}