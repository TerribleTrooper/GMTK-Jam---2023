using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuButtons : MonoBehaviour
{
	[SerializeField] string firslLevelSceneName;

	public void PlayGame()
	{
		SceneManager.LoadScene(firslLevelSceneName);
	}

	public void QuitGame()
	{
		Application.Quit();
	}
}
