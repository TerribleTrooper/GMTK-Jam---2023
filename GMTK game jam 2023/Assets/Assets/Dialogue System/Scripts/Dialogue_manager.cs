using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

public class Dialogue_manager : MonoBehaviour
{
	#region Variables
	public static Dialogue_manager instance; //Singleton

	//UI reference
	public TMP_Text nameText;
	public TMP_Text dialogueText;
	public TMP_Text answer1Text;
	public TMP_Text answer2Text;
	public Animator textBubbleAnimator;
	public Animator answerButtonAnimator;

	int currentSentence = 0;
	int currentDialogueEntry = 0;

	//Various variables for typing the text
	bool hasFinishedSentence = false;
	bool waitToType = false;
	bool isTyping = false;
	bool isInAngledBrackets = false;
	bool isInLineBrackets = false;

	//Variables for answering
	bool hasAnswer = false;

	List <char> unfilteredText = new List<char>(); //The text from the dialogue entry but in the form of a char array
	List<char> usedText = new List<char>(); //The text used by text mesh pro (includes <> but not |, _)
	List<char> filteredText = new List<char>(); //The text used to detect | and _ uses (doesn't include <>)

	//Array that remember the last sentence that you have seen with a certain character
	List<DialogueRemember> remember = new List<DialogueRemember>();

	Dialogue currentDialogue;
	[HideInInspector]
	public bool dialogueActive = false;
	#endregion

	private void Awake()
	{
		if(instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	public void StartDialogue(Dialogue dialogue)
	{
		dialogueActive = true;

		currentDialogue = dialogue;
		currentDialogueEntry = 0;
		SetCurrentSentence();
		nameText.text = dialogue.characterName;
		hasFinishedSentence = false;
		waitToType = true;
		isTyping = false;
		isInAngledBrackets = false;
		isInLineBrackets = false;

		textBubbleAnimator.SetBool("isOpen", true);
		if (!SentenceInBounds(currentSentence))
		{
			EndDialogue();
			return;
		}

		ShowNextSentence();
	}

	public void ShowNextSentence()
	{
		if(!answerButtonAnimator.GetBool("isOpen") && hasAnswer)
		{
			answer1Text.text = GetCurrentEntry().answer1.answer;
			answer2Text.text = GetCurrentEntry().answer2.answer;
			answerButtonAnimator.SetBool("isOpen", true);
		}

		if (hasAnswer && !isTyping)//You can't continue if don't answer
		{
			return;
		}
		hasAnswer = GetCurrentEntry().answer1.answer != "";

		if (!hasFinishedSentence)
		{
			if (isTyping) //If you are impatient you can make the text appear instantly.
			{
				if (!GetCurrentEntry().canSkip)	//You can't skip if the dialogue scriptable object says so
					return;

				StopAllCoroutines();

				dialogueText.text = "";
				dialogueText.maxVisibleCharacters = 99999;
				dialogueText.maxVisibleWords = 99999;

				for (int i = 0; i < usedText.Count; i++)
				{
					dialogueText.text += usedText[i];
				}

				if (hasAnswer)
				{
					answer1Text.text = GetCurrentEntry().answer1.answer;
					answer2Text.text = GetCurrentEntry().answer2.answer;
					answerButtonAnimator.SetBool("isOpen", true);
				}

				if (GetCurrentEntry().dialogueEvent.when != DialogueEvent.eventWhen.never)
				{
					Dialogue_event_manager.instance.dialogueEvents[GetCurrentEntry().dialogueEvent.invokeEvent].Invoke();
				}

				HandleCurrentEntry();

				isTyping = false;
			}
			else
			{
				StartCoroutine(TypeText());
			}
		}
		else
		{
			EndDialogue();
		}
	}

	IEnumerator TypeText()
	{
		//dialogueText.textInfo.CopyMeshInfoVertexData(); //IMPORTANT for adding text vertex effects!!!!
		isTyping = true;

		FilterText();

		dialogueText.text = "";
		dialogueText.maxVisibleCharacters = 0;

		StringBuilder stringBuilder = new StringBuilder();

		for (int i = 0; i < usedText.Count; i++)
		{
			stringBuilder.Append(usedText[i]);
			//dialogueText.text += usedText[i];
		}

		dialogueText.text = stringBuilder.ToString();

		if (GetCurrentEntry().dialogueEvent.when == DialogueEvent.eventWhen.beforeWaiting)
		{
			Dialogue_event_manager.instance.dialogueEvents[GetCurrentEntry().dialogueEvent.invokeEvent].Invoke();
		}

		if (waitToType)
		{
			yield return new WaitForSeconds(GetCurrentSentence().wait);
			waitToType = false;
		}
		else
		{
			yield return new WaitForSeconds(GetCurrentEntry().wait);
		}

		if (GetCurrentEntry().dialogueEvent.when == DialogueEvent.eventWhen.beforeTyping)
		{
			Dialogue_event_manager.instance.dialogueEvents[GetCurrentEntry().dialogueEvent.invokeEvent].Invoke();
		}

		//For typing letters
		if (GetCurrentEntry().type == DialogueEntry.typeStyle.letters)
		{
			dialogueText.maxVisibleWords = 99999;
			dialogueText.maxVisibleCharacters = 0;

			for (int i = 0; i < filteredText.Count; i++)
			{
				if (filteredText[i] == '|')
				{
					isInLineBrackets = !isInLineBrackets;
					continue;
				}

				if (isInLineBrackets)
				{
					dialogueText.maxVisibleCharacters++;
				}
				else if (filteredText[i] == '_')
				{
					yield return new WaitForSeconds(GetCurrentEntry().underscoreWaitTime);
				}
				else
				{
					dialogueText.maxVisibleCharacters++;
					yield return new WaitForSeconds(GetCurrentEntry().typeSpeed);
				}
			}
		}

		//For typing words
		if (GetCurrentEntry().type == DialogueEntry.typeStyle.words)
		{
			dialogueText.maxVisibleWords = 0;
			dialogueText.maxVisibleCharacters = 99999;

			for (int i = 0; i < dialogueText.textInfo.wordCount; i++)
			{
				if (filteredText[i] == '|')
				{
					isInLineBrackets = !isInLineBrackets;
					continue;
				}

				if (isInLineBrackets)
				{
					dialogueText.maxVisibleWords++;
				}
				else if (filteredText[i] == '_')
				{
					yield return new WaitForSeconds(GetCurrentEntry().underscoreWaitTime);
				}
				else
				{
					dialogueText.maxVisibleWords++;
					yield return new WaitForSeconds(GetCurrentEntry().typeSpeed);
				}
			}
		}

		if (hasAnswer)
		{
			answer1Text.text = GetCurrentEntry().answer1.answer;
			answer2Text.text = GetCurrentEntry().answer2.answer;
			answerButtonAnimator.SetBool("isOpen", true);
		}

		if (GetCurrentEntry().dialogueEvent.when == DialogueEvent.eventWhen.afterTyping)
		{
			Dialogue_event_manager.instance.dialogueEvents[GetCurrentEntry().dialogueEvent.invokeEvent].Invoke();
		}

		HandleCurrentEntry();

		isTyping = false;
	}

	private void HandleCurrentEntry()
	{
		if (GetCurrentEntry().isLastMessage)
		{
			currentSentence++;
			currentDialogueEntry = 0;

			hasFinishedSentence = true;
			return;
		}

		currentDialogueEntry += 1 + GetCurrentEntry().jumpEntries;
		if (GetCurrentEntry() == null)
		{
			currentSentence++;
			currentDialogueEntry = 0;

			hasFinishedSentence = true;
			return;
		}
	}

	void FilterText()
	{
		unfilteredText.Clear();
		usedText.Clear();
		filteredText.Clear();

		for (int i = 0; i < GetCurrentEntry().text.ToCharArray().Length; i++)
		{
			unfilteredText.Add(GetCurrentEntry().text.ToCharArray()[i]);
		}

		//Get the text used by text mesh pro
		for (int i = 0; i < unfilteredText.Count; i++)
		{
			if(unfilteredText[i] == '|' || unfilteredText[i] == '_')
			{
				continue;
			}

			usedText.Add(unfilteredText[i]);
		}

		//Get the text used by special character commands such as waiting more before typing or typing without waiting
		isInAngledBrackets = false;
		for (int i = 0; i < unfilteredText.Count; i++)
		{
			if (unfilteredText[i] == '<' || unfilteredText[i] == '>')
			{
				isInAngledBrackets = !isInAngledBrackets;
				continue;
			}

			if (!isInAngledBrackets)
			{
				filteredText.Add(unfilteredText[i]);
			}
		}
	}

	public void EndDialogue()
	{
		textBubbleAnimator.SetBool("isOpen", false);
		dialogueText.text = null;

		dialogueActive = false;

		for (int i = 0; i < remember.Count; i++)
		{
			if (remember[i].name == currentDialogue.characterName)
			{
				remember[i].currentSentence = currentSentence;
				currentSentence = 0;
				return;
			}
		}

		DialogueRemember dr = new DialogueRemember();
		dr.name = currentDialogue.characterName;
		dr.currentSentence = currentSentence;

		currentSentence = 0;

		remember.Add(dr);
	}

	void SetCurrentSentence()
	{
		for (int i = 0; i < remember.Count; i++)
		{
			if (currentDialogue.characterName == remember[i].name)
			{
				currentSentence = remember[i].currentSentence;
				return;
			}
		}
	}

	public void Answer1Pressed()
	{
		answerButtonAnimator.SetBool("isOpen", false);
		currentDialogueEntry += currentDialogue.sentence[currentSentence].dialogueEntry[currentDialogueEntry - 1].answer1.jumpEntries;
		hasAnswer = false;
		ShowNextSentence();
	}

	public void Answer2Pressed()
	{
		answerButtonAnimator.SetBool("isOpen", false);
		currentDialogueEntry += currentDialogue.sentence[currentSentence].dialogueEntry[currentDialogueEntry - 1].answer2.jumpEntries;
		hasAnswer = false;
		ShowNextSentence();
	}

	DialogueEntry GetCurrentEntry()
	{
		int sentence = Mathf.Clamp(currentSentence, 0, currentDialogue.sentence.Length - 1);
		int dialogueEntry = Mathf.Clamp(currentDialogueEntry, 0, currentDialogue.sentence[sentence].dialogueEntry.Length - 1);

		if (currentDialogueEntry >= currentDialogue.sentence[sentence].dialogueEntry.Length)
		{
			return null;
		}
		else
		{
			return currentDialogue.sentence[sentence].dialogueEntry[dialogueEntry];
		}
	}

	Sentence GetCurrentSentence()
	{
		int sentence = Mathf.Clamp(currentSentence, 0, currentDialogue.sentence.Length - 1);

		if (currentSentence >= currentDialogue.sentence.Length)
		{
			return null;
		}
		else
		{
			return currentDialogue.sentence[sentence];
		}
	}

	bool SentenceInBounds(int index)
	{
		if((index >= 0) && (index < currentDialogue.sentence.Length))
		{
			return true;
		}
		else
		{
			if (currentDialogue.repeatLastEntry)
			{
				currentSentence--;
				return true;
			}
			return false;
		}
		//return (index >= 0) && (index < currentDialogue.sentence.Length);
	}
}