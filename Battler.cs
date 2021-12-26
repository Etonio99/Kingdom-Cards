using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Unnamed Battler", menuName = "Kingdom Cards/Battler")]
public class Battler : ScriptableObject
{
    public new string name;
    public string title = "Battler";

    public Sprite battlerSprite;
    public Sprite displaySprite;
    public Sprite buttonSprite;

    public string kingdom;
    public int battlerNumber = 0;

    public int health = 25;
    public int mana = 3;

    public List<string> deck = new List<string>();

    public int cardBack;

    public int rewardGold;
    public int rewardGems;
    [Tooltip("A value of -1 results in no rewarded chest.")]
    public int rewardChest = -1;
}