using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChestCardScript : MonoBehaviour
{
    [Header("Base Attributes")]
    public string currentCard;
    [HideInInspector]
    public int cardID = 0;
    private AssetsManagerScript ams;

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

    private Vector2 movePos;

    public GameObject goldReturnTag;
    public Text ownedText;

    private Vector2 nonCreatureSpritePos = new Vector2(0f, 1.6875f);

    private void Awake()
    {
        ams = AssetsManagerScript.instance;
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
            if (ams.cards[cardID].abilityName != "No Ability")
                ShowAbilityBox(true);

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

        transform.tag = "DisplayCard";

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

    public void ShowGoldReturn()
    {
        goldReturnTag.SetActive(true);
    }

    //Used to remove the card after a round has ended.
    public void Wipe()
    {
        tag = "Untagged";
        Destroy(gameObject);
    }
}