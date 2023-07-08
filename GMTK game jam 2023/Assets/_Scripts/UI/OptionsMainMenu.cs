using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Options))]
public class OptionsMainMenu : MonoBehaviour
{
	Options options;
	[SerializeField] KeyCode exitKey = KeyCode.Escape;

	private void Awake()
	{
		options = GetComponent<Options>();
	}

	private void Update()
	{
		if (Input.GetKeyDown(exitKey))
		{
			options.optionsBackButton.onClick.Invoke();
		}
	}
}
