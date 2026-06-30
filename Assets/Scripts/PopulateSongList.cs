using NUnit.Framework;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PopulateSongList : MonoBehaviour
{
    [SerializeField] private GameObject songListItemPrefab;
    [SerializeField] private GameObject listParent;
    [SerializeField] private GameObject LevelSeparator;
    [SerializeField] private SongSaveSystem saveSystem = new SongSaveSystem();
    public GameObject SelectGamemodeMenu;
    public GameObject disclamerMenu;
    void Start()
    {
        GameManager.instance.importedFiles = saveSystem.Load();
        AddMissingSongsFromFolder();
        bool initilized = false;
        foreach(var song in GameManager.instance.importedFiles)
        {
            if(song.GamificationOn)
            {
                initilized = true;
                break;
            }
        }
        if(!initilized)
        {
            var groups = GameManager.instance.importedFiles.GroupBy(s => s.Level);

            foreach (var group in groups)
            {
                List<SelectSongItem> songsInGroup = group.ToList();
                Shuffle(songsInGroup);

                int trueCount = songsInGroup.Count / 2;
                if (songsInGroup.Count % 2 == 1 && Random.value > 0.5f)
                {
                    trueCount++;
                }

                for (int i = 0; i < songsInGroup.Count; i++)
                {
                    songsInGroup[i].GamificationOn = i < trueCount;
                }
            }

            saveSystem.Save(GameManager.instance.importedFiles);
        }
        RefreshList();
    }
    public void disclamerLoading()
    {
        if(GameManager.instance.noteClipsCopy.Count == 0)
        {
            disclamerMenu.SetActive(true);
        }
    }

    private void Shuffle<T>(List<T> list)
    {
        for(int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
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
        {
            saveSystem.Save(GameManager.instance.importedFiles);
        }
    }
    public void RefreshList()
    {
        foreach (Transform child in listParent.transform)
        {
            Destroy(child.gameObject);
        }

        var sorted = GameManager.instance.importedFiles.OrderBy(x => x.Level).ThenBy(x => x.Title).ToList();
        if (sorted.Count == 0) return;

        int minLevel = sorted.Min(x => x.Level);
        int currentLevel = int.MinValue;

        foreach (var item in sorted)
        {
            if (item.Level != currentLevel)
            {
                currentLevel = item.Level;
                var sep = Instantiate(LevelSeparator, listParent.transform);
                var child0 = sep.transform.childCount > 0 ? sep.transform.GetChild(0) : null;
                if (child0 != null)
                {
                    var sepText = child0.GetComponent<TMPro.TextMeshProUGUI>();
                    if (sepText != null)
                        sepText.text = $"Poziom: {currentLevel}";
                }
                var progressBarRoot = sep.transform.GetChild(1).gameObject;


                int prevLevel = currentLevel - 1;
                var prevSongs = sorted.Where(x => x.Level == prevLevel).ToList();
                float progress = 0f;

                if (prevSongs.Count > 0)
                {
                    double sumScores = prevSongs.Sum(s => s.BestScore);
                    progress = (float)(sumScores / (prevSongs.Count * 0.75f));
                }
                else
                {
                    sep.transform.GetChild(1).gameObject.SetActive(false);
                }
                sep.transform.GetChild(1).GetChild(0).GetComponent<Image>().fillAmount = Mathf.Clamp01(progress);                             
            }

            var go = Instantiate(songListItemPrefab, listParent.transform);
            var img = go.GetComponent<UnityEngine.UI.Image>();
            if (item == GameManager.instance.currentSong)
                img.color = new Color(0.8f, 0.8f, 1f);
            else
                img.color = new Color(1f, 0.6839622f, 0.68396f);

            // determine if this level should be interactable based on previous level progress
            bool interactable = true;
            if (item.Level != minLevel) // <- odblokowywanie poziomów
            {
                int prevLevel = item.Level - 1;
                var prevSongs = sorted.Where(x => x.Level == prevLevel).ToList();
                if (prevSongs.Count > 0)
                {

                    double sumScores = prevSongs.Sum(s => (float)s.BestScore);
                    Debug.Log($"Checking if level {item.Level} should be interactable based on previous level {prevLevel} progress: {sumScores} {prevSongs.Count * 0.80f}");
                    interactable = (sumScores >= (prevSongs.Count * 0.80f));
                }

            }

            var btn = go.GetComponent<UnityEngine.UI.Button>();
            btn.interactable = interactable;
            go.GetComponent<SelectSongItemView>().Initialize(item, this, saveSystem);
        }

    }
}
