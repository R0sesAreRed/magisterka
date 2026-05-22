using UnityEngine;

[CreateAssetMenu(fileName = "SelectSongItem", menuName = "Scriptable Objects/SelectSongItem")]
[System.Serializable]
public class SelectSongItem : ScriptableObject
{
    public string Title;
    public string FilePath;
    public double BestScore;
    public int Level;
    public bool added = false;

    public void Initialize(SelectSongItem item, GameObject menuPrefab)
    {
        menuPrefab.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = item.Title;
        menuPrefab.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = $"Najlepszy wynik: {item.BestScore}%";
        if(GameManager.instance.levelsOn)
            menuPrefab.transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = $"Poziom: {item.Level:F1}%";
        else
            menuPrefab.transform.GetChild(2).gameObject.SetActive(false);
        menuPrefab.transform.GetChild(3).gameObject.SetActive(item.added);
    }
}

