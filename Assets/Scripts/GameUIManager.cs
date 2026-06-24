using Melanchall.DryWetMidi.Interaction;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.InputSystem.Controls.AxisControl;
using static UnityEngine.Rendering.DebugUI;
using Unity.VisualScripting;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager instance;
    private readonly SongSaveSystem songSaveSystem = new SongSaveSystem();
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] Image progressBar;
    [SerializeField] TextMeshProUGUI feedbackText;

    [SerializeField] GameObject[] hearths;
    public AudioSource audioSource;

    public GameObject GameEndMenu;
    public GameObject GamePausedMenu;
    private float healthPoints = 3000;
    private float comboMeter = 0;
    public FalingNotesSpawner FNS;

    private string defaultPauseMenuTitle = string.Empty;
    private string verificationPauseMenuTitle = string.Empty;

    private readonly List<SelectSongItem> verificationSongs = new List<SelectSongItem>();
    private readonly Dictionary<string, double> verificationSongBpmMods = new Dictionary<string, double>();
    private int verificationCurrentSongIndex = -1;
    private bool verificationContinueRequested = false;
    private bool verificationTransitionInProgress = false;
    private bool verificationFlowActive = false;

    private string defaultEndMenuButtonText = string.Empty;
    private string defaultEndMenuTitleText = string.Empty;
    private bool defaultEndMenuChild3Active = true;
    private bool defaultEndMenuChild4Active = true;
    private bool defaultEndMenuChild1Active = true;
    private bool defaultEndMenuChild2Active = true;

    public GameObject ScoreText;
    public GameObject ProgressBar;
    public GameObject Feedback;

    public GameObject Background;
    bool LevelComplete = false;
    public AudioClip loseHP;
    public AudioClip gainHP;

    public GameObject FeedbackTextPrefab;

    public float HealthPoints
    {
        get { return healthPoints; }
        set
        {
            float clamped = Mathf.Clamp(value, 0, 3000);
            if (Mathf.Approximately(healthPoints, clamped))
                return;
            if ((healthPoints < 1000 && clamped > 1000) || (healthPoints < 2000 && clamped > 2000) || (healthPoints < 3000 && clamped >= 3000))
            {
                audioSource.PlayOneShot(gainHP);
            }
                healthPoints = clamped;
            //Debug.Log("healthPoints: " + healthPoints);
            for (int i = 0; i < hearths.Length; i++)
            {
                float hearthValue = Mathf.Clamp(healthPoints - i * 1000, 0, 1000) / 1000f;
                //Debug.Log("hearthValue: " + hearthValue);
                Transform mask = hearths[i].transform.Find("HearthMask");
                if (mask != null)
                {
                    Image maskImage = mask.GetComponent<Image>();
                    if (maskImage != null)
                        maskImage.fillAmount = hearthValue;
                }
                Transform outline = hearths[i].transform.Find("HearthOutline");
                if (outline != null)
                {
                    outline.gameObject.SetActive(hearthValue >= 1f);
                }
            }
            
            if(value <= 0 && !GameManager.instance.verification)
            {
                FNS.PauseEndLevel();
                OnLevelLose();
            }
        }
    }

    public float ComboMeter
    {
        get {  return comboMeter; } 
        set { comboMeter = value; }
    }
    public double totalSongTime = 0;
    public double songTimePlayed = 0;
    private int score = 0; //punkty zdobywane w czasie gry
    private double displayScore = 0; //punkty wy�wietlane na ekranie
    public int Score
    {
        get { return score; }
        set
        {
            score = value;    
        }
    }
    private void Awake()
    {
        Debug.Log($"[GameUIManager] Awake start. verification={(GameManager.instance != null && GameManager.instance.verification)}, GameEndMenu={(GameEndMenu != null)}, GamePausedMenu={(GamePausedMenu != null)}, ScoreText={(ScoreText != null)}, ProgressBar={(ProgressBar != null)}, Feedback={(Feedback != null)}, FNS={(FNS != null)}");
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        CachePauseMenuTitle();
        CacheEndMenuDefaults();
        if (GameManager.instance.verification)
        {
            Debug.Log("[GameUIManager] Verification mode active on Awake - applying verification UI state. removing hearths");
            foreach (var hearth in hearths)
            {
                hearth.SetActive(false);
            }
        }
        if (!GameManager.instance.pointsOn)
        {
            ScoreText.SetActive(false);
        }
        if(!GameManager.instance.progressBarOn)
        {
            ProgressBar.SetActive(false);
        }
        if(!GameManager.instance.hitQualityOn)
        {
            Feedback.SetActive(false);
        }
    }
    void Start()
    {
        // If verification was requested before entering this scene, start it automatically
        if (GameManager.instance != null && GameManager.instance.verification)
        {
            Debug.Log("[GameUIManager] Verification flag detected on scene enter — starting verification.");
            //StartVerification();
        }
        else
        {
            Debug.Log($"[GameUIManager] Start without verification. GameManager={(GameManager.instance != null)} verification={(GameManager.instance != null && GameManager.instance.verification)}");
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (displayScore < score)
        {
            displayScore += 1f; //0.1f
            scoreText.text = "Wynik: " + ((int)displayScore).ToString();
        }
        if (!GameManager.instance.IsPaused)
        {
            progressBar.fillAmount = (float)(songTimePlayed / totalSongTime);
            //Debug.Log("[GameUIManager] ProgressBar fill: " + progressBar.fillAmount + " songTimePlayed: " + songTimePlayed + " totalSongTime: " + totalSongTime);
        }
        if (songTimePlayed >= totalSongTime + 1500 && !LevelComplete)
        {
            LevelComplete = true;
            Debug.Log("[GameUIManager] OnLevelEnd called from Update");
            OnLevelComlete();
        }
    }


    public int CalculateScore(double hitTime, double noteStartTime, double releaseTime, double noteLength, GameManager.NK note)
    {
        double timeDifference = Mathf.Abs((float)(hitTime - noteStartTime));
        //double releaseDifference = Mathf.Abs((float)(releaseTime - (noteStartTime + noteLength)));
        double lengthDifference = Mathf.Abs((float)(Mathf.Abs((float)(noteStartTime - releaseTime)) - noteLength));
        Debug.Log($"Nuta {note} r�nica czasu: {timeDifference}, r�nica length: {lengthDifference}");
        if (timeDifference+ lengthDifference <= 0.3f) // idealne trafienie 15
        {
            //Debug.Log("Perfekcyje trafienie");
            feedbackText.text = "Perfekcyjnie!";
            StartCoroutine(SpawnFeedbackText(note, "Super!"));
            HealthPoints += 100;
            DataCollection.instance.PerfectNotes++;
            return 100;
        }
        else if (timeDifference+ lengthDifference <= 0.6f) // dobre trafienie 30
        {
            //Debug.Log("Dobre trafienie");
            feedbackText.text = "Dobrze!";
            StartCoroutine(SpawnFeedbackText(note, "Dobrze!"));
            HealthPoints += 70;
            DataCollection.instance.GoodNotes++;
            return 70;
        }
        else if (timeDifference + lengthDifference <= 0.9f) // s�abe trafienie 45
        {
            feedbackText.text = "OK";
            StartCoroutine(SpawnFeedbackText(note, "OK"));
            //Debug.Log("S�abe trafienie");
            HealthPoints += 50;
            DataCollection.instance.OkNotes++;
            return 50;
        }
        else // nietrafienie
        {
            feedbackText.text = "Nietrafione";
            StartCoroutine(SpawnFeedbackText(note, "Pudło"));
            Debug.Log("Losing hp from " + HealthPoints);
            HealthPoints = Mathf.Round((HealthPoints) / 1000) * 1000;
            Debug.Log("New hp: " + HealthPoints);
            DataCollection.instance.MissedNotes++;
            return 0;  
        }
        
    }

    public IEnumerator SpawnFeedbackText(GameManager.NK note, string text)
    {
        GameObject key = KeyboardManager.instance.KeysDict[note];

        if (key != null)
        {
            float lifetime = 1.5f;
            float elapsed = 0f;
            float slideDistance = 50f;

            RectTransform keyRect = key.GetComponent<RectTransform>();

            Vector3 startPosition = key.transform.position
                                  + new Vector3(0f, keyRect.rect.height / 2f - 30f, 0f);

            Vector3 endPosition = startPosition
                                + new Vector3(0f, slideDistance, 0f);

            GameObject feedbackObj = Instantiate(
                FeedbackTextPrefab,
                startPosition,
                Quaternion.identity,
                Feedback.transform
            );

            TMP_Text feedbackText = feedbackObj.GetComponent<TMP_Text>();
            feedbackText.text = text;

            Color startColor = feedbackText.color;

            while (elapsed < lifetime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / lifetime;

                feedbackObj.transform.position = Vector3.Lerp(startPosition, endPosition, t);

                Color currentColor = startColor;
                currentColor.a = Mathf.Lerp(startColor.a, 0f, t);
                feedbackText.color = currentColor;

                yield return null;
            }

            Destroy(feedbackObj);
        }
    }


    public void OnLevelComlete()
    {
        if(totalSongTime > 0)
        {
            DataCollection.instance.LevelSuccess = true;
            DataCollection.instance.TotalTimePlayed += totalSongTime;
            AccountUtility.UpdateAccountTimePlayed(totalSongTime);
            Metronome.instance.StopMetronome();
            TurnOnEndMenu();
            DataCollection.instance.SubmitData();
        }
        else
        {
            Metronome.instance.StopMetronome();
            TurnOnEndMenu();
            Debug.LogWarning("Total song time is zero or negative, cannot complete level.");
        }
    }
    public void OnLevelLose()
    {
        DataCollection.instance.TotalTimePlayed += songTimePlayed;
        AccountUtility.UpdateAccountTimePlayed(songTimePlayed);
        Metronome.instance.StopMetronome();
        TurnOnEndMenu();
        DataCollection.instance.SubmitData();
    }

    public void TurnOnEndMenu()
    {
        if (!verificationFlowActive)
        {
            RestoreEndMenuDefaults();
        }

        if (totalSongTime <= 0)
        {
            var titleText = GameEndMenu.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();
            var scoreText = GameEndMenu.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();
            var detailsObj = GameEndMenu.transform.GetChild(2).gameObject;
            var detailsText = detailsObj.GetComponent<TMPro.TextMeshProUGUI>();

            titleText.text = "Błąd utworu";
            scoreText.text = "Nie można obliczyć wyniku (czas utworu = 0).";

            detailsObj.SetActive(true);
            string songTitle = GameManager.instance.currentSong != null ? GameManager.instance.currentSong.Title : "(brak)";
            string songPath = GameManager.instance.currentSong != null ? GameManager.instance.currentSong.FilePath : "(brak)";
            detailsText.text = $"Zgłoszenie: sprawdź plik MIDI.\nTytuł: {songTitle}\nŚcieżka: {songPath}\nTotalNotes: {DataCollection.instance.TotalNotes}";

            if (verificationFlowActive)
            {
                ConfigureVerificationEndMenu();
            }

            GameEndMenu.SetActive(true);
            return;
        }

        GameEndMenu.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = LevelComplete ? "Wygrana!" : "Przegrana!";
        double levelScore = DataCollection.instance.TotalNotes > 0
            ? ((double)score / (double)(DataCollection.instance.TotalNotes * 100)) * 100
            : 0;
        Debug.Log("wynik: " + score + ", max wynik: " + DataCollection.instance.TotalNotes * 100 + ", procent: " + levelScore);
        if (GameManager.instance.currentSong != null)
        {
            GameManager.instance.currentSong.BestScore = System.Math.Max(GameManager.instance.currentSong.BestScore, levelScore);

            var storedSong = GameManager.instance.importedFiles.Find(song =>
                song.Title == GameManager.instance.currentSong.Title &&
                song.FilePath == GameManager.instance.currentSong.FilePath);

            if (storedSong != null)
            {
                storedSong.BestScore = GameManager.instance.currentSong.BestScore;
                // Set Completed flag when level is successfully completed
                if (LevelComplete)
                {
                    storedSong.Completed = true;
                    GameManager.instance.currentSong.Completed = true;
                }
            }

            songSaveSystem.Save(GameManager.instance.importedFiles);
        }
        GameEndMenu.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = $"Wynik: {levelScore:F1}%";

        if (verificationFlowActive)
        {
            ConfigureVerificationEndMenu();
        }

        GameEndMenu.SetActive(true);
    }
    public void TurnOffEndMenu()
    {
        GameEndMenu.SetActive(false);
    }

    private IEnumerator PrepareVerificationSong(int songIndex)
    {
        if (GameManager.instance == null)
            yield break;

        if (songIndex < 0 || songIndex >= verificationSongs.Count)
            yield break;

        Debug.Log($"[GameUIManager] PrepareVerificationSong begin. index={songIndex}, totalSongs={verificationSongs.Count}, title='{verificationSongs[songIndex].Title}'");

        verificationCurrentSongIndex = songIndex;
        var song = verificationSongs[songIndex];
        GameManager.instance.currentSong = song;

        // During verification, reset note visibility budget before each song.
        GameManager.instance.visibleNotes = 5;

        if (verificationSongBpmMods.TryGetValue(song.Title, out double storedBpmMod))
        {
            GameManager.instance.BPMmod = storedBpmMod;
        }
        else
        {
            GameManager.instance.BPMmod = 1d;
        }

        if (DataCollection.instance != null)
        {
            DataCollection.instance.ResetCouters();
        }

        ResetGameUIState();

        if (FNS != null)
        {
            FNS.ResetNotes();
            FNS.Pause();
        }

        if (MidiReader.instance != null)
        {
            Debug.Log($"[GameUIManager] Resetting MIDI for verification song '{song.Title}'.");
            MidiReader.instance.ResetMidiForNewSong();
        }
        else
        {
            Debug.LogWarning("[GameUIManager] MidiReader.instance is null while preparing verification song.");
        }

        TurnOffEndMenu();

        GameManager.instance.IsPaused = true;
        GameManager.instance.nextNoteIndex = 0;
        songTimePlayed = 0;

        float timeout = 10f;
        float start = Time.time;
        while ((GameManager.instance.CurrMidiNotes == null || GameManager.instance.CurrMidiNotes.Count == 0) && Time.time - start < timeout)
        {
            yield return null;
        }

        if (GameManager.instance.CurrMidiNotes == null || GameManager.instance.CurrMidiNotes.Count == 0)
        {
            Debug.LogWarning($"[GameUIManager] Could not load MIDI for verification song '{song.Title}'.");
            yield break;
        }

        Debug.Log($"[GameUIManager] MIDI loaded for verification song '{song.Title}'. noteCount={GameManager.instance.CurrMidiNotes.Count}, totalSongTime={totalSongTime}");

        if (KeyboardManager.instance != null)
        {
            Debug.Log($"[GameUIManager] Resetting KeyboardManager for verification song '{song.Title}'.");
            KeyboardManager.instance.ResetGameState();
        }
        else
        {
            Debug.LogWarning("[GameUIManager] KeyboardManager.instance is null while preparing verification song.");
        }

        GameManager.instance.songStartTime = Time.time;
        GameManager.instance.IsPaused = false;
        if (FNS != null)
        {
            FNS.Upause();
        }

        Debug.Log($"[GameUIManager] Verification song ready: '{song.Title}' with BPMmod {GameManager.instance.BPMmod}.");
    }
    public void TurnOnPauseMenu()
    {
        Debug.Log("[GameUIManager] TurnOnPauseMenu called. GamePausedMenu: " + (GamePausedMenu != null));
        if (GamePausedMenu != null)
        {
            ApplyPauseMenuState(GameManager.instance != null && GameManager.instance.verification);
            GamePausedMenu.SetActive(true);
        }
        else
            Debug.LogWarning("[GameUIManager] GamePausedMenu is null!");
    }

    public void TurnOffPauseMenu()
    {
        Debug.Log("[GameUIManager] TurnOffPauseMenu called. GamePausedMenu: " + (GamePausedMenu != null));
        if (GamePausedMenu != null)
        {
            GamePausedMenu.SetActive(false);
            ApplyPauseMenuState(false);
        }
        else
            Debug.LogWarning("[GameUIManager] GamePausedMenu is null!");
    }

    private void CachePauseMenuTitle()
    {
        if (GamePausedMenu == null || GamePausedMenu.transform.childCount == 0)
            return;

        var titleText = GamePausedMenu.transform.GetChild(0).GetComponent<TMP_Text>();
        if (titleText != null && string.IsNullOrEmpty(defaultPauseMenuTitle))
        {
            defaultPauseMenuTitle = titleText.text;
        }
    }

    private void CacheEndMenuDefaults()
    {
        if (GameEndMenu == null)
            return;

        if (GameEndMenu.transform.childCount > 3)
        {
            defaultEndMenuChild3Active = GameEndMenu.transform.GetChild(3).gameObject.activeSelf;
        }

        if (GameEndMenu.transform.childCount > 0)
        {
            var titleText = GameEndMenu.transform.GetChild(0).GetComponent<TMP_Text>();
            if (titleText != null)
            {
                defaultEndMenuTitleText = titleText.text;
            }
        }

        if (GameEndMenu.transform.childCount > 1)
        {
            defaultEndMenuChild1Active = GameEndMenu.transform.GetChild(1).gameObject.activeSelf;
        }

        if (GameEndMenu.transform.childCount > 2)
        {
            defaultEndMenuChild2Active = GameEndMenu.transform.GetChild(2).gameObject.activeSelf;
        }

        if (GameEndMenu.transform.childCount > 4)
        {
            var child4 = GameEndMenu.transform.GetChild(4);
            defaultEndMenuChild4Active = child4.gameObject.activeSelf;
            var buttonText = child4.GetComponentInChildren<TMP_Text>(true);
            if (buttonText != null)
            {
                defaultEndMenuButtonText = buttonText.text;
            }
        }
    }

    private string NormalizeSongTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return string.Empty;

        string decomposed = title.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);
        for (int i = 0; i < decomposed.Length; i++)
        {
            char c = decomposed[i];
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category == UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsLetterOrDigit(c))
                sb.Append(c);
        }

        return sb.ToString();
    }

    private SelectSongItem FindSongByTitleSmart(string title)
    {
        if (GameManager.instance == null || GameManager.instance.importedFiles == null || string.IsNullOrWhiteSpace(title))
            return null;

        // First pass: strict, case-insensitive title comparison.
        var strict = GameManager.instance.importedFiles.FirstOrDefault(s =>
            s != null &&
            !string.IsNullOrWhiteSpace(s.Title) &&
            string.Equals(s.Title.Trim(), title.Trim(), StringComparison.OrdinalIgnoreCase));
        if (strict != null)
            return strict;

        // Second pass: normalized (diacritics/punctuation-insensitive) comparison.
        string normalizedTarget = NormalizeSongTitle(title);
        var normalized = GameManager.instance.importedFiles.FirstOrDefault(s =>
            s != null &&
            !string.IsNullOrWhiteSpace(s.Title) &&
            NormalizeSongTitle(s.Title) == normalizedTarget);
        if (normalized != null)
            return normalized;

        // Third pass: relaxed containment to handle minor naming differences.
        return GameManager.instance.importedFiles.FirstOrDefault(s =>
            s != null &&
            !string.IsNullOrWhiteSpace(s.Title) &&
            (NormalizeSongTitle(s.Title).Contains(normalizedTarget) || normalizedTarget.Contains(NormalizeSongTitle(s.Title))));
    }

    private void EnsureVerificationSongCatalogLoaded()
    {
        if (GameManager.instance == null)
            return;

        if (GameManager.instance.importedFiles == null)
        {
            GameManager.instance.importedFiles = new List<SelectSongItem>();
        }

        if (GameManager.instance.importedFiles.Count == 0)
        {
            GameManager.instance.importedFiles = songSaveSystem.Load();
            if (GameManager.instance.importedFiles == null)
            {
                GameManager.instance.importedFiles = new List<SelectSongItem>();
            }
        }

        var folderSongs = Resources.LoadAll<SelectSongItem>("SongObjects");
        bool addedAny = false;
        foreach (var so in folderSongs)
        {
            if (so == null)
                continue;

            bool exists = GameManager.instance.importedFiles.Exists(x =>
                x != null &&
                string.Equals(x.Title, so.Title, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.FilePath, so.FilePath, StringComparison.OrdinalIgnoreCase));

            if (!exists)
            {
                GameManager.instance.importedFiles.Add(so);
                addedAny = true;
            }
        }

        if (addedAny)
        {
            songSaveSystem.Save(GameManager.instance.importedFiles);
        }
    }

    private void RestoreEndMenuDefaults()
    {
        if (GameEndMenu == null)
            return;

        if (GameEndMenu.transform.childCount > 3)
        {
            GameEndMenu.transform.GetChild(3).gameObject.SetActive(defaultEndMenuChild3Active);
        }

        if (GameEndMenu.transform.childCount > 0)
        {
            var titleText = GameEndMenu.transform.GetChild(0).GetComponent<TMP_Text>();
            if (titleText != null && !string.IsNullOrEmpty(defaultEndMenuTitleText))
            {
                titleText.text = defaultEndMenuTitleText;
            }
        }

        if (GameEndMenu.transform.childCount > 1)
        {
            GameEndMenu.transform.GetChild(1).gameObject.SetActive(defaultEndMenuChild1Active);
        }

        if (GameEndMenu.transform.childCount > 2)
        {
            GameEndMenu.transform.GetChild(2).gameObject.SetActive(defaultEndMenuChild2Active);
        }

        if (GameEndMenu.transform.childCount > 4)
        {
            var child4 = GameEndMenu.transform.GetChild(4);
            child4.gameObject.SetActive(defaultEndMenuChild4Active);

            var buttonText = child4.GetComponentInChildren<TMP_Text>(true);
            if (buttonText != null && !string.IsNullOrEmpty(defaultEndMenuButtonText))
            {
                buttonText.text = defaultEndMenuButtonText;
            }
        }
    }

    private void ConfigureVerificationEndMenu()
    {
        if (GameEndMenu == null)
            return;

        if (GameEndMenu.transform.childCount > 0)
        {
            var titleText = GameEndMenu.transform.GetChild(0).GetComponent<TMP_Text>();
            if (titleText != null)
            {
                string nextTitle = verificationCurrentSongIndex + 1 < verificationSongs.Count
                    ? verificationSongs[verificationCurrentSongIndex + 1].Title
                    : string.Empty;
                titleText.text = !string.IsNullOrWhiteSpace(nextTitle)
                    ? $"Następna melodia: {nextTitle}"
                    : "Weryfikacja zakończona";
            }
        }

        if (GameEndMenu.transform.childCount > 1)
        {
            GameEndMenu.transform.GetChild(1).gameObject.SetActive(false);
        }

        if (GameEndMenu.transform.childCount > 2)
        {
            GameEndMenu.transform.GetChild(2).gameObject.SetActive(false);
        }

        if (GameEndMenu.transform.childCount > 3)
        {
            GameEndMenu.transform.GetChild(3).gameObject.SetActive(false);
        }

        if (GameEndMenu.transform.childCount <= 4)
            return;

        var child4 = GameEndMenu.transform.GetChild(4);
        bool isLastVerificationSong = verificationCurrentSongIndex >= verificationSongs.Count - 1;
        child4.gameObject.SetActive(!isLastVerificationSong);
        if (isLastVerificationSong)
            return;

        var buttonText = child4.GetComponentInChildren<TMP_Text>(true);
        if (buttonText != null)
        {
            buttonText.text = "kontunuuj";
        }
    }

    private IEnumerator ContinueVerificationFromButtonCoroutine()
    {
        verificationTransitionInProgress = true;
        verificationContinueRequested = false;
        TurnOffEndMenu();

        int nextIndex = verificationCurrentSongIndex + 1;
        if (nextIndex < verificationSongs.Count)
        {
            // Switch song and rerun MidiReader when the continue button is pressed.
            yield return StartCoroutine(PrepareVerificationSong(nextIndex));
        }

        verificationContinueRequested = true;
        verificationTransitionInProgress = false;
    }

    private void ApplyPauseMenuState(bool verificationMode)
    {
        if (GamePausedMenu == null || GamePausedMenu.transform.childCount < 3)
            return;

        var titleText = GamePausedMenu.transform.GetChild(0).GetComponent<TMP_Text>();
        if (titleText != null)
        {
            titleText.text = verificationMode && !string.IsNullOrWhiteSpace(verificationPauseMenuTitle)
                ? verificationPauseMenuTitle
                : defaultPauseMenuTitle;
        }

        GamePausedMenu.transform.GetChild(2).gameObject.SetActive(!verificationMode);
    }

    public void ShowVerificationPauseMenu(string songTitle)
    {
        verificationPauseMenuTitle = songTitle;
        if (GameManager.instance != null)
        {
            GameManager.instance.IsPaused = true;
        }

        if (FNS != null)
        {
            FNS.Pause();
        }

        TurnOnPauseMenu();
    }

    /// <summary>
    /// Replays the current song by resetting all game state values and reinitializing the game
    /// </summary>
    public void ReplaySong()
    {
        if (verificationFlowActive)
        {
            if (!verificationTransitionInProgress)
            {
                StartCoroutine(ContinueVerificationFromButtonCoroutine());
            }
            return;
        }

        Debug.Log("[GameUIManager] ReplaySong called");
        
        // Reset GameUIManager state
        ResetGameUIState();
        
        // Reset KeyboardManager state
        if (KeyboardManager.instance != null)
        {
            KeyboardManager.instance.ResetGameState();
        }
        
        // Reset FalingNotesSpawner state and destroy all notes
        if (FNS != null)
        {
            FNS.ResetNotes();
        }
        
        // Reset GameManager state
        if (GameManager.instance != null)
        {
            GameManager.instance.songStartTime = Time.time;
            GameManager.instance.nextNoteIndex = 0;
            GameManager.instance.completedQuests = 0;
        }
        
        // Reset DataCollection counters
        if (DataCollection.instance != null)
        {
            DataCollection.instance.ResetCouters();
        }
        if(GameManager.instance.singleSongVerifying)
        {
            GameManager.instance.visibleNotes = 5;
        }

        // Hide end menu and unpause the game
        Metronome.instance.ResumeMetronome();
        TurnOffEndMenu();
        GameManager.instance.IsPaused = false;
        
        Debug.Log("[GameUIManager] ReplaySong completed - game is ready to play");
    }
    private void ResetGameUIState()
    {
        // Reset score
        score = 0;
        displayScore = 0;
        scoreText.text = "Wynik: 0";
        
        // Reset health to full
        healthPoints = 3000;
        
        // Reset progress bar
        progressBar.fillAmount = 0f;
        
        // Reset feedback text
        feedbackText.text = "";
        
        // Reset level completion flag
        LevelComplete = false;
        
        // Reset song timing
        songTimePlayed = 0;
        
        // Reset all hearts display
        for (int i = 0; i < hearths.Length; i++)
        {
            Transform mask = hearths[i].transform.Find("HearthMask");
            if (mask != null)
            {
                Image maskImage = mask.GetComponent<Image>();
                if (maskImage != null)
                    maskImage.fillAmount = 1f;
            }
            Transform outline = hearths[i].transform.Find("HearthOutline");
            if (outline != null)
            {
                outline.gameObject.SetActive(true);
            }
        }
        
        Debug.Log("[GameUIManager] GameUI state reset complete");
    }

    public void ResetBPMmod()
    {
        GameManager.instance.BPMmod = 1.0;
    }
}