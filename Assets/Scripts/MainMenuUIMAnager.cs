using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuUIMAnager : MonoBehaviour
{
    public GameObject[] menuButtons;

    public TMPro.TextMeshProUGUI timeText;
    void Start()
    {
        DisableButtons();
    }

    public void DisableButtons()
    {
        menuButtons[0].GetComponent<Button>().interactable = (GameManager.instance.achievementsOn);
        menuButtons[1].GetComponent<Button>().interactable = (GameManager.instance.leaderBoardOn);
        menuButtons[2].GetComponent<Button>().interactable = (GameManager.instance.shopAndCurrencyOn);
        menuButtons[3].GetComponent<Button>().interactable = (GameManager.instance.rewardsAndCosmeticOn);
        menuButtons[4].GetComponent<Button>().interactable = (GameManager.instance.questsOn);
        menuButtons[5].GetComponent<Button>().interactable = DataCollection.instance.TotalTimePlayed >= 1800000;
        timeText.text = $"{(DataCollection.instance.TotalTimePlayed/60000):F1}/30 minut";
    }

    public void SetObjectActiveOutsideTutorial(GameObject GO)
    {
        if (GameManager.instance.tutorialCompleted)
        {
            GO.SetActive(true);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    // UI handler to start verification sequence
    // Note: verification should be requested by setting GameManager.instance.verification = true
    // and then loading the gameplay scene. GameUIManager will start verification on scene enter.
}
