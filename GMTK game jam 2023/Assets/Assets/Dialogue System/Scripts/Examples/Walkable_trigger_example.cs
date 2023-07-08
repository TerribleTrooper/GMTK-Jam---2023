using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walkable_trigger_example : MonoBehaviour
{
	public Dialogue_trigger dialogueTrigger;
	public KeyCode interactKey = KeyCode.E;
	public string playerTag = "Player";
	bool canTalk = false;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag(playerTag))
		{
			canTalk = true;
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.CompareTag(playerTag))
		{
			canTalk = false;
		}
	}

	private void Update()
	{
		if (canTalk && Input.GetKeyDown(interactKey) && !Dialogue_manager.instance.dialogueActive)
		{
			dialogueTrigger.TriggerDialogue();
		}
	}
}
