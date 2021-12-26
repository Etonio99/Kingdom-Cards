using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AfterBattleRewardScript : MonoBehaviour
{
    private int gold = 0;
    private int gems = 0;
    private int tickets = 0;
    private int chest = -1;
    private string kingdom = "";

    public GameObject rewardsPopup;

    [Tooltip("Gold, Gems, Tickets, Chest, RewardsText")]
    public GameObject[] rewardUIElements;
    public Text[] rewardTexts;

    private bool checkForTouch = false;

    public void ShowRewards(Vector3 _statRewards, int _chestID, string _kingdom)
    {
        DialogueManager.instance.canTap = false;
        rewardsPopup.SetActive(true);
        gold = (int)_statRewards.x;
        gems = (int)_statRewards.y;
        tickets = (int)_statRewards.z;
        chest = _chestID;
        kingdom = _kingdom;

        rewardUIElements[4].SetActive(true);

        if (gold > 0)
        {
            rewardUIElements[0].SetActive(true);
            rewardTexts[0].text = gold.ToString();
        }
        if (gems > 0)
        {
            rewardUIElements[1].SetActive(true);
            rewardTexts[1].text = gems.ToString();
        }
        if (tickets > 0)
        {
            rewardUIElements[2].SetActive(true);
            rewardTexts[2].text = tickets.ToString();
        }
        if (chest > -1)
        {
            rewardUIElements[3].SetActive(true);
            rewardUIElements[3].GetComponentInChildren<Animator>().runtimeAnimatorController = ChestManagerScript.instance.chestAnimators[ChestManagerScript.instance.chests[chest].animatorID];
        }

        checkForTouch = true;
    }

    private void Update()
    {
        if (checkForTouch)
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                if (chest > -1)
                    ChestManagerScript.instance.OpenChest(chest, kingdom);
                else
                {
                    DialogueManager.instance.canTap = true;
                    //if (GameObject.Find("BattleManager"))
                        //BattleManagerScript.instance.afterBattleTrigger.DisplayDialogue();
                }
                checkForTouch = false;
                ResetRewards();
            }
        }
    }

    void ResetRewards()
    {
        gold = 0;
        gems = 0;
        tickets = 0;
        chest = -1;
        rewardsPopup.SetActive(false);

        foreach (var _go in rewardUIElements)
        {
            _go.SetActive(false);
        }
    }
}