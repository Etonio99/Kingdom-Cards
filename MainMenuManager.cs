using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    private ZoneMenuManagerScript zmms;

    public Text userNameText;

    [Tooltip("Main, Play, Shop")]
    public GameObject[] menus;
    private int activeMenu = 0;
    private Vector2 onScreenPos;
    private Vector2[] offScreenPos;
    private bool showZoneMenu = false;
    private Vector2 zoneMenuOffScreenPos;

    private void Awake()
    {
        zmms = GetComponent<ZoneMenuManagerScript>();
    }

    private void Start()
    {
        userNameText.text = PlayerPrefs.GetString("UserName");

        float _screenHeight = Camera.main.orthographicSize * 2.0f;
        float _screenWidth = _screenHeight * Camera.main.aspect;

        offScreenPos = new Vector2[menus.Length];

        onScreenPos = menus[0].transform.position;
        offScreenPos[0] = new Vector2(0f, _screenHeight);
        offScreenPos[1] = new Vector2(0f, -_screenHeight);
        offScreenPos[2] = new Vector2(0f, -_screenHeight);
        zoneMenuOffScreenPos = offScreenPos[0];

        for (int i = 0; i < menus.Length; i++)
        {
            if (i != activeMenu)
                menus[i].transform.position = offScreenPos[i];
        }
    }

    private void FixedUpdate()
    {
        float _interpolation = 7f * Time.fixedDeltaTime;
        for (int i = 0; i < menus.Length; i++)
        {
            if (activeMenu == i)
            {
                Vector2 _newPos = Vector2.Lerp(menus[i].transform.position, onScreenPos, _interpolation);
                menus[i].transform.position = _newPos;
            }
            else
            {
                Vector2 _newPos = Vector2.Lerp(menus[i].transform.position, offScreenPos[i], _interpolation);
                menus[i].transform.position = _newPos;
            }
        }

        if (showZoneMenu)
        {
            Vector2 _newPos = Vector2.Lerp(zmms.battlerSelectMenu.transform.position, onScreenPos, _interpolation);
            zmms.battlerSelectMenu.transform.position = _newPos;
        }
        else
        {
            Vector2 _newPos = Vector2.Lerp(zmms.battlerSelectMenu.transform.position, zoneMenuOffScreenPos, _interpolation);
            zmms.battlerSelectMenu.transform.position = _newPos;
        }
    }

    public void SwitchMenu(int _id)
    {
        activeMenu = _id;
    }

    public void ShowZoneMenu(int _zoneId)
    {
        zmms.SetKingdom(_zoneId);
        zmms.SetupMenu();
        zmms.UpdateMenu();
        showZoneMenu = true;
    }

    public void HideZoneMenu()
    {
        showZoneMenu = false;
    }
}