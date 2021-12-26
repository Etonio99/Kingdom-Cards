using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransitionScript : MonoBehaviour
{
    public Image bgImage;
    public Image battlerImage;
    public Text battleText;

    public Color[] bgColors;

    private Vector2 offScreenPos;
    private bool coverScreen = false;
    public Vector2 battlerPosition;
    private Vector2 battlerOffScreenPos;
    private bool showBattler = false;
    public Vector2 textPosition;
    private Vector2 textOffScreenPos;

    public GameObject rewardsBox;
    [Tooltip("Status Image, Gold, Gems, Chest, No Rewards, Rewards")]
    public GameObject[] rewardUIElements;
    [Tooltip("Victory, Defeat")]
    public Sprite[] statusImages;
    public Text[] rewardTexts;

    private bool waitForTouch = false;

    private List<GameObject> disabledCanvases = new List<GameObject>();

    private void Start()
    {
        offScreenPos = new Vector2(0f, Screen.height * 3f);
        ResetTransition();
    }

    private void Update()
    {
        if (waitForTouch)
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                waitForTouch = false;
                StartCoroutine(EndTransition());
                if (BattleManagerScript.instance.playerWins)
                {
                    ZoneMenuManagerScript _zmms = GameObject.Find("MainMenuManager").GetComponent<ZoneMenuManagerScript>();
                    _zmms.OpenChest();
                    _zmms.UpdateKingdomProgress();
                    _zmms.UpdateMenu();
                }
                    
            }
        }
    }

    private void FixedUpdate()
    {
        float _interpolation = 10f * Time.deltaTime;
        if (coverScreen)
        {
            Vector2 _newPos;
            _newPos.x = Mathf.SmoothStep(bgImage.transform.localPosition.x, 0f, _interpolation);
            _newPos.y = Mathf.SmoothStep(bgImage.transform.localPosition.y, 0f, _interpolation);
            bgImage.transform.localPosition = _newPos;
        }
        else
        {
            Vector2 _newPos;
            _newPos.x = Mathf.SmoothStep(bgImage.transform.localPosition.x, offScreenPos.x, _interpolation);
            _newPos.y = Mathf.SmoothStep(bgImage.transform.localPosition.y, offScreenPos.y, _interpolation);
            bgImage.transform.localPosition = _newPos;
        }

        if (showBattler)
        {
            Vector2 _newPos;
            _newPos.x = Mathf.SmoothStep(battlerImage.transform.localPosition.x, battlerPosition.x, _interpolation);
            _newPos.y = Mathf.SmoothStep(battlerImage.transform.localPosition.y, battlerPosition.y, _interpolation);
            battlerImage.transform.localPosition = _newPos;
            Vector2 _newPosText;
            _newPosText.x = Mathf.SmoothStep(battleText.transform.localPosition.x, textPosition.x, _interpolation);
            _newPosText.y = Mathf.SmoothStep(battleText.transform.localPosition.y, textPosition.y, _interpolation);
            battleText.transform.localPosition = _newPosText;
        }
        else
        {
            Vector2 _newPos;
            _newPos.x = Mathf.SmoothStep(battlerImage.transform.localPosition.x, battlerOffScreenPos.x, _interpolation);
            _newPos.y = Mathf.SmoothStep(battlerImage.transform.localPosition.y, battlerOffScreenPos.y, _interpolation);
            battlerImage.transform.localPosition = _newPos;
            Vector2 _newPosText;
            _newPosText.x = Mathf.SmoothStep(battleText.transform.localPosition.x, textOffScreenPos.x, _interpolation);
            _newPosText.y = Mathf.SmoothStep(battleText.transform.localPosition.y, textOffScreenPos.y, _interpolation);
            battleText.transform.localPosition = _newPosText;
        }
    }

    public void BattleTransition(string _battlerName, Sprite _battlerSprite)
    {
        BattleManagerScript.instance.activeBattle = true;

        bgImage.gameObject.SetActive(true);
        bgImage.color = bgColors[0];
        battlerImage.gameObject.SetActive(true);
        battleText.gameObject.SetActive(true);
        battleText.transform.localPosition = textOffScreenPos;
        battleText.text = "Player " + _battlerName + "\nwants to battle!";
        battlerImage.transform.localPosition = battlerOffScreenPos;
        battlerImage.sprite = _battlerSprite;

        StartCoroutine(Transition());
    }

    IEnumerator Transition()
    {
        coverScreen = true;
        yield return new WaitForSeconds(0.25f);
        showBattler = true;
        battlerOffScreenPos = new Vector2(Screen.width * 2.5f, battlerPosition.y);
        textOffScreenPos = new Vector2(-Screen.width * 2.5f, textPosition.y);
        yield return new WaitForSeconds(2.5f);
        showBattler = false;

        GameObject[] _temp = GameObject.FindGameObjectsWithTag("DisableCanvas");
        foreach (var _go in _temp)
        {
            disabledCanvases.Add(_go);
            _go.SetActive(false);
        }

        yield return new WaitForSeconds(0.35f);
        coverScreen = false;
        yield return new WaitForSeconds(1f);
        ResetTransition();
    }

    public void EndBattleTransition()
    {
        bgImage.gameObject.SetActive(true);
        bgImage.color = bgColors[1];

        rewardsBox.SetActive(true);
        rewardUIElements[0].SetActive(true);
        if (BattleManagerScript.instance.playerWins)
        {
            Vector3 _tempRewards = GameObject.Find("MainMenuManager").GetComponent<ZoneMenuManagerScript>().GetRewards();

            rewardUIElements[0].GetComponent<Image>().sprite = statusImages[0];

            rewardUIElements[5].SetActive(true);

            if (_tempRewards.x > 0)
            {
                rewardUIElements[1].SetActive(true);
                rewardTexts[0].text = _tempRewards.x.ToString();
            }
            if (_tempRewards.y > 0)
            {
                rewardUIElements[2].SetActive(true);
                rewardTexts[1].text = _tempRewards.y.ToString();
            }
            if (_tempRewards.z > -1)
            {
                rewardUIElements[3].SetActive(true);
                rewardUIElements[3].GetComponentInChildren<Animator>().runtimeAnimatorController = ChestManagerScript.instance.chestAnimators[ChestManagerScript.instance.chests[(int)_tempRewards.z].animatorID];
            }
        }
        else
        {
            rewardUIElements[0].GetComponent<Image>().sprite = statusImages[1];
            rewardUIElements[4].SetActive(true);
        }

        StartCoroutine(AfterBattleTransition());
    }

    IEnumerator AfterBattleTransition()
    {
        coverScreen = true;
        yield return new WaitForSeconds(1.25f);
        waitForTouch = true;
        for (int i = 0; i < disabledCanvases.Count; i++)
        {
            disabledCanvases[i].SetActive(true);
        }
        disabledCanvases.Clear();
    }

    IEnumerator EndTransition()
    {
        coverScreen = false;
        yield return new WaitForSeconds(1f);
        rewardsBox.SetActive(false);
        foreach (var _go in rewardUIElements)
        {
            _go.SetActive(false);
        }
        ResetTransition();
    }

    void ResetTransition()
    {
        coverScreen = false;
        showBattler = false;
        battlerOffScreenPos = new Vector2(-Screen.width * 2.5f, battlerPosition.y);
        textOffScreenPos = new Vector2(Screen.width * 2.5f, textPosition.y);
        battlerImage.transform.localPosition = battlerOffScreenPos;
        battleText.transform.localPosition = textOffScreenPos;
        bgImage.gameObject.SetActive(false);
        battlerImage.gameObject.SetActive(false);
        battleText.gameObject.SetActive(false);
    }
}