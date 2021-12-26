using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dialogue
{
    public string speakerName = "Unnamed";
    [TextArea(3, 10)]
    public string[] sentences;
    public DialogueType dialogueType;
}

public enum DialogueType
{
    Default, Battler, Treasure, AfterBattle
}
