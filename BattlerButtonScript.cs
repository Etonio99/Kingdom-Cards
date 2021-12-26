using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlerButtonScript : MonoBehaviour
{
    ZoneMenuManagerScript zmms;

    public int battlerId = 0;

    private void Start()
    {
        zmms = GameObject.Find("MainMenuManager").GetComponent<ZoneMenuManagerScript>();
    }

    public void Activate()
    {
        zmms.SetBattler(battlerId);
        zmms.UpdateMenu();
    }
}