using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuUIMAnager : MonoBehaviour
{
    public GameObject[] menuButtons;

    public TMPro.TextMeshProUGUI timeText;
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
