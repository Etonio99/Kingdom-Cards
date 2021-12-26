using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    public Text dialogueText;
    public GameObject dialogueBox;
    private bool showingDialogue = false;
    private bool finishedWriting = false;
    private bool instantFinish = false;

    private Queue<string> sentences = new Queue<string>();

    private DialogueType storedialogueType;
    private GameObject storedSender;

    [HideInInspector]
    public bool movePlayerForward = false;

    [HideInInspector]
    public bool canTap = true;

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

    private void Update()
    {
        if (showingDialogue)
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended && canTap)
            {
                if (finishedWriting)
                    DisplayNextSentence();
                else
                    instantFinish = true;
            }
        }
    }

    public void DisplayDialogue(Dialogue _dialogue, GameObject _sender)
    {
        sentences.Clear();
        dialogueBox.SetActive(true);
        showingDialogue = true;

        storedialogueType = _dialogue.dialogueType;
        storedSender = _sender;

        foreach (string _sentence in _dialogue.sentences)
        {
            sentences.Enqueue(_dialogue.speakerName + ": " + _sentence);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        finishedWriting = false;
        string _sentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(_sentence));
    }

    IEnumerator TypeSentence(string _sentence)
    {
        dialogueText.text = "";
        foreach (char _letter in _sentence.ToCharArray())
        {
            if (instantFinish)
            {
                dialogueText.text = _sentence;
                instantFinish = false;
                finishedWriting = true;
                break;
            }
            dialogueText.text += _letter;
            if (dialogueText.text == _sentence)
                finishedWriting = true;
            else
                finishedWriting = false;
            yield return null;
        }
    }

    void EndDialogue()
    {
        showingDialogue = false;
        dialogueBox.SetActive(false);

        if (storedialogueType == DialogueType.Battler)
        {
            storedSender.GetComponent<BattlerScript>().StartBattle();
        }

        if (storedialogueType == DialogueType.AfterBattle)
        {
            if (!BattleManagerScript.instance.playerWins)
                BoardManagerScript.instance.SetUsableUI(true);
        }

        storedialogueType = DialogueType.Default;
        storedSender = null;

        if (movePlayerForward)
        {
            BoardManagerScript.instance.MovePlayerForward();
            movePlayerForward = false;
        }
    }
}