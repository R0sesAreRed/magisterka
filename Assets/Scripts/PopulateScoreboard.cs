using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopulateScoreboard : MonoBehaviour
{
    public GameObject playerScorePrefab;
    public GameObject scoreboardParent;

    [Serializable]
    private class LeaderboardEntry
    {
        public string PlayerName;
        public int TotalScore;
        public Dictionary<string, int> BestScoreBySongTitle = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    }


    void Start()
    {
        RefreshList();
    }

    public async void RefreshList()
    {
        if (scoreboardParent == null || playerScorePrefab == null)
        {
            Debug.LogWarning("[PopulateScoreboard] Missing prefab or parent reference.");
            return;
        }

        ClearList();

        try
        {
            DependencyStatus dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (dependencyStatus != DependencyStatus.Available)
            {
                Debug.LogError($"[PopulateScoreboard] Firebase dependencies are not available: {dependencyStatus}");
                return;
            }

            FirebaseFirestore firestore = FirebaseFirestore.DefaultInstance;
            QuerySnapshot snapshot = await firestore.Collection("gameRuns").GetSnapshotAsync();

            Dictionary<string, LeaderboardEntry> totalsByPlayer = new Dictionary<string, LeaderboardEntry>(StringComparer.OrdinalIgnoreCase);

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (!document.Exists)
                {
                    continue;
                }

                Dictionary<string, object> data = document.ToDictionary();
                if (!TryGetString(data, "SelectedAccount", out string playerName) || string.IsNullOrWhiteSpace(playerName))
                {
                    continue;
                }

                if (!TryGetSettingsString(data, out string settingsString) || settingsString.Length <= 7 || settingsString[7] == '0')
                {
                    continue;
                }

                if (!TryGetString(data, "SongTitle", out string songTitle) || string.IsNullOrWhiteSpace(songTitle))
                {
                    continue;
                }

                int runScore = CalculateRunScore(data);

                if (!totalsByPlayer.TryGetValue(playerName, out LeaderboardEntry entry))
                {
                    entry = new LeaderboardEntry
                    {
                        PlayerName = playerName,
                        TotalScore = 0
                    };
                    totalsByPlayer.Add(playerName, entry);
                }

                if (!entry.BestScoreBySongTitle.TryGetValue(songTitle, out int bestForSong) || runScore > bestForSong)
                {
                    entry.BestScoreBySongTitle[songTitle] = runScore;
                }
            }

            foreach (LeaderboardEntry entry in totalsByPlayer.Values)
            {
                entry.TotalScore = entry.BestScoreBySongTitle.Values.Sum();
            }

            List<LeaderboardEntry> sortedEntries = totalsByPlayer.Values
                .OrderByDescending(entry => entry.TotalScore)
                .ThenBy(entry => entry.PlayerName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            string currentPlayerName = GameManager.instance != null ? GameManager.instance.SelectedAccount : string.Empty;

            foreach (LeaderboardEntry entry in sortedEntries)
            {
                GameObject row = Instantiate(playerScorePrefab, scoreboardParent.transform);

                if (row.transform.childCount > 0)
                {
                    TMP_TextOrText(row.transform.GetChild(0), entry.PlayerName);
                }

                if (row.transform.childCount > 1)
                {
                    TMP_TextOrText(row.transform.GetChild(1), entry.TotalScore.ToString());
                }

                bool isCurrentPlayer = !string.IsNullOrWhiteSpace(currentPlayerName) &&
                                       string.Equals(entry.PlayerName, currentPlayerName, StringComparison.OrdinalIgnoreCase);

                ApplyRowHighlight(row, isCurrentPlayer);
            }
        }
        catch (Exception exception)
        {
            Debug.LogError($"[PopulateScoreboard] Failed to refresh leaderboard: {exception}");
        }
    }

    private void ClearList()
    {
        for (int i = scoreboardParent.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(scoreboardParent.transform.GetChild(i).gameObject);
        }
    }

    private int CalculateRunScore(Dictionary<string, object> data)
    {
        int perfectHits = GetInt(data, "PerfectNotes");
        int goodHits = GetInt(data, "GoodNotes");
        int okHits = GetInt(data, "OkNotes");
        int missedHits = GetInt(data, "MissedNotes");

        return (perfectHits * 100) + (goodHits * 70) + (okHits * 40) + (missedHits * 0);
    }

    private static int GetInt(Dictionary<string, object> data, string key)
    {
        if (!data.TryGetValue(key, out object value) || value == null)
        {
            return 0;
        }

        try
        {
            return Convert.ToInt32(value);
        }
        catch
        {
            return 0;
        }
    }

    private static bool TryGetString(Dictionary<string, object> data, string key, out string result)
    {
        result = string.Empty;
        if (!data.TryGetValue(key, out object value) || value == null)
        {
            return false;
        }

        result = value.ToString();
        return true;
    }

    private static bool TryGetSettingsString(Dictionary<string, object> data, out string result)
    {
        if (TryGetString(data, "SettingsString", out result))
        {
            return true;
        }

        // Fallback for older writes if field casing ever differs.
        return TryGetString(data, "settingsString", out result);
    }

    private void ApplyRowHighlight(GameObject row, bool isCurrentPlayer)
    {
        Color highlightColor = new Color(0.76f, 0.92f, 1f, 1f);
        Color normalColor = new Color(1f, 0.988934f, 0.5707547f, 1f);
        Color targetColor = isCurrentPlayer ? highlightColor : normalColor;

        Image rowImage = row.GetComponent<Image>();
        if (rowImage != null)
        {
            rowImage.color = targetColor;
        }
    }

    private void TMP_TextOrText(Transform child, string value)
    {
        TMP_Text tmpText = child.GetComponent<TMP_Text>();
        if (tmpText != null)
        {
            tmpText.text = value;
            return;
        }

        UnityEngine.UI.Text uiText = child.GetComponent<UnityEngine.UI.Text>();
        if (uiText != null)
        {
            uiText.text = value;
        }
    }
}
