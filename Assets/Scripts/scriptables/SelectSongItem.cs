using UnityEngine;

[CreateAssetMenu(fileName = "SelectSongItem", menuName = "Scriptable Objects/SelectSongItem")]
[System.Serializable]
public class SelectSongItem : ScriptableObject
{
    public string Title;
    public string FilePath;
    public int HighScore;
    public int MaxScore;
    public bool added = false;

    public void Initialize(SelectSongItem item, GameObject menuPrefab)
    {
        menuPrefab.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = item.Title;
        menuPrefab.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = $"Najlepszy wynik: {item.HighScore}";
        menuPrefab.transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = $"Maksymalny wynik: {item.MaxScore}";
        menuPrefab.transform.GetChild(3).gameObject.SetActive(item.added);
    }
}

