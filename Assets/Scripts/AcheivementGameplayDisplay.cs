using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AcheivementGameplayDisplay : MonoBehaviour
{
    private GameObject canvas;
    public GameObject AchievementDisplayPrefab;

    private bool isDisplaying = false;
    private Queue<AchievementData> displayQueue = new Queue<AchievementData>();

    void Start()
    {
        Object.DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        FindCanvasInScene();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindCanvasInScene();
    }

    private void FindCanvasInScene()
    {
        Canvas foundCanvas = FindFirstObjectByType<Canvas>();
        if (foundCanvas != null)
            canvas = foundCanvas.gameObject;
        else
            canvas = null;
    }
    public void DisplayAchievement(AchievementData achv)
    {
        displayQueue.Enqueue(achv);
        if (!isDisplaying)
            StartCoroutine(DisplayAchievementCoroutine());
    }

    private IEnumerator DisplayAchievementCoroutine()
    {
        isDisplaying = true;
        while (displayQueue.Count > 0)
        {
            AchievementData achv = displayQueue.Dequeue();
            if (canvas == null || AchievementDisplayPrefab == null)
                yield break;
            GameObject go = Instantiate(AchievementDisplayPrefab, canvas.transform);
            RectTransform rt = go.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = Vector2.zero;
            }
            go.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = achv.title;
            go.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = achv.description;
            yield return new WaitForSeconds(3f);
            Destroy(go);
        }
        isDisplaying = false;
    }
}
