using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialogue_trigger : MonoBehaviour
{
    public Dialogue dialogue;

    public void TriggerDialogue()
	{
		Dialogue_manager.instance.StartDialogue(dialogue);
	}
}
