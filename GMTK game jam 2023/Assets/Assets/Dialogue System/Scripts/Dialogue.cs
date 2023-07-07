using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Sentence
{
    [Tooltip("The time to wait before starting the dialogue.\nThe recommended value is longer than the text bubble entry animation.")]
    public float wait = 0.75f;
    [Tooltip("Think of a dialogue entry as a page of the dialogue.")]
    public DialogueEntry[] dialogueEntry = new DialogueEntry[2];
}

[System.Serializable]
public class DialogueEntry
{
    [Tooltip("The time to wait before starting to type.\nIf it's the first entry, it will be ignored\nDefault = 0.1")]
    public float wait = 0.1f;
    [TextArea(4, 7)]
    [Tooltip("The text that will be shown.\nYou can use rich text tags, " +
        "you can use '_' to stop typing for an amount of seconds defined by the underscoreWaitTime variable " +
        "and you can surround a word with '|' to type it without waiting")]
    public string text;

    [Tooltip("How much time to wait before typing the next letter/word\nDefault = 0.05")]
    public float typeSpeed = 0.05f;
    [Tooltip("How much time to wait when a underscore is reached.\nDefault = 0.15")]
    public float underscoreWaitTime = 0.15f;

	public enum typeStyle
	{
        letters,
        words
	}
    [Tooltip("Type a letter at a time or type the whole word at a time.")]
    public typeStyle type = typeStyle.letters;

    [Tooltip("Jump over entries.\nSet to 0 if you want to go to the next entry in the list.\nUse to connect branches in your dialogue.")]
    public int jumpEntries = 0;
    [Tooltip("Can the player press the continue button to to make the whole dialogue text visible at once?" +
		"\nOnly set to false if the player must not miss important dialogue typing pauses.")]
    public bool canSkip = true;

    public DialogueAnswer answer1;
    public DialogueAnswer answer2;

    public DialogueEvent dialogueEvent;

    public bool isLastMessage = false;
}

[CreateAssetMenu(fileName = "New dialogue", menuName = "ScriptableObjects/Dialogue", order = 0)]
public class Dialogue : ScriptableObject
{
    [Tooltip("The name of your character.")]
    public string characterName;
    [Tooltip("When the last sentence is reached the character will keep repeating it so that players don;t forget what they were told")]
    public bool repeatLastEntry = false;
    [Tooltip("Think of a sentence as the text from when the text bubble was opened to when it was closed." +
        " When the last dialogue entry is reached the sentence is ended." +
        " When the player triggers a new conversation your character will say other things.")]
    public Sentence[] sentence = new Sentence[2];
}

[System.Serializable]
public class DialogueRemember
{
    public string name;
    public int currentSentence;
}

[System.Serializable]
public class DialogueAnswer
{
    public string answer;
    [Tooltip("Jump over entries and branch the dialogue.\nSet to 0 if you want to go to the next entry in the list.")]
    public int jumpEntries;
}

[System.Serializable]
public class DialogueEvent
{
	public enum eventWhen
	{
        never,
        beforeWaiting,
        beforeTyping,
        afterTyping
	}

    public eventWhen when;
    [Tooltip("Which event from the Dialogue Event Manager array should this entry ivoke?")]
    public int invokeEvent;
}