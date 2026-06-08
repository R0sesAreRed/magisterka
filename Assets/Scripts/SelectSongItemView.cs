    using UnityEngine;
using UnityEngine.UI;

public class SelectSongItemView : MonoBehaviour
{
    public SelectSongItem songData;
    private PopulateSongList songList;
    private SongSaveSystem saveSystem;
    private bool ToggleOn = false;
    public void Initialize(SelectSongItem item, PopulateSongList list, SongSaveSystem save)
    {
        songData = item;
        songList = list;
        saveSystem = save;
        var SelectButton = GetComponent<UnityEngine.UI.Button>();

        transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = item.Title;
        transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = $"Najlepszy wynik: {item.BestScore:F1}%";
        transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = $"Poziom trudności: {item.Level}";
        transform.GetChild(3).gameObject.SetActive(songData.added);
    }

    public void DeleteSong()
    {
        // Usu� z listy importowanych plik�w
        GameManager.instance.importedFiles.Remove(songData);

        // Zapisz zmienion� list� do pliku JSON
        saveSystem.Save(GameManager.instance.importedFiles);

        // Od�wie� list� w UI
        songList.RefreshList();
    }


    public void SelectSong()
    {
        GameManager.instance.currentSong = songData;
        SelectsongUIManager.instance.GameModeSelection.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = $"{GameManager.instance.currentSong.Title}";
        SelectsongUIManager.instance.GameModeSelection.transform.GetChild(3).GetComponent<Button>().interactable =
        GameManager.instance.currentSong.Completed && (GameManager.instance.currentSong.BestScore >= 75.0 || !GameManager.instance.pointsOn);

        GameManager.instance.BPMmod = 1.0;
        SelectsongUIManager.instance.GameModeSelection.SetActive(true);
        
        Debug.Log($"Selected song: {GameManager.instance.currentSong.FilePath}");
        songList.RefreshList();
    }
}
