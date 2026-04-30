using System.Collections;
using UnityEngine;

public class MainMenuUIMAnager : MonoBehaviour
{
    public GameObject[] menuButtons;
    void Start()
    {
        DisableButtons();
    }

    public void DisableButtons()
    { 
        menuButtons[0].SetActive(GameManager.instance.achievementsOn);
        menuButtons[1].SetActive(GameManager.instance.leaderBoardOn);
        menuButtons[2].SetActive(GameManager.instance.shopAndCurrencyOn);
        menuButtons[3].SetActive(GameManager.instance.rewardsAndCosmeticOn);
    }
}
