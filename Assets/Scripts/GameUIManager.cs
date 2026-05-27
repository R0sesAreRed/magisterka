using Melanchall.DryWetMidi.Interaction;
using System;
using System.Linq;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.InputSystem.Controls.AxisControl;
using static UnityEngine.Rendering.DebugUI;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager instance;
    private readonly SongSaveSystem songSaveSystem = new SongSaveSystem();
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] Image progressBar;
    [SerializeField] TextMeshProUGUI feedbackText;

    [SerializeField] GameObject[] hearths;

    public GameObject GameEndMenu;
    public GameObject GamePausedMenu;
    private float healthPoints = 3000;
    public FalingNotesSpawner FNS;

    public GameObject ScoreText;
    public GameObject ProgressBar;
    public GameObject Feedback;

    public GameObject Background;
    bool LevelComplete = false;


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
                GameEvents.OnRegainedHp.Invoke(1);
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
            
            if(value <= 0)
            {
                FNS.PauseEndLevel();
                OnLevelLose();
            }
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

        if(!GameManager.instance.pointsOn)
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
        if(!GameManager.instance.rewardsAndCosmeticOn)
        {
            var fontcosmetic = GameManager.instance.playerEquippedCosmetics.Find(c => c.type == CosmeticType.Font);
            if(fontcosmetic!= null && ScoreText.activeSelf)
            {
                ScoreText.GetComponent<TMP_Text>().font = fontcosmetic.font;
                Feedback.GetComponent<TMP_Text>().font = fontcosmetic.font;
            }
            var backgroundCosmetic = GameManager.instance.playerEquippedCosmetics.Find(c => c.type == CosmeticType.Background);
            if(backgroundCosmetic!= null && Background.activeSelf)
            {
                Background.GetComponent<Image>().sprite = backgroundCosmetic.sprite;
                Background.GetComponent<Image>().color = backgroundCosmetic.colorWhite;
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (displayScore < score)
        {
            displayScore += 1f; //0.1f
            scoreText.text = "Score: " + ((int)displayScore).ToString();
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


    public int CalculateScore(double hitTime, double noteStartTime, double releaseTime, double noteLength, GameManager.NK note) //pobawi� si� tym �eby dobrze czu�o r�nice
    {
        double timeDifference = Mathf.Abs((float)(hitTime - noteStartTime));
        double releaseDifference = Mathf.Abs((float)(releaseTime - (noteStartTime + noteLength)));
        Debug.Log($"Nuta {note} r�nica czasu: {timeDifference}, r�nica release: {releaseDifference}");
        if (timeDifference+releaseDifference <= 0.2f) // idealne trafienie 15
        {
            //Debug.Log("Perfekcyje trafienie");
            feedbackText.text = "Perfekcyjnie!";
            HealthPoints += 100;
            GameEvents.OnPerfectHit?.Invoke(1);
            GameEvents.OnHit?.Invoke(1);
            DataCollection.instance.PerfectNotes++;
            return 100;
        }
        else if (timeDifference+releaseDifference <= 0.4f) // dobre trafienie 30
        {
            //Debug.Log("Dobre trafienie");
            feedbackText.text = "Dobrze!";
            HealthPoints += 70;
            GameEvents.SetAchievementValue?.Invoke(AchievementRestriction.HitQuality, 0);
            GameEvents.OnHit?.Invoke(1);
            DataCollection.instance.GoodNotes++;
            return 70;
        }
        else if (timeDifference + releaseDifference <= 0.6f) // s�abe trafienie 45
        {
            feedbackText.text = "OK";
            //Debug.Log("S�abe trafienie");
            HealthPoints += 50;
            GameEvents.SetAchievementValue?.Invoke(AchievementRestriction.HitQuality, 0);
            GameEvents.OnHit?.Invoke(1);
            DataCollection.instance.OkNotes++;
            return 50;
        }
        else // nietrafienie
        {
            feedbackText.text = "Nietrafione";
            Debug.Log("Losing hp from " + HealthPoints);
            HealthPoints = Mathf.Round((HealthPoints) / 1000) * 1000;
            Debug.Log("New hp: " + HealthPoints);
            GameEvents.SetAchievementValue?.Invoke(AchievementRestriction.HitQuality, 0);
            GameEvents.SetAchievementValue?.Invoke(AchievementRestriction.HitAccuracy, 0);
            DataCollection.instance.MissedNotes++;
            return 0;  
        }
        
    }

    public void OnLevelComlete()
    {
        if(totalSongTime > 0)
        {
            DataCollection.instance.LevelSuccess = true;
            DataCollection.instance.TotalTimePlayed += totalSongTime;
            AccountUtility.UpdateAccountTimePlayed(totalSongTime);
            CosmeticsData rewardCosmetic = null;
            if (GameManager.instance.achievementsOn)
            {
                GameEvents.OnCompleteLevel.Invoke(1);
                if (DataCollection.instance.MissedNotes == 0)
                    GameEvents.OnCompleteLevelHealthQuality.Invoke(1);
                GameEvents.OnCompleteLevelQuality.Invoke((int)((DataCollection.instance.PerfectNotes / DataCollection.instance.TotalNotes) * 100));
            }
            if (GameManager.instance.questsOn)
            {
                QuestEvents.ProgressQuest.Invoke(QuestRestriction.Melodie, 1);
                QuestEvents.ProgressQuest.Invoke(QuestRestriction.MelodieFinish, 1);
                if (DataCollection.instance.MissedNotes == 0)
                    QuestEvents.SingleLevelProgress.Invoke(QuestRestriction.NoMisses, 1);
                if (DataCollection.instance.TotalNotes * 60 <= score)
                    QuestEvents.SingleLevelProgress.Invoke(QuestRestriction.Score, 1);
                QuestEvents.SingleLevelProgress.Invoke(QuestRestriction.PerfectHits, DataCollection.instance.PerfectNotes);
            }
            else if (GameManager.instance.shopAndCurrencyOn)
            {
                GameManager.instance.currency += 100;
                AccountUtility.UpdateAccountCurrency(GameManager.instance.currency);
            }
            else if (GameManager.instance.rewardsAndCosmeticOn)
            {
                var availableCosmetics = GameManager.instance.allCosmetics.FindAll(c => !GameManager.instance.playerCosmetics.Contains(c));
                if (availableCosmetics.Count > 0)
                {
                    rewardCosmetic = availableCosmetics[UnityEngine.Random.Range(0, availableCosmetics.Count)];
                }
            }



            TurnOnEndMenu(rewardCosmetic);
            DataCollection.instance.SubmitData();
        }
        else
        {
            TurnOnEndMenu(null);
            Debug.LogWarning("Total song time is zero or negative, cannot complete level.");
        }
    }
    public void OnLevelLose()
    {
        DataCollection.instance.TotalTimePlayed += songTimePlayed;
        AccountUtility.UpdateAccountTimePlayed(songTimePlayed);
        QuestEvents.ProgressQuest.Invoke(QuestRestriction.Melodie, 1);
        QuestEvents.SingleLevelProgress.Invoke(QuestRestriction.PerfectHits, DataCollection.instance.PerfectNotes);
        TurnOnEndMenu(null);

        DataCollection.instance.SubmitData();
    }

    public void TurnOnEndMenu(CosmeticsData cosm)
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
        if (GameManager.instance.questsOn && GameManager.instance.completedQuests > 0)
            GameEndMenu.transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = $"Ukończono zadania: {GameManager.instance.completedQuests}";
        else if(GameManager.instance.questsOn && GameManager.instance.completedQuests <= 0)
            GameEndMenu.transform.GetChild(2).gameObject.SetActive(false);
        else if (GameManager.instance.shopAndCurrencyOn)
            GameEndMenu.transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = $"Zdobyto walutę: 100$";
        else if (GameManager.instance.rewardsAndCosmeticOn)
            GameEndMenu.transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = cosm != null ? $"Zdobyto przedmiot kosmetyczny: {cosm.name}" : "Brak nagrody kosmetycznej";
        else
            GameEndMenu.transform.GetChild(2).gameObject.SetActive(false);
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
            GamePausedMenu.SetActive(true);
        else
            Debug.LogWarning("[GameUIManager] GamePausedMenu is null!");
    }

    public void TurnOffPauseMenu()
    {
        Debug.Log("[GameUIManager] TurnOffPauseMenu called. GamePausedMenu: " + (GamePausedMenu != null));
        if (GamePausedMenu != null)
            GamePausedMenu.SetActive(false);
        else
            Debug.LogWarning("[GameUIManager] GamePausedMenu is null!");
    }
}

//co jest potrzebne �eby zresetowa� poziom:
//totalSongTime
//songTimePlayed
//questscompleted