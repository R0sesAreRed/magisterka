using UnityEngine;

public class SceneRouter : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
            Debug.Log($"Loading scene: {sceneName}");
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
