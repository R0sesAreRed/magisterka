using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialPopup : MonoBehaviour
{
    public int myStep;
    public GameObject nextPopup;
    public string nextSceneTutorialRoute;
    public bool LastStep;
    void Start()
    {
        gameObject.transform.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
        if (TutorialRoute.instance.currentTutorialStep == myStep && !GameManager.instance.tutorialCompleted)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
        if(nextPopup != null)
        {
            gameObject.transform.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => nextStep(nextPopup));
        }
        else if (!string.IsNullOrEmpty(nextSceneTutorialRoute))
        {
            gameObject.transform.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => NavigateToScene(nextSceneTutorialRoute));
        }
        else if (LastStep)
        {
            gameObject.transform.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => completeTutorial());
        }
        gameObject.transform.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => gameObject.SetActive(false));
    }

    public void NavigateToScene(string nextScene)
    {
        SceneManager.LoadScene(nextScene);
        TutorialRoute.instance.currentTutorialStep++;
    }
    public void nextStep(GameObject GO)
    {
        GO.SetActive(true);
        TutorialRoute.instance.currentTutorialStep++;
    }
    public void completeTutorial()
    {
        GameManager.instance.tutorialCompleted = true;
        TutorialRoute.instance.currentTutorialStep++;
        AccountUtility.UpdateAccountTutorialCompleted(true);
    }
}
