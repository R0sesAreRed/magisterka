using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PopulateSongList : MonoBehaviour
{
    [SerializeField] private GameObject songListItemPrefab;
    [SerializeField] private GameObject listParent;
    [SerializeField] private GameObject LevelSeparator;
    [SerializeField] private SongSaveSystem saveSystem = new SongSaveSystem();

    void Start()
    {
        // Wczytaj list� tylko raz na start
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
                if (GameManager.instance.levelsOn)
                {
                    var sep = Instantiate(LevelSeparator, listParent.transform);
                    var child0 = sep.transform.childCount > 0 ? sep.transform.GetChild(0) : null;
                    if (child0 != null)
                    {
                        var sepText = child0.GetComponent<TMPro.TextMeshProUGUI>();
                        if (sepText != null)
                            sepText.text = $"Poziom: {currentLevel}";
                    }
                    if(GameManager.instance.progressBarOn)
                    {
                        var progressBarRoot = sep.transform.GetChild(1).gameObject;
                        bool showProgressBar = currentLevel > 0;
                        progressBarRoot.SetActive(showProgressBar);

                        if (showProgressBar)
                        {
                            int prevLevel = currentLevel - 1;
                            var prevSongs = sorted.Where(x => x.Level == prevLevel).ToList();
                            float progress = 0f;

                            if (prevSongs.Count > 0)
                            {
                                if (!GameManager.instance.pointsOn)
                                {
                                    int completedCount = prevSongs.Count(s => s.Completed);
                                    progress = (float)completedCount / prevSongs.Count;
                                }
                                else
                                {
                                    double sumScores = prevSongs.Sum(s => s.BestScore);
                                    progress = (float)(sumScores / (prevSongs.Count * 50.0));
                                }
                            }

                            sep.transform.GetChild(1).GetChild(0).GetComponent<Image>().fillAmount = Mathf.Clamp01(progress); //here
                        }
                    }
                }
            }

            var go = Instantiate(songListItemPrefab, listParent.transform);
            var img = go.GetComponent<UnityEngine.UI.Image>();
            if (item == GameManager.instance.currentSong)
                img.color = new Color(0.8f, 0.8f, 1f);
            else
                img.color = new Color(1f, 0.6839622f, 0.68396f);

            // determine if this level should be interactable based on previous level progress
            bool interactable = true;
            if (GameManager.instance.levelsOn && item.Level != minLevel)
            {
                int prevLevel = item.Level - 1;
                var prevSongs = sorted.Where(x => x.Level == prevLevel).ToList();
                if (prevSongs.Count > 0)
                {
                    if (!GameManager.instance.pointsOn)
                    {
                        // Use Completed flag: more than 50% of songs must be completed
                        int completedCount = prevSongs.Count(s => s.Completed);
                        interactable = completedCount > (prevSongs.Count * 0.5);
                    }
                    else
                    {
                        // Use BestScore: at least 50% of points must be collected in aggregate
                        double sumScores = prevSongs.Sum(s => s.BestScore);
                        interactable = sumScores >= (prevSongs.Count * 50.0);
                    }
                }
            }

            var btn = go.GetComponent<UnityEngine.UI.Button>();
            if (btn != null)
            {
                btn.interactable = interactable;
            }

            go.GetComponent<SelectSongItemView>().Initialize(item, this, saveSystem);

            // delete button handled inside SelectSongItemView.Initialize (only active for imported items)
        }

    }
}
