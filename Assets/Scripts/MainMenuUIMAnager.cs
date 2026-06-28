using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuUIMAnager : MonoBehaviour
{
    public GameObject[] menuButtons;

    public TMPro.TextMeshProUGUI timeText;


    public void QuitGame()
    {
        Application.Quit();
    }
}
