using UnityEngine;

public class SelectSongItemView : MonoBehaviour
{
    public SelectSongItem songData;
    private PopulateSongList songList;
    private SongSaveSystem saveSystem;
    public void Initialize(SelectSongItem item, PopulateSongList list, SongSaveSystem save)
    {
        songData = item;
        songList = list;
        saveSystem = save;
        var SelectButton = GetComponent<UnityEngine.UI.Button>();
        SelectButton.onClick.RemoveAllListeners();
        SelectButton.onClick.AddListener(SelectSong);

        transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = item.Title;
        transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = $"Najlepszy wynik: {item.HighScore}";
        transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = $"Maksymalny wynik: {item.MaxScore}";
        transform.GetChild(3).gameObject.SetActive(item.added);
        var deleteButton = transform.GetChild(3).GetComponent<UnityEngine.UI.Button>();
        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(DeleteSong);
    }

    public void DeleteSong()
    {
        // Usuñ z listy importowanych plików
        GameManager.instance.importedFiles.Remove(songData);

        // Zapisz zmienion¹ listê do pliku JSON
        saveSystem.Save(GameManager.instance.importedFiles);

        // Odœwie¿ listê w UI
        songList.RefreshList();
    }

    public void SelectSong()
    {
        GameManager.instance.currentSong = songData;
        Debug.Log($"Selected song: {GameManager.instance.currentSong.FilePath}");
    }
}
