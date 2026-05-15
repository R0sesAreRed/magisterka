using Melanchall.DryWetMidi.Interaction;
using System;
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
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] Image progressBar;
    [SerializeField] TextMeshProUGUI feedbackText;

    [SerializeField] GameObject[] hearths;

    public GameObject GameEndMenu;
    public GameObject GamePausedMenu;
    private float healthPoints = 3000;
    public FalingNotesSpawner FNS;
    public float HealthPoints
    {
        get { return healthPoints; }
        set
        {
            float clamped = Mathf.Clamp(value, 0, 3000);
            if (Mathf.Approximately(healthPoints, clamped))
                return;

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
                OnLevelEnd();
            }
        }
    }

    public double totalSongTime = 0;
    public double songTimePlayed = 0;
    private int score = 0; //punkty zdobywane w czasie gry
    private double displayScore = 0; //punkty wyœwietlane na ekranie
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
            displayScore += 0.1f;
            scoreText.text = "Score: " + ((int)displayScore).ToString();
        }
        if (!GameManager.instance.IsPaused)
        {
            progressBar.fillAmount = (float)(songTimePlayed / totalSongTime);
            Debug.Log("[GameUIManager] ProgressBar fill: " + progressBar.fillAmount + " songTimePlayed: " + songTimePlayed + " totalSongTime: " + totalSongTime);
        }
        if (songTimePlayed >= totalSongTime + 1500 && !GameEndMenu.activeSelf)
        {
            Debug.Log("[GameUIManager] OnLevelEnd called from Update");
            OnLevelEnd();
        }
    }


    public int CalculateScore(double hitTime, double noteStartTime, double releaseTime, double noteLength, GameManager.NK note) //pobawiæ siê tym ¿eby dobrze czu³o ró¿nice
    {
        double timeDifference = Mathf.Abs((float)(hitTime - noteStartTime));
        double releaseDifference = Mathf.Abs((float)(releaseTime - (noteStartTime + noteLength)));
        Debug.Log($"Nuta {note} ró¿nica czasu: {timeDifference}, ró¿nica release: {releaseDifference}");
        if (timeDifference+releaseDifference <= 0.15f) // idealne trafienie
        {
            //Debug.Log("Perfekcyje trafienie");
            feedbackText.text = "Perfekcyjnie!";
            HealthPoints += 100;
            GameEvents.OnPerfectHit?.Invoke(1);
            GameEvents.OnHit?.Invoke(1);
            return 100;
        }
        else if (timeDifference+releaseDifference <= 0.3f) // dobre trafienie
        {
            //Debug.Log("Dobre trafienie");
            feedbackText.text = "Dobrze!";
            HealthPoints += 70;
            GameEvents.SetAchievementValue?.Invoke(AchievementRestriction.HitQuality, 0);
            GameEvents.OnHit?.Invoke(1);
            return 70;
        }
        else if (timeDifference + releaseDifference <= 0.45f) // s³abe trafienie
        {
            feedbackText.text = "OK";
            //Debug.Log("S³abe trafienie");
            HealthPoints += 50;
            GameEvents.SetAchievementValue?.Invoke(AchievementRestriction.HitQuality, 0);
            GameEvents.OnHit?.Invoke(1);
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
            return 0;  
        }
        
    }

    public void OnLevelEnd()
    {
        GameEndMenu.SetActive(true);

        //achievements, questy i reset czego trzeba
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

//co jest potrzebne ¿eby zresetowaæ poziom:
//totalSongTime
//songTimePlayed