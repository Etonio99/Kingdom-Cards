using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckCreatorCardScript : MonoBehaviour
{
    [Header("Base Attributes")]
    public string currentCard;
    [HideInInspector]
    public int cardID = 0;
    private AssetsManagerScript ams;
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
    [Tooltip("Mana, Attack, Health")]
    public SpriteRenderer[] statSpriteRenderers;
    public SpriteRenderer creatureSpriteRenderer;
    private Animator creatureAnimator;
    public SpriteRenderer elementSpriteRenderer;
    public SpriteRenderer scrollSpriteRenderer;

    private int ownedAmount = 0;
    private int amountAvailable = 0;
    public SpriteRenderer amountSpriteRenderer;

    public Transform abilityBox;
    public Text[] abilityTexts;
    private Vector2 abilityBoxMovePos;
    private bool showAbilityBox = false;
    private bool activatedAbility = false;

    private float touchHoldTime = 0f;

    private Vector2 movePos;

    private Vector2 creatureSpritePos = new Vector2(0.5624988f, 1.6875f);
    private Vector2 nonCreatureSpritePos = new Vector2(0f, 1.6875f);

    private bool removeMode = false;

    private bool skipEntryAnim = false;

    //If this string has a creature name in it, that card will be placed into the card owner's hand after this card has been wiped.
    private string specificCardToPlace = "";

    private void Awake()
    {
        ams = AssetsManagerScript.instance;
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

            if (ams.cards[cardID].element > 0)
            {
                scrollSpriteRenderer.sprite = ams.scrollSprites[1];
                elementSpriteRenderer.sprite = ams.elementSprites[(int)ams.cards[cardID].element - 1];
            }

            if (ams.cards[cardID].cardBack == CardBacks.Spell || ams.cards[cardID].cardBack == CardBacks.Structure)
            {
                if (ams.cards[cardID].cardBack == CardBacks.Spell)
                    cardSpriteRenderer.sprite = ams.spellCardSprites[StatsManagerScript.instance.curCardBack];
                else
                    cardSpriteRenderer.sprite = ams.structureCardSprites[StatsManagerScript.instance.curCardBack];
                cardBgSpriteRenderer.enabled = false;
            }
            else
            {
                cardSpriteRenderer.sprite = ams.creatureCardSprites[StatsManagerScript.instance.curCardBack];
                cardBgSpriteRenderer.sprite = ams.cardBgSprites[(int)ams.cards[cardID].cardBack];
            }

            int _amountInDeck = 0;
            for (int i = 0; i < StatsManagerScript.instance.playerDeck.Count; i++)
            {
                if (StatsManagerScript.instance.playerDeck[i] == currentCard)
                    _amountInDeck++;
            }
            ownedAmount = StatsManagerScript.instance.unlockedCards[cardID];
            amountAvailable = ownedAmount - _amountInDeck;
            amountSpriteRenderer.sprite = ams.numberSprites[amountAvailable];

            if (skipEntryAnim)
                PlayTrigger("SkipToIdle");

            UpdateCard(1);
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

            if (amountAvailable == 0 && !removeMode)
            {
                cardSpriteRenderer.color = Color.gray;
                scrollSpriteRenderer.color = Color.gray;
                creatureSpriteRenderer.color = Color.gray;
                foreach (SpriteRenderer _sr in statSpriteRenderers)
                {
                    _sr.color = Color.gray;
                }
                nameText.color = Color.gray;
                elementSpriteRenderer.color = Color.gray;
            }
            else
            {
                cardSpriteRenderer.color = Color.white;
                scrollSpriteRenderer.color = Color.white;
                creatureSpriteRenderer.color = Color.white;
                foreach (SpriteRenderer _sr in statSpriteRenderers)
                {
                    _sr.color = Color.white;
                }
                nameText.color = Color.white;
                elementSpriteRenderer.color = Color.white;
            }

            if (removeMode)
                amountSpriteRenderer.transform.parent.gameObject.SetActive(false);
            else
                amountSpriteRenderer.sprite = ams.numberSprites[amountAvailable];
        }
    }

    public void SetUpCard(string _creatureName)
    {
        currentCard = _creatureName;

        for (int i = 0; i < ams.cards.Length; i++)
        {
            if (ams.cards[i].creatureName == _creatureName)
            {
                cardID = i;
                break;
            }
        }

        transform.name = "Card: " + currentCard;

        if (ams.cards[cardID].cardBack == CardBacks.Spell || ams.cards[cardID].cardBack == CardBacks.Structure)
        {
            creatureSpriteRenderer.transform.localPosition = nonCreatureSpritePos;
            scrollSpriteRenderer.enabled = false;
            statSpriteRenderers[1].enabled = false;
            statSpriteRenderers[2].enabled = false;
            elementSpriteRenderer.enabled = false;
        }
        else
        {
            creatureSpriteRenderer.transform.localPosition = creatureSpritePos;
            scrollSpriteRenderer.enabled = true;
            statSpriteRenderers[1].enabled = true;
            statSpriteRenderers[2].enabled = true;
            if (ams.cards[cardID].element != Element.None)
                elementSpriteRenderer.enabled = true;
            else
                elementSpriteRenderer.enabled = false;
        }

        UpdateCard(0);
    }

    void SkipEntryAnim(bool _setting)
    {
        skipEntryAnim = _setting;
    }

    void RemoveMode()
    {
        removeMode = true;
    }

    public void SetMovePos(Vector2 _pos)
    {
        movePos = _pos;
    }

    //Check to see if this card is tapped by the player.
    private bool CheckForTouch()
    {
        if (Input.touchCount > 0)
        {
            Vector3 _touchPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
            Vector2 _newTouchPos = new Vector2(_touchPos.x, _touchPos.y);

            //Check if the player is holding down on the card to view it's info.
            if (Input.GetTouch(0).phase == TouchPhase.Stationary)
            {
                touchHoldTime += Time.deltaTime;

                if (touchHoldTime > 0.7f && m_Collider2D == Physics2D.OverlapPoint(_newTouchPos) && !DeckCreatorManagerScript.instance.displayCard)
                {
                    CardScript _displayCard = Instantiate(DeckCreatorManagerScript.instance.battleCardPrefab, new Vector2(-10f, 0f), Quaternion.identity);
                    _displayCard.SetMovePos(Vector2.zero);
                    _displayCard.SetUpCard(currentCard, 2);
                    touchHoldTime = 0.0f;
                    DeckCreatorManagerScript.instance.SetCardDisplay(_displayCard);
                }
            }

            //Check if the player is tapping the card to send it to battle.
            if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                Collider2D _cardCollider = GetComponent<Collider2D>();
                if (_cardCollider.OverlapPoint(_newTouchPos))
                {
                    if (DeckCreatorManagerScript.instance.canInteract)
                    {
                        if (removeMode)
                        {
                            StatsManagerScript.instance.playerDeck.Remove(currentCard);
                            ams.UpdatePlayerDeck();
                            StatsManagerScript.instance.UpdateSavedDecks();
                            DeckCreatorManagerScript.instance.UpdateCards();
                        }
                        else
                        {
                            if (amountAvailable > 0 && StatsManagerScript.instance.playerDeck.Count < 20)
                            {
                                amountAvailable--;
                                StatsManagerScript.instance.playerDeck.Add(currentCard);
                                ams.UpdatePlayerDeck();
                                StatsManagerScript.instance.UpdateSavedDecks();
                                DeckCreatorManagerScript.instance.UpdateUI();
                                UpdateCard(1);
                            }
                        }
                    }
                }
                touchHoldTime = 0.0f;
            }
        }
        return false;
    }

    //Used to set triggers on the sprite's animator and play animations.
    public void PlayTrigger(string _trigger)
    {
        creatureAnimator.SetTrigger(_trigger);
    }

    void Wipe()
    {
        SetMovePos(new Vector2(transform.position.x, transform.position.y + Camera.main.orthographicSize * 2.0f));
        Destroy(gameObject, 1.0f);
    }
}