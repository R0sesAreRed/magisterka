using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuUIMAnager : MonoBehaviour
{
    public GameObject[] menuButtons;
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
    }

    public void SetObjectActiveOutsideTutorial(GameObject GO)
    {
        if (GameManager.instance.tutorialCompleted)
        {
            GO.SetActive(true);
        }
    }
}
