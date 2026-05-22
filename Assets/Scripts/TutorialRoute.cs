using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TutorialRoute : MonoBehaviour
{
    public static TutorialRoute instance;
    public int currentTutorialStep = 0;
    void Start()
    {
        if (instance == null && instance != this)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        Object.DontDestroyOnLoad(gameObject);
    }
}
