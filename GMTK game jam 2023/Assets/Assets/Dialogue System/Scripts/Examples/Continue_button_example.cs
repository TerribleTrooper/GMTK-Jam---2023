using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Continue_button_example : MonoBehaviour
{
	public KeyCode button = KeyCode.E;

	private void Update()
	{
		if(Dialogue_manager.instance.dialogueActive && Input.GetKeyDown(button))
		{
			Dialogue_manager.instance.ShowNextSentence();
		}
	}
}
