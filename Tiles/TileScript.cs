using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour
{
    public int tileNum = 0;
    public TileType tileType;

    public DialogueTrigger dialogueTrigger;

    public void LandOnTile()
    {
        if (tileType == TileType.Battler)
        {
            BattlerScript _bs = dialogueTrigger.GetComponent<BattlerScript>();
            BoardManagerScript.instance.SetBattlerInfo(_bs.battlerName, _bs.rewardGold, _bs.rewardGems);
        }
    }

    public void ActivateTile()
    {
        if (tileType == TileType.Default)
            return;
        else if (tileType == TileType.Battler)
        {
            //ExhaustTile();
            dialogueTrigger.DisplayDialogue();
        }
        else if (tileType == TileType.Treasure)
        {
            //ExhaustTile();
        }
    }

    public void ExhaustTile()
    {
        //tileType = TileType.Exhausted;
    }

    private void Update()
    {
        CheckForTouch();
    }

    void CheckForTouch()
    {
        if (Input.touchCount > 0)
        {
            Vector3 _touchPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
            Vector2 _newTouchPos = new Vector2(_touchPos.x, _touchPos.y);

            //Check if the player is holding down on the card to view it's info.
            if (Input.GetTouch(0).phase == TouchPhase.Ended && !BattleManagerScript.instance.activeBattle)
            {
                if (Vector2.Distance(transform.position, _newTouchPos) < 0.5f)
                {
                    //Kind of TEMP. For now...
                    BoardManagerScript.instance.MovePlayerToTile(tileNum);
                }
            }
        }
    }
}

public enum TileType
{
    Default, Battler, Treasure, Exit, Exhausted
}