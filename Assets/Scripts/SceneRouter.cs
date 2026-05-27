using UnityEngine;

public class SceneRouter : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        if(GameManager.instance.tutorialCompleted)
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void LoadSceneTutorial(string sceneName)
    {
        if (!GameManager.instance.tutorialCompleted)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            TutorialRoute.instance.currentTutorialStep++;
        }

    }
}
