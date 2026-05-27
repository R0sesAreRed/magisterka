using JetBrains.Annotations;
using System.Collections;
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
        Debug.Log("Popup start initiated");
        Debug.Log("current step : " + TutorialRoute.instance.currentTutorialStep + " my step " + myStep + " tutorialcompleted " + GameManager.instance.tutorialCompleted);
        if (TutorialRoute.instance.currentTutorialStep == myStep && !GameManager.instance.tutorialCompleted)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
        
    }

    public void NavigateToScene(string nextScene)
    {
        SceneManager.LoadScene(nextScene);
        
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

    public void nextStepSkipLevels(GameObject GO)
    {
        if (GameManager.instance.levelsOn)
        {
            GO.SetActive(true);
            TutorialRoute.instance.currentTutorialStep++;
        }
        else
        {
            nextPopup.SetActive(true);
            TutorialRoute.instance.currentTutorialStep += 2;
        }
    }

    public void nextStepAfterDelay(GameObject GO)
    {
        StartCoroutine(nextStepAfterDelayenum(GO));
    }
    private IEnumerator nextStepAfterDelayenum(GameObject GO)
    {
        yield return new WaitForSeconds(4.5f);
        Debug.Log("Activating next popup after delay");
        GO.SetActive(true);
        TutorialRoute.instance.currentTutorialStep++;
    }
}
