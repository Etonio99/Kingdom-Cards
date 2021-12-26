using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class DeckCreatorManagerScript : MonoBehaviour
{
    public static DeckCreatorManagerScript instance;
    private AssetsManagerScript ams;
    private StatsManagerScript sms;

    public DeckCreatorCardScript deckCreatorCardPrefab;
    public CardScript battleCardPrefab;
    [HideInInspector]
    public CardScript displayCard;
    private bool canTapDisplayCard = false;
    public GameObject screenBlock;
    private int page = 0;
    private int totalPages = 0;
    public Text pagesText;
    [HideInInspector]
    public bool canInteract = true;
    public Transform[] cardHoldSlots;
    public Text cardCountText;
    private bool showingDeck = false;
    public Sprite[] viewButtonSprites;
    public Image viewButtonImage;
    private int amountOfUnlockedCards = 0;

    public Transform cardHolders;
    public float[] cardHolderHeights;

    public GameObject noCardsText;
    public Text viewText;

    public Image[] deckNumberButtons;
    public Sprite[] deckNumberButtonSprites;

    [Tooltip("Poof")]
    public Transform[] particleEffects;

    public GameObject optionsMenu;

    private void Awake()
    {
        instance = this;
        ams = AssetsManagerScript.instance;
        sms = StatsManagerScript.instance;
    }

    private void Start()
    {
        OrganizeCards();

        StartCoroutine(SpawnCards());
        for (int i = 0; i < sms.unlockedCards.Count; i++)
            if (sms.unlockedCards[i] > 0)
                amountOfUnlockedCards++;

        totalPages = (int)Mathf.Ceil(amountOfUnlockedCards / 6.0f);
        if (totalPages == 0)
            totalPages = 1;
        UpdateUI();
    }

    private void Update()
    {
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
                        StartCoroutine(DelayCanInteract(0.25f));
                    }
                }
                else
                {
                    canTapDisplayCard = true;
                }
            }
        }
    }

    void OrganizeCards()
    {
        ams.deckCreatorCards = ams.deckCreatorCards.OrderBy(c => c.creatureName).ToList();
        ams.deckCreatorCards = ams.deckCreatorCards.OrderBy(c => c.manaValue).ToList();

        ams.playerDeck = ams.playerDeck.OrderBy(c => c.creatureName).ToList();
        ams.playerDeck = ams.playerDeck.OrderBy(c => c.manaValue).ToList();
    }

    public void SwitchDeck(int _deck)
    {
        if (canInteract)
        {
            if (_deck == sms.activeDeck)
                return;
            PlayerPrefs.SetInt("ActiveDeck", _deck);
            sms.activeDeck = _deck;
            sms.UpdateDeck();
            ChangePage(0);
            sms.SaveGame();
        }
    }

    public void ResetDeck()
    {
        if (canInteract)
        {
            sms.playerDeck.Clear();
            ams.UpdatePlayerDeck();
            ChangePage(0);
        }
    }

    public void LoadScene(string _sceneName)
    {
        sms.SaveGame();
        if (sms.playerDeck.Count < 20)
        {
            //PopupManagerScript.instance.AddPopup("Notice.", "Your currently selected deck does not have 20 cards and therefore you will not be able to play any battles.");
        }
        SceneTransitionsManager.instance.LoadScene(_sceneName);
    }

    public void UpdateUI()
    {
        pagesText.text = (page + 1) + "/" + totalPages;
        cardCountText.text = sms.playerDeck.Count + "/20";

        if (showingDeck)
        {
            if (ams.playerDeck.Count == 0)
                noCardsText.SetActive(true);
            else
                noCardsText.SetActive(false);
        }
        else
        {
            noCardsText.SetActive(false);
        }

        for (int i = 0; i < deckNumberButtons.Length; i++)
        {
            if (i + 1 == sms.activeDeck)
                deckNumberButtons[i].sprite = deckNumberButtonSprites[i];
            else
                deckNumberButtons[i].sprite = deckNumberButtonSprites[i + 3];
        }
    }

    //Switches between showing the player's deck and their unlocked cards.
    public void ChangeView()
    {
        if (canInteract)
        {
            showingDeck = !showingDeck;

            if (showingDeck)
            {
                OrganizeCards();
                viewButtonImage.sprite = viewButtonSprites[1];
                cardHolders.localPosition = new Vector2(0f, cardHolderHeights[1]);
                totalPages = (int)Mathf.Ceil(ams.playerDeck.Count / 6.0f);
                if (totalPages == 0)
                    totalPages = 1;
                viewText.text = "Deck:";
            }
            else
            {
                viewButtonImage.sprite = viewButtonSprites[0];
                cardHolders.localPosition = new Vector2(0f, cardHolderHeights[0]);

                totalPages = (int)Mathf.Ceil(amountOfUnlockedCards / 6.0f);
                if (totalPages == 0)
                    totalPages = 1;
                viewText.text = "Collection:";
            }

            StartCoroutine(DelayCanInteract(1.5f));
            StartCoroutine(WipeCards());
            page = 0;
            StartCoroutine(SpawnCards());
            UpdateUI();
        }
    }

    IEnumerator WipeCards()
    {
        GameObject[] _cards = GameObject.FindGameObjectsWithTag("PlayerCard");
        for (int i = 0; i < _cards.Length; i++)
        {
            _cards[i].SendMessage("Wipe");
            yield return new WaitForSeconds(0.025f);
        }
    }

    IEnumerator SpawnCards()
    {
        yield return new WaitForSeconds(0.2f);
        for (int i = 0; i < 6; i++)
        {
            int _addAmnt = 6 * page;

            if (showingDeck)
            {
                if (i + _addAmnt < ams.playerDeck.Count)
                {
                    Vector2 _spawnLoc;
                    _spawnLoc = new Vector2(cardHoldSlots[i].position.x, -Camera.main.orthographicSize * 2.0f);
                    DeckCreatorCardScript _cardInstance;
                    _cardInstance = Instantiate(deckCreatorCardPrefab, _spawnLoc, Quaternion.identity);
                    _cardInstance.SetMovePos(cardHoldSlots[i].position);
                    _cardInstance.SendMessage("RemoveMode");
                    _cardInstance.SetUpCard(ams.playerDeck[i + _addAmnt].creatureName);
                    yield return new WaitForSeconds(0.025f);
                }
                else
                    break;
            }
            else
            {
                if (i + _addAmnt < amountOfUnlockedCards)
                {
                    Vector2 _spawnLoc;
                    _spawnLoc = new Vector2(cardHoldSlots[i].position.x, -Camera.main.orthographicSize * 2.0f);
                    DeckCreatorCardScript _cardInstance;
                    _cardInstance = Instantiate(deckCreatorCardPrefab, _spawnLoc, Quaternion.identity);
                    _cardInstance.SetMovePos(cardHoldSlots[i].position);
                    _cardInstance.SetUpCard(ams.deckCreatorCards[i + _addAmnt].creatureName);
                    yield return new WaitForSeconds(0.025f);
                }
                else
                    break;
            }
        }
    }

    IEnumerator DelayCanInteract(float _time)
    {
        canInteract = false;
        yield return new WaitForSeconds(_time);
        canInteract = true;
    }

    public void UpdateCards()
    {
        StartCoroutine(DelayUpdateCards(0.01f));
    }

    IEnumerator DelayUpdateCards(float _time)
    {
        yield return new WaitForSeconds(_time);

        int _addAmnt = 6 * page;

        ams.playerDeck = ams.playerDeck.OrderBy(c => c.creatureName).ToList();
        ams.playerDeck = ams.playerDeck.OrderBy(c => c.manaValue).ToList();

        GameObject[] _activeCards = GameObject.FindGameObjectsWithTag("PlayerCard");
        for (int i = 0; i < _activeCards.Length; i++)
        {
            if (i + _addAmnt < ams.playerDeck.Count)
            {
                _activeCards[i].GetComponent<DeckCreatorCardScript>().SetUpCard(ams.playerDeck[i + _addAmnt].creatureName);
                _activeCards[i].SendMessage("SkipEntryAnim", true);
            }
            else
            {
                Instantiate(particleEffects[0], _activeCards[i].transform.position, Quaternion.identity);
                Destroy(_activeCards[i]);
            }

            totalPages = (int)Mathf.Ceil(ams.playerDeck.Count / 6.0f);
            if (totalPages == 0)
                totalPages = 1;
            if (page + 1 > totalPages)
                ChangePage(-1);
            UpdateUI();
        }
    }

    //Used when a card is held down long enough to show a Display of it.
    public void SetCardDisplay(CardScript _card)
    {
        if (_card != null)
        {
            screenBlock.SetActive(true);
            canInteract = false;
        }
        else
        {
            displayCard.InstantWipe();
            screenBlock.SetActive(false);
            canInteract = true;
        }
        displayCard = _card;
    }

    public void ChangePage(int _amnt)
    {
        //-1 = back a page, 1 = forward a page.
        if (canInteract)
        {
            StartCoroutine(DelayCanInteract(1.3f));
            StartCoroutine(WipeCards());
            page += _amnt;
            if (page >= totalPages)
                page = 0;
            if (page < 0)
                page = totalPages - 1;
            StartCoroutine(SpawnCards());
            UpdateUI();
        }
    }

    void ReloadPage()
    {
        StartCoroutine(DelayCanInteract(1.3f));
        StartCoroutine(WipeCards());
        StartCoroutine(SpawnCards());
        UpdateUI();
    }

    //Options Menu
    public void ShowOptionsMenu(bool _setting)
    {
        optionsMenu.SetActive(_setting);
        if (_setting)
            canInteract = false;
        else
            StartCoroutine(DelayCanInteract(0.5f));
    }

    public void RemoveAllCards()
    {
        sms.playerDeck.Clear();
        ams.UpdatePlayerDeck();
        sms.UpdateSavedDecks();
        ShowOptionsMenu(false);
        ReloadPage();
        sms.SaveGame();
        StartCoroutine(DelayCanInteract(0.5f));
    }
}