using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlerScript : MonoBehaviour
{
    public string battlerName = "[Name]";
    public int battlerID = 0;

    public List<string> deck = new List<string>();

    public DialogueTrigger afterBattleDialogue;
    public int opponentStartingHealth = 25;

    public int rewardGold = 15;
    public int rewardGems = 0;
    [Tooltip("Value of -1 does not reward the player with a chest.")]
    public int rewardChest = -1;
    public string kingdom = "";


    //TEMP
    private Battler battler;


    public void StartBattle()
    {
        BattleManagerScript.instance.ts.BattleTransition(battlerName, null);
        StartCoroutine(DelayBattleStart());
    }

    IEnumerator DelayBattleStart()
    {
        yield return new WaitForSeconds(3.1f);
        BattleManagerScript.instance.StartNewBattle(battler);
        //BattleManagerScript.instance.SetAfterBattleDialogueTrigger(afterBattleDialogue);
        BattleManagerScript.instance.SetRewards(rewardGold, rewardGems, rewardChest, kingdom);
    }
}