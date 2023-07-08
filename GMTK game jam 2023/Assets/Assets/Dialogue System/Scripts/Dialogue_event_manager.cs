using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Dialogue_event_manager : MonoBehaviour
{
	public static Dialogue_event_manager instance;
    public UnityEvent[] dialogueEvents;

	private void Awake()
	{
		instance = this;
	}
}
