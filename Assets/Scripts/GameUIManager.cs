using Melanchall.DryWetMidi.Interaction;
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
    private float healthPoints = 3000;

    public float HealthPoints
    {
        get { return healthPoints; }
        set
        {
            float clamped = Mathf.Clamp(value, 0, 3000);
            if (Mathf.Approximately(healthPoints, clamped))
                return;

            healthPoints = clamped;
            for (int i = 0; i < hearths.Length; i++)
            {
                float hearthValue = Mathf.Clamp(healthPoints - i * 1000, 0, 1000) / 1000f;

                Transform mask = hearths[i].transform.Find("HearthMask");
                if (mask != null)
                {
                    Transform fill = mask.Find("HearthFill");
                    if (fill != null)
                    {
                        Image fillImage = fill.GetComponent<Image>();
                        if (fillImage != null)
                            fillImage.fillAmount = hearthValue;
                    }
                }
                Transform outline = hearths[i].transform.Find("HearthOutline");
                if (outline != null)
                {
                    outline.gameObject.SetActive(hearthValue >= 1f);
                }
            }
        }
    }

    public double totalSongTime;
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
        if (instance == null && instance != this)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
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
        if(displayScore < score)
        {
            displayScore += 0.1f; //p³ynne zwiêkszanie wyœwietlanego wyniku
            scoreText.text = "Score: " + ((int)displayScore).ToString();
        }
        if(!GameManager.instance.IsPaused)
        {
            progressBar.fillAmount = (float)(songTimePlayed / totalSongTime); //aktualizacja paska postêpu
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
            return 100;
        }
        else if (timeDifference+releaseDifference <= 0.3f) // dobre trafienie
        {
            //Debug.Log("Dobre trafienie");
            feedbackText.text = "Dobrze!";
            HealthPoints += 70;
            return 70;
        }
        else if (timeDifference + releaseDifference <= 0.45f) // s³abe trafienie
        {
            feedbackText.text = "OK";
            //Debug.Log("S³abe trafienie");
            HealthPoints += 50;
            return 50;
        }
        else // nietrafienie
        {
            feedbackText.text = "Nietrafione";
            //Debug.Log("Nietrafienie");
            HealthPoints = Mathf.Round(HealthPoints / 1000) * 1000;
            return 0;  
        }

    }
}
