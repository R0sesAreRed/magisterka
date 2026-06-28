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
    [SerializeField] Image progressBar;

    public AudioSource audioSource;
    public GameObject GameEndMenu;
    public GameObject GamePausedMenu;
    private float comboMeter = 0;
    private int comboMult;
    public FalingNotesSpawner FNS;
    public GameObject ScoreText;
    public GameObject ProgressBar;

    bool LevelComplete = false;
    public AudioClip loseHP;
    public AudioClip gainHP;

    public GameObject FeedbackTextPrefab;

    public Image ComboBar;
    public TextMeshProUGUI ComboText;

    public GameObject hearthsHolder;
    public GameObject comboHolder;

    public float ComboMeter                           //obgaddać jak z hp i może dodać tutaj opcję przegrywania
    {
        get {  return comboMeter; } 
        set 
        { 
            float clamped = Mathf.Clamp(value, 0, 5000);
            ComboBar.fillAmount = clamped / 5000f;
            comboMult = (int)(clamped / 1000) + 1;
            ComboText.text = comboMult.ToString() + "x";
            comboMeter = clamped; 
        }
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
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        if (!GameManager.instance.progressBarOn)
        {
            ProgressBar.SetActive(false);
        }

        if (GameManager.instance.comboOn)
        {
            hearthsHolder.SetActive(false);
            comboHolder.SetActive(true);
        }
        else
        {
            hearthsHolder.SetActive(true);
            comboHolder.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (displayScore < score)
        {
            displayScore += 1f; //0.1f
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

            if (GameManager.instance.gamificationOn)
            {
                StartCoroutine(SpawnFeedbackText(note, "Super!"));
                ComboMeter += 500;
            }               
            DataCollection.instance.PerfectNotes++;
            return GameManager.instance.gamificationOn? 100 : 100*comboMult;
        }
        else if (timeDifference+ lengthDifference <= 0.6f) // dobre trafienie 30
        {
            if (GameManager.instance.gamificationOn)
            {
                StartCoroutine(SpawnFeedbackText(note, "Dobrze!"));
                ComboMeter += 350;
            }
            DataCollection.instance.GoodNotes++;
            return GameManager.instance.gamificationOn ? 70 : 70 * comboMult;
        }
        else if (timeDifference + lengthDifference <= 0.9f) // s�abe trafienie 45
        {
            if (GameManager.instance.gamificationOn)
            {
                StartCoroutine(SpawnFeedbackText(note, "OK"));
                ComboMeter += 250;
            }        
            DataCollection.instance.OkNotes++;
            return GameManager.instance.gamificationOn ? 50 : 50 * comboMult;
        }
        else // nietrafienie
        {
            if (GameManager.instance.gamificationOn)
            {
                StartCoroutine(SpawnFeedbackText(note, "Pudło"));
                comboMeter = Mathf.Round(((comboMeter) / 1000) - 1) * 1000;
            }                
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
                Quaternion.identity
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

        GameEndMenu.SetActive(true);
    }
    public void TurnOffEndMenu()
    {
        GameEndMenu.SetActive(false);
    }

    public void TurnOnPauseMenu()
    {
        Debug.Log("[GameUIManager] TurnOnPauseMenu called. GamePausedMenu: " + (GamePausedMenu != null));
        if (GamePausedMenu != null)
        {
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

        }
        else
            Debug.LogWarning("[GameUIManager] GamePausedMenu is null!");
    }

    public void ReplaySong()
    { 

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
        }
        
        // Reset DataCollection counters
        if (DataCollection.instance != null)
        {
            DataCollection.instance.ResetCouters();
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
        
        
        // Reset progress bar
        progressBar.fillAmount = 0f;
        
        
        // Reset level completion flag
        LevelComplete = false;
        
        // Reset song timing
        songTimePlayed = 0;

    }

    public void ResetBPMmod()
    {
        GameManager.instance.BPMmod = 1.0;
    }
}