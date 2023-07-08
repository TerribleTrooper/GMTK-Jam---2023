using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
	public static bool canPause = true;
	[SerializeField] KeyCode pauseButton = KeyCode.Escape;
	[SerializeField] GameObject pauseMenu;

	[Space(10)]
	[SerializeField] string mainMenuSceneName = "Main Menu";

	[Space(10)]
	[SerializeField] Options optionsMenu;

	bool isPaused = false;

	private void Start()
	{
		optionsMenu.IsInOptions = false;
		optionsMenu.optionsBackButton.onClick.Invoke();
		
		Pause(false);
	}

	void Update()
    {
		// Toggle isPaused variable
		if (canPause && Input.GetKeyDown(pauseButton))
		{
			if (!optionsMenu.IsInOptions)
			{
				isPaused = !isPaused;
				Pause(isPaused);
			}
			else
			{
				optionsMenu.optionsBackButton.onClick.Invoke();
			}
		}
	}

	public void Pause(bool pause)
	{
		isPaused = pause;
		if (pause)
		{
			pauseMenu.SetActive(true);
			Time.timeScale = 0;    // Also stop time
		}
		else
		{
			pauseMenu.SetActive(false);
			Time.timeScale = 1f;    // Also resume time
		}
	}

	public void BackToMainMenu()
	{
		SceneManager.LoadScene(mainMenuSceneName);
	}
}
