using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardManagerScript : MonoBehaviour
{
    public static BoardManagerScript instance;

    public Transform player;
    public Transform[] tiles;
    public Sprite[] tileSprites;
    private int curTile = 0;

    private Vector2 movePos;
    public Transform[] effects;

    private bool canUseUI = true;
    public GameObject[] UIXs;
    public GameObject boardCanvas;

    private CameraScript _cs;
    public Sprite[] mapButtonSprites;
    public Image mapButton;

    public GameObject menuButtons;

    public Text userNameText;

    public Text battlerInfoText;

    private void Awake()
    {
        instance = this;

        _cs = Camera.main.GetComponent<CameraScript>();
    }

    private void Start()
    {
        SetCameraFollow(true);
        SetCameraPan(false);
        SetUsableUI(true);
        SetupPlayer();

        userNameText.text = PlayerPrefs.GetString("UserName");
    }

    public void ShowMenu(bool _setting)
    {
        if (canUseUI)
        {
            menuButtons.SetActive(_setting);
            if (_setting)
            {
                if (_cs.canPan)
                {
                    CameraPanButton();
                    SetCameraFollow(true);
                }
            }
        }
    }

    public void MovePlayerForward()
    {
        if (_cs.canPan)
        {
            CameraPanButton();
            SetCameraFollow(true);
        }
        curTile += 1;
        if (curTile >= tiles.Length)
            curTile = tiles.Length - 1;
        StartCoroutine(UpdatePlayerPosition());
    }

    public void MovePlayerToTile(int _tile)
    {
        if (_cs.canPan)
        {
            CameraPanButton();
            SetCameraFollow(true);
        }
        curTile = _tile;
        StartCoroutine(UpdatePlayerPosition());
    }

    public void ActivateTile()
    {
        if (canUseUI && StatsManagerScript.instance.playerDeck.Count == 20)
        {
            SetUsableUI(false);
            StartCoroutine(DelayActivateTile());
        }
    }

    IEnumerator DelayActivateTile()
    {
        yield return new WaitForSeconds(0.05f);
        tiles[curTile].GetComponent<TileScript>().ActivateTile();
    }

    public void ResetPlayer ()
    {
        curTile = 0;
        StartCoroutine(UpdatePlayerPosition());
    }

    void SetCameraPan (bool _setting)
    {
        _cs.canPan = _setting;
        if (_setting)
            _cs.finPanEffect = false;
    }

    public void CameraPanButton ()
    {
        if (canUseUI)
        {
            _cs.canPan = !_cs.canPan;
            if (_cs.canPan)
            {
                _cs.finPanEffect = false;
                mapButton.sprite = mapButtonSprites[0];
            }
            else
            {
                mapButton.sprite = mapButtonSprites[1];
            }
        }
    }

    public void SetCameraFollow (bool _setting)
    {
        _cs.followPlayer = _setting;
    }

    private void Update()
    {
        float _interpolation = 10f * Time.deltaTime;
        Vector2 _newPos = Vector2.Lerp(player.position, movePos, _interpolation);
        player.position = _newPos;
    }

    public void SetUsableUI (bool _setting)
    {
        canUseUI = _setting;

        if (canUseUI)
        {
            foreach (GameObject _x in UIXs)
                _x.SetActive(false);
        }
        else
        {
            foreach (GameObject _x in UIXs)
                _x.SetActive(true);
        }
    }

    //Used as soon as the scene is loaded to put the player on the first tile.
    void SetupPlayer()
    {
        curTile = 0;
        Vector2 startPos = new Vector2(tiles[curTile].position.x, tiles[curTile].position.y + 1.125f);
        player.position = startPos;
        movePos = startPos;
        SetCameraFollow(true);
        tiles[curTile].GetComponent<SpriteRenderer>().sprite = tileSprites[1];
        tiles[curTile].SendMessage("LandOnTile");
    }

    IEnumerator UpdatePlayerPosition()
    {
        SetCameraFollow(false);
        movePos = new Vector2(player.position.x, player.position.y + 1f);
        yield return new WaitForSeconds(0.2f);
        Instantiate(effects[0], player.position, Quaternion.identity);
        yield return new WaitForSeconds(0.05f);

        player.position = new Vector2(tiles[curTile].position.x, tiles[curTile].position.y + 2f);
        movePos = new Vector2(tiles[curTile].position.x, tiles[curTile].position.y + 1.125f);
        SetCameraFollow(true);
        yield return new WaitForSeconds(0.25f);
        //TEMP
        tiles[curTile].GetComponent<SpriteRenderer>().sprite = tileSprites[1];
        //END TEMP
        SetUsableUI(true);
        tiles[curTile].SendMessage("LandOnTile");
    }

    public void SetBattlerInfo(string _battlerName, int _rewardGold, int _rewardGems)
    {
        string _temp = "";
        if (_battlerName != null)
            _temp += "Battler: " + _battlerName;
        if (_rewardGold > 0)
            _temp += "\nReward Gold: " + _rewardGold.ToString();
        if (_rewardGems > 0)
            _temp += "\nReward Gems: " + _rewardGems.ToString();

        battlerInfoText.text = _temp;
    }
}