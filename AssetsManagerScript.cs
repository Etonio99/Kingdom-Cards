using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetsManagerScript : MonoBehaviour
{
    public static AssetsManagerScript instance;

    public Sprite[] creatureCardSprites;
    public Sprite[] spellCardSprites;
    public Sprite[] structureCardSprites;
    public Sprite[] cardBgSprites;
    public Sprite[] numberSprites;
    public Sprite[] elementSprites;
    public Sprite[] scrollSprites;

    public Card[] cards;
    public Kingdom[] kingdoms;
    [HideInInspector]
    public List<Card> deckCreatorCards = new List<Card>();
    public Sprite[] battlerSprites;

    [HideInInspector]
    public List<Card> playerDeck = new List<Card>();

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
        UpdatePlayerDeck();
    }

    public void UpdatePlayerDeck()
    {
        playerDeck.Clear();
        foreach (string _s in StatsManagerScript.instance.playerDeck)
        {
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i].creatureName == _s)
                {
                    playerDeck.Add(cards[i]);
                    break;
                }
            }
        }
    }

    public void SetUpDeckCreatorCards()
    {
        deckCreatorCards.Clear();

        if (StatsManagerScript.instance.unlockedCards.Count < cards.Length)
        {
            for (int i = StatsManagerScript.instance.unlockedCards.Count; i < cards.Length; i++)
                StatsManagerScript.instance.unlockedCards.Add(0);
        }

        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i].canBeInDeck)
            {
                if (StatsManagerScript.instance.unlockedCards[i] > 0)
                    deckCreatorCards.Add(cards[i]);
            }
        }
    }
}

[System.Serializable]
public class Card
{
    public string creatureName = "Unnamed Creature";
    public string creatureDescription = "Undescribed Creature";
    public Element element;
    public CardBacks cardBack;
    public int manaValue = 0;
    public int attackValue = 0;
    public int healthValue = 0;
    public Sprite creatureSprite;
    public RuntimeAnimatorController creatureAnimator;
    public Ability[] abilities;
    public string abilityName = "No Ability";
    public string abilityDescription = "This creature does not have an ability.";
    public UseLimit[] limits;
    public bool canBeInDeck = true;
}

public enum Element
{
    None, Dark, Light, Mechanical, Flora, Fauna, Ghost
}

public enum CardBacks
{
    Unthemed, Spell, Structure, Village, Necromancer, Scrapyard, Shadow
}

[System.Serializable]
public class Ability
{
    public AbilityType ability;
    public int abilityValue = 0;
    [Tooltip("Only change this option if the ability places a card is placed into one of the player's hands.")]
    public Vector3 cardChangeValues;
    public string stringValue = "";
}

public enum AbilityType
{
    //These values are stored in another file not included here for me to understand.
    CPHOA, COHOA, CPHODA, COHODA, CPHOE, COHOE, CPMOA, COMOA, CPMODA, COMODA, PCIPH, PCIOH, PCIPHOD, PCIOHOD, PCIPHODA, PCIOHODA,
        SCFOD, CNPCS, CNOCS, BADONH, DAFOC, PCIPDOA, PCIODOA, CCHOA, DAOOC, CPMOE, COMOE
}

[System.Serializable]
public class UseLimit
{
    public Limits limit;
    public int limitValue = 0;
}

public enum Limits
{
    WFH, WOHLTSH, WOHLTSM, UN
}

[System.Serializable]
public class Kingdom
{
    public string kingdomName = "Unnamed Kingdom";
    public string[] unlockableCards;
}