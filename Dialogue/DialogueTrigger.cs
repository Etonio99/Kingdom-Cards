using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialouge;
    [Header("After Battle Dialogue")]
    public Dialogue winDialogue;
    public Dialogue loseDialogue;
    [Header("Options")]
    public bool movePlayerForward = false;

    public void DisplayDialogue()
    {
        if (dialouge.sentences.Length > 0)
        {
            DialogueManager.instance.DisplayDialogue(winDialogue, gameObject);
            if (movePlayerForward)
                DialogueManager.instance.movePlayerForward = true;
        }
        else
        {
            if (BattleManagerScript.instance.playerWins)
            {
                DialogueManager.instance.DisplayDialogue(winDialogue, gameObject);
                if (movePlayerForward)
                    DialogueManager.instance.movePlayerForward = true;
            }
            else
            {
                DialogueManager.instance.DisplayDialogue(loseDialogue, gameObject);
            }
        }
    }
}
