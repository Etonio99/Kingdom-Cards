using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChestRewardScript : MonoBehaviour
{
    public Image rewardImage;
    public Text rewardText;

    public Sprite[] rewardSprites;

    private Vector2 movePos;

    public void SetMovePos(Vector2 _pos)
    {
        movePos = _pos;
    }

    public void SetupReward(int _rewardID, int _amount)
    {
        rewardImage.sprite = rewardSprites[_rewardID];
        rewardText.text = _amount.ToString();
    }

    private void Update()
    {
        float _interpolation = 5f * Time.deltaTime;
        Vector2 _newPos = Vector2.Lerp(transform.position, movePos, _interpolation);
        transform.position = _newPos;
    }

    //Used to remove the card after a round has ended.
    public void Wipe()
    {
        tag = "Untagged";
        Destroy(gameObject);
    }
}