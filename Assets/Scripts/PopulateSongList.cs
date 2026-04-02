using UnityEngine;

public class PopulateSongList : MonoBehaviour
{
    [SerializeField] private GameObject songListItemPrefab;
    [SerializeField] private GameObject listParent;
    [SerializeField] private SongSaveSystem saveSystem = new SongSaveSystem();

    void Start()
    {
        // Wczytaj listê tylko raz na start
        GameManager.instance.importedFiles = saveSystem.Load();
        AddMissingSongsFromFolder();
        RefreshList();
    }

    public void AddMissingSongsFromFolder()
    {
        var folderSongs = Resources.LoadAll<SelectSongItem>("SongObjects");
        bool addedAny = false;

        foreach (var so in folderSongs)
        {
            bool exists = GameManager.instance.importedFiles.Exists(x => x.Title == so.Title && x.FilePath == so.FilePath);
            if (!exists)
            {
                GameManager.instance.importedFiles.Add(so);
                addedAny = true;
            }
        }

        if (addedAny)
            saveSystem.Save(GameManager.instance.importedFiles);
    }

    public void RefreshList()
    {
        foreach (Transform child in listParent.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var item in GameManager.instance.importedFiles)
        {
            var go = Instantiate(songListItemPrefab, listParent.transform);
            go.GetComponent<SelectSongItemView>().Initialize(item, this, saveSystem);
        }
    }
}
