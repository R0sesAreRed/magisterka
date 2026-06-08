using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TutorialRoute : MonoBehaviour
{
    public static TutorialRoute instance;
    public int currentTutorialStep = 0;
    void Awake()
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

    public void nextStepByValue(int step)
    {
        var allPopups = GameObject.FindObjectsByType<TutorialPopup>(FindObjectsSortMode.None);
        foreach (var popup in allPopups)
        {
            if (popup.myStep == step)
            {
                GameObject foundObject = popup.gameObject;
                foundObject.SetActive(true);
                Debug.Log("Znaleziono obiekt: " + foundObject.name);
                break;
            }
        }
    }
}
