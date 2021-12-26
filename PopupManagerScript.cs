using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupManagerScript : MonoBehaviour
{
    public static PopupManagerScript instance;

    private Queue<string> titles = new Queue<string>();
    private Queue<string> messages = new Queue<string>();

    private bool showingPopup = false;
    public GameObject popupWindow;
    public Text[] texts;

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

    public void AddPopup(string _title, string _message)
    {
        titles.Enqueue(_title);
        messages.Enqueue(_message);

        //if (!popupWindow.activeSelf)
            DisplayPopup();
    }

    public void DisplayPopup()
    {
        if (titles.Count > 0)
        {
            texts[0].text = titles.Dequeue();
            texts[1].text = messages.Dequeue();

            if (!popupWindow.activeSelf)
                popupWindow.SetActive(true);
        }
        else
            popupWindow.SetActive(false);
    }
}