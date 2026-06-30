using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;

public class DataCollection : MonoBehaviour
{
    [Header("Firebase Firestore")]
    [SerializeField] private string firestoreCollectionName = "gameRunsRevised";
    [SerializeField] private bool signInAnonymously = true;
    [SerializeField] private bool initializeOnStart = true;

    [Header("Firebase AppOptions (for desktop)")]
    [Tooltip("App ID from Firebase Project settings (optional for desktop). Example: 1:1234567890:desktop:abcdef")] 
    [SerializeField] private string firebaseAppId = "";
    [Tooltip("API Key from Firebase Project settings (optional for desktop).")]
    [SerializeField] private string firebaseApiKey = "";
    [Tooltip("Project ID from Firebase Project settings (optional for desktop). Example: your-project-id")] 
    [SerializeField] private string firebaseProjectId = "";
    [Tooltip("Storage bucket if used (optional).")]
    [SerializeField] private string firebaseStorageBucket = "";

    public int TotalNotes;
    public int MissedNotes = 0;
    public int OkNotes = 0;
    public int GoodNotes = 0;
    public int PerfectNotes = 0;
    public int WrongNotes = 0;
    public double TotalTimePlayed = 0;
    public bool LevelSuccess = false;

    public static DataCollection instance;
    private FirebaseAuth firebaseAuth;
    private FirebaseFirestore firestore;
    private bool firebaseReady;
    private bool firebaseInitializing;
    private bool isProcessingQueue;
    private string queueFilePath;
    private string archiveFilePath;
    private SubmissionQueue submissionQueue = new SubmissionQueue();

    [Serializable]
    private class SubmissionRecord
    {
        public string DocumentId;
        public string SelectedAccount;
        public string SongTitle;
        public double BestScore;
        public double BPMmod;
        public int TotalNotes;
        public int MissedNotes;
        public int OkNotes;
        public int GoodNotes;
        public int PerfectNotes;
        public int WrongNotes;
        public double TotalTimePlayed;
        public bool LevelSuccess;
        public bool GamificationActive;
        public string SubmitionTime;
        public int AttemptCount;
        public string LastError;
    }

    [Serializable]
    private class SubmissionQueue
    {
        public List<SubmissionRecord> Pending = new List<SubmissionRecord>();
    }

    void Start()
    {
        if (instance == null && instance != this)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

        queueFilePath = Path.Combine(Application.persistentDataPath, "submission_queue.json");
        archiveFilePath = Path.Combine(Application.persistentDataPath, "submission_archive.jsonl");
        LoadQueueFromDisk();

        if (initializeOnStart)
        {
            _ = InitializeFirebaseAsync();
            _ = ProcessQueueAsync();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void SubmitData()
    {
        TotalNotes = GameManager.instance != null && GameManager.instance.CurrMidiNotes != null
            ? GameManager.instance.CurrMidiNotes.Count
            : TotalNotes;

        SubmissionRecord payload = BuildPayload();
        EnqueueSubmission(payload);
        _ = ProcessQueueAsync();
        ResetCouters();
    }

    private async Task InitializeFirebaseAsync()
    {
        if (firebaseInitializing || firebaseReady)
        {
            return;
        }

        firebaseInitializing = true;

        try
        {
            DependencyStatus dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (dependencyStatus != DependencyStatus.Available)
            {
                Debug.LogError($"[DataCollection] Firebase dependencies are not available: {dependencyStatus}");
                return;
            }

            // Create or reuse a FirebaseApp using AppOptions when running on desktop.
            FirebaseApp app = null;
            bool hasAppOptions = !string.IsNullOrWhiteSpace(firebaseAppId) || !string.IsNullOrWhiteSpace(firebaseApiKey) || !string.IsNullOrWhiteSpace(firebaseProjectId) || !string.IsNullOrWhiteSpace(firebaseStorageBucket);

            try
            {
                if (hasAppOptions)
                {
                    AppOptions options = new AppOptions
                    {
                        AppId = string.IsNullOrWhiteSpace(firebaseAppId) ? null : firebaseAppId,
                        ApiKey = string.IsNullOrWhiteSpace(firebaseApiKey) ? null : firebaseApiKey,
                        ProjectId = string.IsNullOrWhiteSpace(firebaseProjectId) ? null : firebaseProjectId,
                        StorageBucket = string.IsNullOrWhiteSpace(firebaseStorageBucket) ? null : firebaseStorageBucket
                    };

                    // If a default instance exists and its ProjectId matches, reuse it
                    if (FirebaseApp.DefaultInstance != null)
                    {
                        app = FirebaseApp.DefaultInstance;
                    }
                    else
                    {
                        app = FirebaseApp.Create(options, "DesktopApp");
                    }
                }
                else
                {
                    // No explicit options provided; use default instance or create a default app
                    app = FirebaseApp.DefaultInstance ?? FirebaseApp.Create(new AppOptions());
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[DataCollection] Could not create FirebaseApp with AppOptions: {ex.Message}. Falling back to DefaultInstance.");
                app = FirebaseApp.DefaultInstance;
            }

            firebaseAuth = FirebaseAuth.GetAuth(app);
            firestore = FirebaseFirestore.GetInstance(app);

            if (signInAnonymously && firebaseAuth.CurrentUser == null)
            {
                await firebaseAuth.SignInAnonymouslyAsync();
            }

            firebaseReady = true;
            Debug.Log("[DataCollection] Firebase initialized.");
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"[DataCollection] Firebase initialization failed: {exception}");
        }
        finally
        {
            firebaseInitializing = false;
        }
    }

    private SubmissionRecord BuildPayload()
    {
        SelectSongItem currentSong = GameManager.instance != null ? GameManager.instance.currentSong : null;

        return new SubmissionRecord
        {
            DocumentId = BuildDocumentId(),
            SelectedAccount = GameManager.instance != null ? GameManager.instance.SelectedAccount : string.Empty,
            SongTitle = currentSong != null ? currentSong.Title : string.Empty,
            BestScore = currentSong != null ? currentSong.BestScore : 0d,
            BPMmod = GameManager.instance != null ? GameManager.instance.BPMmod : 1d,
            TotalNotes = TotalNotes,
            MissedNotes = MissedNotes,
            OkNotes = OkNotes,
            GoodNotes = GoodNotes,
            PerfectNotes = PerfectNotes,
            WrongNotes = WrongNotes,
            TotalTimePlayed = TotalTimePlayed,
            LevelSuccess = LevelSuccess,
            GamificationActive = GameManager.instance.gamificationOn,
            SubmitionTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            LastError = string.Empty
        };
    }

    private async Task ProcessQueueAsync()
    {
        if (isProcessingQueue)
        {
            return;
        }

        isProcessingQueue = true;

        try
        {
            if (submissionQueue.Pending.Count == 0)
            {
                return;
            }

            if (!firebaseReady)
            {
                await InitializeFirebaseAsync();
            }

            if (!firebaseReady || firestore == null)
            {
                Debug.LogWarning("[DataCollection] Firebase is not ready. Pending submissions will stay on disk.");
                return;
            }

            while (submissionQueue.Pending.Count > 0)
            {
                SubmissionRecord record = submissionQueue.Pending[0];

                try
                {
                    await firestore.Collection(firestoreCollectionName)
                        .Document(record.DocumentId)
                        .SetAsync(ToFirestorePayload(record));

                    submissionQueue.Pending.RemoveAt(0);
                    SaveQueueToDisk();
                    AppendToArchive(record);
                    Debug.Log($"[DataCollection] Firebase submission succeeded: {firestoreCollectionName}/{record.DocumentId}. Remaining queue: {submissionQueue.Pending.Count}");
                }
                catch (System.Exception submitException)
                {
                    record.AttemptCount += 1;
                    record.LastError = submitException.Message;
                    submissionQueue.Pending[0] = record;
                    SaveQueueToDisk();
                    Debug.LogError($"[DataCollection] Firebase submission failed for {record.DocumentId}. It remains queued for retry. Error: {submitException}");
                    break;
                }
            }
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"[DataCollection] Queue processing failed: {exception}");
        }
        finally
        {
            isProcessingQueue = false;
        }
    }

    private Dictionary<string, object> ToFirestorePayload(SubmissionRecord record)
    {
        return new Dictionary<string, object>
        {
            { "SelectedAccount", record.SelectedAccount },
            { "SongTitle", record.SongTitle },
            { "BestScore", record.BestScore },
            { "BPMmod", record.BPMmod },
            { "TotalNotes", record.TotalNotes },
            { "MissedNotes", record.MissedNotes },
            { "OkNotes", record.OkNotes },
            { "GoodNotes", record.GoodNotes },
            { "PerfectNotes", record.PerfectNotes },
            { "WrongNotes", record.WrongNotes },
            { "TotalTimePlayed", record.TotalTimePlayed },
            { "LevelSuccess", record.LevelSuccess },
            { "SettingsString", record.GamificationActive },
            { "SubmitionTime", record.SubmitionTime },
        };
    }

    private void EnqueueSubmission(SubmissionRecord submission)
    {
        submissionQueue.Pending.Add(submission);
        SaveQueueToDisk();
        Debug.Log($"[DataCollection] Submission queued: {submission.DocumentId}. Queue length: {submissionQueue.Pending.Count}");
    }

    private void LoadQueueFromDisk()
    {
        try
        {
            if (!File.Exists(queueFilePath))
            {
                submissionQueue = new SubmissionQueue();
                return;
            }

            string json = File.ReadAllText(queueFilePath);
            submissionQueue = JsonUtility.FromJson<SubmissionQueue>(json);

            if (submissionQueue == null || submissionQueue.Pending == null)
            {
                submissionQueue = new SubmissionQueue();
            }

            if (submissionQueue.Pending.Count > 0)
            {
                Debug.Log($"[DataCollection] Loaded {submissionQueue.Pending.Count} queued submissions from disk.");
            }
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"[DataCollection] Failed to load queue from disk: {exception}");
            submissionQueue = new SubmissionQueue();
        }
    }

    private void SaveQueueToDisk()
    {
        try
        {
            string json = JsonUtility.ToJson(submissionQueue, true);
            File.WriteAllText(queueFilePath, json);
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"[DataCollection] Failed to save queue to disk: {exception}");
        }
    }

    private void AppendToArchive(SubmissionRecord record)
    {
        try
        {
            string archivedRecord = JsonUtility.ToJson(record);
            File.AppendAllText(archiveFilePath, archivedRecord + Environment.NewLine);
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"[DataCollection] Failed to append archive record: {exception}");
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
        {
            _ = ProcessQueueAsync();
            return;
        }

        SaveQueueToDisk();
    }

    private void OnApplicationQuit()
    {
        SaveQueueToDisk();
    }

    private string BuildDocumentId()
    {
        SelectSongItem currentSong = GameManager.instance != null ? GameManager.instance.currentSong : null;
        string accountPart = SanitizeDocumentId(GameManager.instance != null ? GameManager.instance.SelectedAccount : "default");
        string songPart = SanitizeDocumentId(currentSong != null ? currentSong.Title : "unknown_song");
        string timestampPart = System.DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
        string randomPart = System.Guid.NewGuid().ToString("N");

        return $"{accountPart}_{songPart}_{timestampPart}_{randomPart}";
    }

    private string SanitizeDocumentId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "default";
        }

        char[] forbiddenChars = { '.', '#', '$', '[', ']', '/', '\\', ' ' };
        foreach (char forbiddenChar in forbiddenChars)
        {
            value = value.Replace(forbiddenChar, '_');
        }

        return value.Trim();
    }

    public void ResetCouters()
    {     
        MissedNotes = 0;
        OkNotes = 0;
        GoodNotes = 0;
        PerfectNotes = 0;
        WrongNotes = 0;
        LevelSuccess = false;
    }

    /// <summary>
    /// Returns top N song titles that the specified account completed (LevelSuccess==true), ordered by number of successful attempts (desc).
    /// Falls back to an empty list on error or if Firebase isn't ready.
    /// </summary>
    public async Task<List<string>> GetTopSuccessfulSongsAsync(string accountName, int topN)
    {
        var result = new List<string>();
        if (!firebaseReady || firestore == null)
        {
            Debug.LogWarning("[DataCollection] Firebase not ready - cannot query top successful songs.");
            return result;
        }

        if (string.IsNullOrEmpty(accountName))
        {
            Debug.LogWarning("[DataCollection] Account name empty - cannot query top successful songs.");
            return result;
        }

        try
        {
            var counts = new Dictionary<string, int>();
            Query query = firestore.Collection(firestoreCollectionName)
                .WhereEqualTo("SelectedAccount", accountName)
                .WhereEqualTo("LevelSuccess", true);

            QuerySnapshot snapshot = await query.GetSnapshotAsync();
            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                if (doc.TryGetValue<string>("SongTitle", out var title))
                {
                    if (string.IsNullOrEmpty(title))
                        continue;
                    if (!counts.ContainsKey(title))
                        counts[title] = 0;
                    counts[title]++;
                }
            }

            result = counts.OrderByDescending(kv => kv.Value).Take(topN).Select(kv => kv.Key).ToList();
            return result;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[DataCollection] Error querying top successful songs: {ex}");
            return result;
        }
    }

    public async Task<double> GetMostFrequentBPMmodAsync(string accountName, string songTitle, double fallbackBpmmod = 1d)
    {
        if (!firebaseReady || firestore == null)
        {
            Debug.LogWarning("[DataCollection] Firebase not ready - cannot query BPMmod history.");
            return fallbackBpmmod;
        }

        if (string.IsNullOrEmpty(accountName) || string.IsNullOrEmpty(songTitle))
        {
            return fallbackBpmmod;
        }

        try
        {
            var counts = new Dictionary<double, int>();
            Query query = firestore.Collection(firestoreCollectionName)
                .WhereEqualTo("SelectedAccount", accountName)
                .WhereEqualTo("SongTitle", songTitle);

            QuerySnapshot snapshot = await query.GetSnapshotAsync();
            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                if (doc.TryGetValue<double>("BPMmod", out var bpmmod))
                {
                    if (!counts.ContainsKey(bpmmod))
                    {
                        counts[bpmmod] = 0;
                    }

                    counts[bpmmod]++;
                }
            }

            if (counts.Count == 0)
            {
                return fallbackBpmmod;
            }

            return counts.OrderByDescending(kv => kv.Value).ThenBy(kv => kv.Key).First().Key;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[DataCollection] Error querying BPMmod history: {ex}");
            return fallbackBpmmod;
        }
    }

}
